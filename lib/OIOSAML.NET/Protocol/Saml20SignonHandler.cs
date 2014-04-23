using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Xml;
using dk.nita.saml20.Actions;
using dk.nita.saml20.Bindings;
using dk.nita.saml20.config;
using dk.nita.saml20.Logging;
using dk.nita.saml20.Properties;
using dk.nita.saml20.protocol.pages;
using dk.nita.saml20.Schema.Core;
using dk.nita.saml20.Schema.Metadata;
using dk.nita.saml20.Schema.Protocol;
using dk.nita.saml20.Specification;
using dk.nita.saml20.Utils;
using Saml2.Properties;
using Trace=dk.nita.saml20.Utils.Trace;

namespace dk.nita.saml20.protocol
{
    /// <summary>
    /// Implements a Saml 2.0 protocol sign-on endpoint. Handles all SAML bindings.
    /// </summary>
    public class Saml20SignonHandler : Saml20AbstractEndpointHandler
    {
        private readonly X509Certificate2  _certificate;

        /// <summary>
        /// Session key used to save the current message id with the purpose of preventing replay attacks
        /// </summary>
        public const string ExpectedInResponseToSessionKey = "ExpectedInResponseTo";

        /// <summary>
        /// Initializes a new instance of the <see cref="Saml20SignonHandler"/> class.
        /// </summary>
        public Saml20SignonHandler()
        {
            _certificate = FederationConfig.GetConfig().SigningCertificate.GetCertificate();

            // Read the proper redirect url from config
            try
            {
                RedirectUrl = SAML20FederationConfig.GetConfig().ServiceProvider.SignOnEndpoint.RedirectUrl;
                ErrorBehaviour = SAML20FederationConfig.GetConfig().ServiceProvider.SignOnEndpoint.ErrorBehaviour.ToString();
            }
            catch(Exception e)
            {
                if (Trace.ShouldTrace(TraceEventType.Error))
                    Trace.TraceData(TraceEventType.Error, e.ToString());
            }
        }

        #region IHttpHandler Members

        /// <summary>
        /// Handles a request.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void Handle(HttpContext context)
        {
            Trace.TraceMethodCalled(GetType(), "Handle()");

            //Some IdP's are known to fail to set an actual value in the SOAPAction header
            //so we just check for the existence of the header field.
            if (Array.Exists(context.Request.Headers.AllKeys, delegate(string s) { return s == SOAPConstants.SOAPAction; }))
            {
                HandleSOAP(context, context.Request.InputStream);
                return;
            }

            if (!string.IsNullOrEmpty(context.Request.Params["SAMLart"]))
            {
                HandleArtifact(context);
            }

            if (!string.IsNullOrEmpty(context.Request.Params["SamlResponse"]))
            {
                HandleResponse(context);
            }
            else
            {
                if (SAML20FederationConfig.GetConfig().CommonDomain.Enabled && context.Request.QueryString["r"] == null
                    && context.Request.Params["cidp"] == null)
                {
                    AuditLogging.logEntry(Direction.OUT, Operation.DISCOVER, "Redirecting to Common Domain for IDP discovery");
                    context.Response.Redirect(SAML20FederationConfig.GetConfig().CommonDomain.LocalReaderEndpoint);
                }
                else
                {
                    AuditLogging.logEntry(Direction.IN, Operation.ACCESS,
                                                 "User accessing resource: " + context.Request.RawUrl +
                                                 " without authentication.");
                    SendRequest(context);
                }
            }
        }
                
        #endregion

        private void HandleArtifact(HttpContext context)
        {
            HttpArtifactBindingBuilder builder = new HttpArtifactBindingBuilder(context);
            Stream inputStream = builder.ResolveArtifact();
            HandleSOAP(context, inputStream);
        }

