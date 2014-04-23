using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Auth;

using dk.nita.saml20;
using dk.nita.saml20.Schema.Core;
using dk.nita.saml20.Validation;
using dk.nita.saml20.Schema.Metadata;
using dk.nita.saml20.Utils;
using dk.nita.saml20.config;
using dk.nita.saml20.Schema.XmlDSig;
using dk.nita.saml20.Bindings;

namespace Symplified.Auth
{
	/// <summary>
	/// SAML 2.0 Authenticator.
	/// </summary>
	public class Saml20Authenticator : WebRedirectAuthenticator
	{
		private Saml20MetadataDocument _idpMetadata;

		private string _spName;

		private static readonly Uri PLACEHOLDER_URI = new Uri ("http://example.com" + new Guid ());

		/// <summary>
		/// Initializes a new instance of the <see cref="Symplified.Auth.Saml20Authenticator"/> class.
		/// </summary>
		/// <param name="spName">Service Provider name.</param>
		/// <param name="idpMetadata">Identity Provider metadata.</param>
		public Saml20Authenticator (string spName, Saml20MetadataDocument idpMetadata) :
			base (PLACEHOLDER_URI, PLACEHOLDER_URI)
		{
			_spName = (string.IsNullOrEmpty (spName)) ? "symplified-mobile-sp" : spName;
			_idpMetadata = idpMetadata;

			var url = _idpMetadata.SSOEndpoint (SAMLBinding.POST).Url;
			var separator = url.Contains ("?") ? "&" : "?";

			var authnRequest = Saml20AuthnRequest.GetDefault (_spName);

			var builder = new HttpRedirectBindingBuilder ();
			builder.Request = authnRequest.GetXml ().OuterXml;

			initialUrl = new Uri (
				String.Format (
					"{0}{1}{2}", url, separator, builder.ToQuery()
				)
			);
		}

		/// <summary>
		/// Event handler called when a new page is being loaded in the web browser.
		/// </summary>
		/// <param name="url">The URL of the page.</param>
		/// <param name="formParams">Form parameters.</param>
		public override void OnPageLoading (Uri url, IDictionary<string,string> formParams)
		{
			if (formParams != null && formParams.ContainsKey ("SAMLResponse")) {
				OnRedirectPageLoaded (url, null, null, formParams);
			} else {
				base.OnPageLoading (url, formParams);
			}
		}

		/// <summary>
		/// Raised when the SAML 2.0 response parameter has been detected.
		/// </summary>
		/// <param name="url">URL of the page.</param>
		/// <param name="query">The parsed query of the URL.</param>
		/// <param name="fragment">The parsed fragment of the URL.</param>
		/// <param name="formParams">Form parameters, including the 'SAMLResponse'.</param>
		protected override void OnRedirectPageLoaded (Uri url, System.Collections.Generic.IDictionary<string, string> query, System.Collections.Generic.IDictionary<string, string> fragment, IDictionary<string, string> formParams)
		{
			string base64SamlAssertion = formParams.ContainsKey ("SAMLResponse") ? formParams ["SAMLResponse"] : string.Empty;
			byte[] xmlSamlAssertionBytes = Convert.FromBase64String (base64SamlAssertion);
			string xmlSamlAssertion = System.Text.UTF8Encoding.Default.GetString (xmlSamlAssertionBytes);

			XmlDocument xDoc = new XmlDocument ();
			xDoc.PreserveWhitespace = true;
			xDoc.LoadXml (xmlSamlAssertion);
		
			XmlElement responseElement = (XmlElement)xDoc.SelectSingleNode ("//*[local-name()='Response']");
#if DEBUG
			Console.WriteLine ("{0}", responseElement.OuterXml);
#endif

			XmlElement assertionElement = (XmlElement)xDoc.SelectSingleNode ("//*[local-name()='Assertion']");
			if (assertionElement != null) {
#if DEBUG
				Console.WriteLine ("{0}", assertionElement.OuterXml);
#endif
				Saml20Assertion samlAssertion = new Saml20Assertion (assertionElement, null, AssertionProfile.Core, false, false);
				List<AsymmetricAlgorithm> trustedIssuers = new List<AsymmetricAlgorithm>(1);

				foreach (KeyDescriptor key in _idpMetadata.Keys)
				{
					System.Security.Cryptography.Xml.KeyInfo ki = 
						(System.Security.Cryptography.Xml.KeyInfo) key.KeyInfo;
					foreach (KeyInfoClause clause in ki)
					{
						AsymmetricAlgorithm aa = XmlSignatureUtils.ExtractKey(clause);
						trustedIssuers.Add(aa);
					}
				}

				try {
					samlAssertion.CheckValid (trustedIssuers);
					SamlAccount sa = new SamlAccount (samlAssertion, responseElement);
					OnSucceeded (sa);
				}
				catch (Saml20Exception samlEx) {
					Console.WriteLine (samlEx);
					OnError (samlEx.Message);
				}
				catch (Exception ex) {
					Console.WriteLine (ex);
					OnError (ex.Message);
				}
			}
			else {
				OnError ("No SAML Assertion Found");                                                                                                                                                                          ;
			}
		}
	}
}