        private void HandleSOAP(HttpContext context, Stream inputStream)
        {
            Trace.TraceMethodCalled(GetType(), "HandleSOAP");
            HttpArtifactBindingParser parser = new HttpArtifactBindingParser(inputStream);
            HttpArtifactBindingBuilder builder = new HttpArtifactBindingBuilder(context);

            if(parser.IsArtifactResolve())
            {
                Trace.TraceData(TraceEventType.Information, Tracing.ArtifactResolveIn);

                IDPEndPoint idp = RetrieveIDPConfiguration(parser.Issuer);
                AuditLogging.IdpId = idp.Id;
                AuditLogging.AssertionId = parser.ArtifactResolve.ID;
                if (!parser.CheckSamlMessageSignature(idp.metadata.Keys))
                {
                    HandleError(context, "Invalid Saml message signature");
                    AuditLogging.logEntry(Direction.IN, Operation.ARTIFACTRESOLVE, "Could not verify signature", parser.SamlMessage);
                };
                builder.RespondToArtifactResolve(parser.ArtifactResolve);
            }else if(parser.IsArtifactResponse())
            {
                Trace.TraceData(TraceEventType.Information, Tracing.ArtifactResponseIn);

                Status status = parser.ArtifactResponse.Status;
                if (status.StatusCode.Value != Saml20Constants.StatusCodes.Success)
                {
                    HandleError(context, status);
                    AuditLogging.logEntry(Direction.IN, Operation.ARTIFACTRESOLVE, string.Format("Illegal status for ArtifactResponse {0} expected 'Success', msg: {1}", status.StatusCode.Value, parser.SamlMessage));
                    return;
                }
                if(parser.ArtifactResponse.Any.LocalName == Response.ELEMENT_NAME)
                {
                    bool isEncrypted;
                    XmlElement assertion = GetAssertion(parser.ArtifactResponse.Any, out isEncrypted);
                    if (assertion == null)
                        HandleError(context, "Missing assertion");
                    if(isEncrypted)
                    {
                        HandleEncryptedAssertion(context, assertion);
                    }else
                    {
                        HandleAssertion(context, assertion);
                    }

                }else
                {
                    AuditLogging.logEntry(Direction.IN, Operation.ARTIFACTRESOLVE, string.Format("Unsupported payload message in ArtifactResponse: {0}, msg: {1}", parser.ArtifactResponse.Any.LocalName, parser.SamlMessage));
                    HandleError(context,
                                string.Format("Unsupported payload message in ArtifactResponse: {0}",
                                              parser.ArtifactResponse.Any.LocalName));
                }
            }else
            {
                Status s = parser.GetStatus();
                if (s != null)
                {
                    HandleError(context, s);
                }
                else
                {
                    AuditLogging.logEntry(Direction.IN, Operation.ARTIFACTRESOLVE, string.Format("Unsupported SamlMessage element: {0}, msg: {1}", parser.SamlMessageName, parser.SamlMessage));
                    HandleError(context, string.Format("Unsupported SamlMessage element: {0}", parser.SamlMessageName));
                }
            }
        }

        /// <summary>
        /// Send an authentication request to the IDP.
        /// </summary>
        private void SendRequest(HttpContext context)
        {
            Trace.TraceMethodCalled(GetType(), "SendRequest()");

            // See if the "ReturnUrl" - parameter is set.
            string returnUrl = context.Request.QueryString["ReturnUrl"];
            if (!string.IsNullOrEmpty(returnUrl))            
                context.Session["RedirectUrl"] = returnUrl;            

            IDPEndPoint idpEndpoint = RetrieveIDP(context);

            if (idpEndpoint == null)
            {
                //Display a page to the user where she can pick the IDP
                SelectSaml20IDP page = new SelectSaml20IDP();
                page.ProcessRequest(context);
                return;
            }

			var authnRequest = Saml20AuthnRequest.GetDefault (idpEndpoint.Name);
            TransferClient(idpEndpoint, authnRequest, context);            
        }


        private Status GetStatusElement(XmlDocument doc)
        {
            XmlElement statElem =
                (XmlElement)doc.GetElementsByTagName(Status.ELEMENT_NAME, Saml20Constants.PROTOCOL)[0];

            return Serialization.DeserializeFromXmlString<Status>(statElem.OuterXml);
        }

        internal static XmlElement GetAssertion(XmlElement el, out bool isEncrypted)
        {
            
            XmlNodeList encryptedList =
                el.GetElementsByTagName(EncryptedAssertion.ELEMENT_NAME, Saml20Constants.ASSERTION);

            if (encryptedList.Count == 1)
            {
                isEncrypted = true;
                return (XmlElement)encryptedList[0];
            }

            XmlNodeList assertionList =
                el.GetElementsByTagName(Assertion.ELEMENT_NAME, Saml20Constants.ASSERTION);

            if (assertionList.Count == 1)
            {
                isEncrypted = false;
                return (XmlElement)assertionList[0];
            }

            isEncrypted = false;
            return null;
        }

        /// <summary>
        /// Handle the authentication response from the IDP.
        /// </summary>        
        private void HandleResponse(HttpContext context)
        {
            Encoding defaultEncoding = Encoding.UTF8;
            XmlDocument doc = GetDecodedSamlResponse(context, defaultEncoding);

            try
            {

                XmlAttribute inResponseToAttribute =
                    doc.DocumentElement.Attributes["InResponseTo"];

                if(inResponseToAttribute == null)
                    throw new Saml20Exception("Received a response message that did not contain an InResponseTo attribute");

                string inResponseTo = inResponseToAttribute.Value;

                CheckReplayAttack(context, inResponseTo);
                
                Status status = GetStatusElement(doc);

                if (status.StatusCode.Value != Saml20Constants.StatusCodes.Success)
                {
                    if (status.StatusCode.Value == Saml20Constants.StatusCodes.NoPassive)
                        HandleError(context, "IdP responded with statuscode NoPassive. A user cannot be signed in with the IsPassiveFlag set when the user does not have a session with the IdP.");

                    HandleError(context, status);
                    return;
                }

                // Determine whether the assertion should be decrypted before being validated.
            
                bool isEncrypted;
                XmlElement assertion = GetAssertion(doc.DocumentElement, out isEncrypted);
                if (isEncrypted)
                {
                    assertion = GetDecryptedAssertion(assertion).Assertion.DocumentElement;
                }

                // Check if an encoding-override exists for the IdP endpoint in question
                string issuer = GetIssuer(assertion);
                IDPEndPoint endpoint = RetrieveIDPConfiguration(issuer);
                if (!string.IsNullOrEmpty(endpoint.ResponseEncoding))
                {
                    Encoding encodingOverride = null;
                    try
                    {
                        encodingOverride = System.Text.Encoding.GetEncoding(endpoint.ResponseEncoding);
                    }
                    catch (ArgumentException ex)
                    {
                        HandleError(context, ex);
                        return;
                    }

                    if (encodingOverride.CodePage != defaultEncoding.CodePage)
                    {
                        XmlDocument doc1 = GetDecodedSamlResponse(context, encodingOverride);
                        assertion = GetAssertion(doc1.DocumentElement, out isEncrypted);
                    }
                }

                HandleAssertion(context, assertion);
                return;
            }
            catch (Exception e)
            {
                HandleError(context, e);
                return;
            }
        }

        private static void CheckReplayAttack(HttpContext context, string inResponseTo)
        {
            var expectedInResponseToSessionState = context.Session[ExpectedInResponseToSessionKey];
            if (expectedInResponseToSessionState == null)
                throw new Saml20Exception("Your session has been disconnected, please logon again");

            string expectedInResponseTo = expectedInResponseToSessionState.ToString();
            if (string.IsNullOrEmpty(expectedInResponseTo) || string.IsNullOrEmpty(inResponseTo))
                throw new Saml20Exception("Empty protocol message id is not allowed.");

            if (inResponseTo != expectedInResponseTo)
            {
                throw new Saml20Exception("Replay attack.");
            }

         }

        private static XmlDocument GetDecodedSamlResponse(HttpContext context, Encoding encoding)
        {
            string base64 = context.Request.Params["SAMLResponse"];

            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            string samlResponse = encoding.GetString(Convert.FromBase64String(base64));
            if (Trace.ShouldTrace(TraceEventType.Information))
                Trace.TraceData(TraceEventType.Information, "Decoded SAMLResponse", samlResponse);

            doc.LoadXml(samlResponse);
            return doc;
        }

        /// <summary>
        /// Decrypts an encrypted assertion, and sends the result to the HandleAssertion method.
        /// </summary>
        private void HandleEncryptedAssertion(HttpContext context, XmlElement elem)
        {
            Trace.TraceMethodCalled(GetType(), "HandleEncryptedAssertion()");
            Saml20EncryptedAssertion decryptedAssertion = GetDecryptedAssertion(elem);
            HandleAssertion(context, decryptedAssertion.Assertion.DocumentElement);
        }

        private static Saml20EncryptedAssertion GetDecryptedAssertion(XmlElement elem)
        {
            Saml20EncryptedAssertion decryptedAssertion = new Saml20EncryptedAssertion((RSA)FederationConfig.GetConfig().SigningCertificate.GetCertificate().PrivateKey);
            decryptedAssertion.LoadXml(elem);
            decryptedAssertion.Decrypt();
            return decryptedAssertion;
        }

        /// <summary>
        /// Retrieves the name of the issuer from an XmlElement containing an assertion.
        /// </summary>
        /// <param name="assertion">An XmlElement containing an assertion</param>
        /// <returns>The identifier of the Issuer</returns>
        private string GetIssuer(XmlElement assertion)
        {
            string result = string.Empty;
            XmlNodeList list = assertion.GetElementsByTagName("Issuer", Saml20Constants.ASSERTION);
            if (list.Count > 0)
            {
                XmlElement issuer = (XmlElement) list[0];
                result = issuer.InnerText;
            }

            return result;
        }

        /// <summary>
        /// Is called before the assertion is made into a strongly typed representation
        /// </summary>
        /// <param name="context">The httpcontext.</param>
        /// <param name="elem">The assertion element.</param>
        /// <param name="endpoint">The endpoint.</param>
        protected virtual void PreHandleAssertion(HttpContext context, XmlElement elem, IDPEndPoint endpoint)
        {
            Trace.TraceMethodCalled(GetType(), "PreHandleAssertion");

            if (endpoint != null && endpoint.SLOEndpoint != null && !String.IsNullOrEmpty(endpoint.SLOEndpoint.IdpTokenAccessor))
            {
                ISaml20IdpTokenAccessor idpTokenAccessor =
                    Activator.CreateInstance(Type.GetType(endpoint.SLOEndpoint.IdpTokenAccessor, false)) as ISaml20IdpTokenAccessor;
                if (idpTokenAccessor != null)
                    idpTokenAccessor.ReadToken(elem);
            }

            Trace.TraceMethodDone(GetType(), "PreHandleAssertion");
        }

        /// <summary>
        /// Deserializes an assertion, verifies its signature and logs in the user if the assertion is valid.
        /// </summary>
        private void HandleAssertion(HttpContext context, XmlElement elem)
        {
            Trace.TraceMethodCalled(GetType(), "HandleAssertion");

            string issuer = GetIssuer(elem);
            
            IDPEndPoint endp = RetrieveIDPConfiguration(issuer);

            AuditLogging.IdpId = endp.Id;

            PreHandleAssertion(context, elem, endp);

            bool quirksMode = false;

            if (endp != null)
            {
                quirksMode = endp.QuirksMode;
            }
            
            Saml20Assertion assertion = new Saml20Assertion(elem, null, quirksMode);
                        
            if (endp == null || endp.metadata == null)
            {
                AuditLogging.logEntry(Direction.IN, Operation.AUTHNREQUEST_POST,
                          "Unknown login IDP, assertion: " + elem);

                HandleError(context, Resources.UnknownLoginIDP);
                return;
            }

            if (!endp.OmitAssertionSignatureCheck)
            {
                if (!assertion.CheckSignature(GetTrustedSigners(endp.metadata.GetKeys(KeyTypes.signing), endp)))
                {
                    AuditLogging.logEntry(Direction.IN, Operation.AUTHNREQUEST_POST,
                    "Invalid signature, assertion: " + elem);

                    HandleError(context, Resources.SignatureInvalid);
                    return;
                }
            }

            if (assertion.IsExpired())
            {
                AuditLogging.logEntry(Direction.IN, Operation.AUTHNREQUEST_POST,
                "Assertion expired, assertion: " + elem);

                HandleError(context, Resources.AssertionExpired);
                return;
            }

            CheckConditions(context, assertion);
            AuditLogging.AssertionId = assertion.Id;
            AuditLogging.logEntry(Direction.IN, Operation.AUTHNREQUEST_POST,
                      "Assertion validated succesfully");

            DoLogin(context, assertion);
        }

        internal static IEnumerable<AsymmetricAlgorithm> GetTrustedSigners(ICollection<KeyDescriptor> keys, IDPEndPoint ep)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");

            List<AsymmetricAlgorithm> result = new List<AsymmetricAlgorithm>(keys.Count);
            foreach (KeyDescriptor keyDescriptor in keys)
            {
                KeyInfo ki = (KeyInfo) keyDescriptor.KeyInfo;
                    
                foreach (KeyInfoClause clause in ki)
                {
                    if(clause is KeyInfoX509Data)
                    {
                        X509Certificate2 cert = XmlSignatureUtils.GetCertificateFromKeyInfo((KeyInfoX509Data) clause);

                        if (!IsSatisfiedByAllSpecifications(ep, cert))
                            continue;
                    }

                    AsymmetricAlgorithm key = XmlSignatureUtils.ExtractKey(clause);
                    result.Add(key);
                }
                
            }

            return result;
        }

        private static bool IsSatisfiedByAllSpecifications(IDPEndPoint ep, X509Certificate2 cert)
        {
            foreach(ICertificateSpecification spec in SpecificationFactory.GetCertificateSpecifications(ep))
            {
                if (!spec.IsSatisfiedBy(cert))
                    return false;   
            }

            return true;
        }


        private void CheckConditions(HttpContext context, Saml20Assertion assertion)
        {
            if(assertion.IsOneTimeUse)
            {
                if (context.Cache[assertion.Id] != null)
                {
                    HandleError(context, Resources.OneTimeUseReplay);
                }else
                {
                    context.Cache.Insert(assertion.Id, string.Empty, null, assertion.NotOnOrAfter, Cache.NoSlidingExpiration);
                }
            }
        }

        private void DoLogin(HttpContext context, Saml20Assertion assertion)
        {
            //User is now logged in at IDP specified in tmp
            context.Session[IDPLoginSessionKey] = context.Session[IDPTempSessionKey];
            context.Session[IDPSessionIdKey] = assertion.SessionIndex;
            context.Session[IDPNameIdFormat] = assertion.Subject.Format;

            if(Trace.ShouldTrace(TraceEventType.Information))
            {
                Trace.TraceData(TraceEventType.Information, string.Format(Tracing.Login, assertion.Subject.Value, assertion.SessionIndex, assertion.Subject.Format));
            }
            AuditLogging.logEntry(Direction.IN, Operation.LOGIN, "Subject: " + assertion.Subject.Value + " NameIDFormat: " + assertion.Subject.Format);


            foreach(IAction action in Actions.Actions.GetActions())
            {
                Trace.TraceMethodCalled(action.GetType(), "LoginAction()");

                action.LoginAction(this, context, assertion);
                
                Trace.TraceMethodDone(action.GetType(), "LoginAction()");
            }
        }

        private void TransferClient(IDPEndPoint idpEndpoint, Saml20AuthnRequest request, HttpContext context)
        {
            AuditLogging.AssertionId = request.ID;
            AuditLogging.IdpId = idpEndpoint.Id;

            //Set the last IDP we attempted to login at.
            context.Session[IDPTempSessionKey]= idpEndpoint.Id;

            // Determine which endpoint to use from the configuration file or the endpoint metadata.
            IDPEndPointElement destination = 
                DetermineEndpointConfiguration(SAMLBinding.REDIRECT, idpEndpoint.SSOEndpoint, idpEndpoint.metadata.SSOEndpoints());

 
    
            request.Destination = destination.Url;

            if (idpEndpoint.ForceAuthn)
                request.ForceAuthn = true;

            object isPassiveFlag = context.Session[IDPIsPassive];

            if (isPassiveFlag != null && (bool)isPassiveFlag)
            {
                request.IsPassive = true;
                context.Session[IDPIsPassive] = null;
            }

            if (idpEndpoint.IsPassive)
                request.IsPassive = true;

            object forceAuthnFlag = context.Session[IDPForceAuthn];

            if (forceAuthnFlag != null && (bool)forceAuthnFlag)
            {
                request.ForceAuthn = true;
                context.Session[IDPForceAuthn] = null;
            }

            if (idpEndpoint.SSOEndpoint != null)
            {
                if (!string.IsNullOrEmpty(idpEndpoint.SSOEndpoint.ForceProtocolBinding))
                {
                    request.ProtocolBinding = idpEndpoint.SSOEndpoint.ForceProtocolBinding;
                }
            }

            //Save request message id to session
            context.Session.Add(ExpectedInResponseToSessionKey, request.ID);

            if (destination.Binding == SAMLBinding.REDIRECT)
            {
                Trace.TraceData(TraceEventType.Information, string.Format(Tracing.SendAuthnRequest, Saml20Constants.ProtocolBindings.HTTP_Redirect, idpEndpoint.Id));
                
                HttpRedirectBindingBuilder builder = new HttpRedirectBindingBuilder();
                builder.signingKey = _certificate.PrivateKey;
                builder.Request = request.GetXml().OuterXml;
                string s = request.Destination + "?" + builder.ToQuery();

                AuditLogging.logEntry(Direction.OUT, Operation.AUTHNREQUEST_REDIRECT, "Redirecting user to IdP for authentication", builder.Request);

                context.Response.Redirect(s, true);
                return;
            }

            if (destination.Binding == SAMLBinding.POST)
            {
                Trace.TraceData(TraceEventType.Information, string.Format(Tracing.SendAuthnRequest, Saml20Constants.ProtocolBindings.HTTP_Post, idpEndpoint.Id));

                HttpPostBindingBuilder builder = new HttpPostBindingBuilder(destination);
                //Honor the ForceProtocolBinding and only set this if it's not already set
                if (string.IsNullOrEmpty(request.ProtocolBinding))
                    request.ProtocolBinding = Saml20Constants.ProtocolBindings.HTTP_Post;
                XmlDocument req = request.GetXml();
                XmlSignatureUtils.SignDocument(req, request.ID);
                builder.Request = req.OuterXml;
                AuditLogging.logEntry(Direction.OUT, Operation.AUTHNREQUEST_POST);

                builder.GetPage().ProcessRequest(context);
                return;
            }

            if(destination.Binding == SAMLBinding.ARTIFACT)
            {
                Trace.TraceData(TraceEventType.Information, string.Format(Tracing.SendAuthnRequest, Saml20Constants.ProtocolBindings.HTTP_Artifact, idpEndpoint.Id));

                HttpArtifactBindingBuilder builder = new HttpArtifactBindingBuilder(context);
                //Honor the ForceProtocolBinding and only set this if it's not already set
                if(string.IsNullOrEmpty(request.ProtocolBinding))
                    request.ProtocolBinding = Saml20Constants.ProtocolBindings.HTTP_Artifact;
                AuditLogging.logEntry(Direction.OUT, Operation.AUTHNREQUEST_REDIRECT_ARTIFACT);

                builder.RedirectFromLogin(destination, request);
            }

            HandleError(context, Resources.BindingError);
        }

    }
}
