﻿using System;
using System.Xml;
using dk.nita.saml20;
//using dk.nita.saml20.config;
using dk.nita.saml20.Schema.Core;
using dk.nita.saml20.Schema.Protocol;
using dk.nita.saml20.Utils;
using Saml2.Properties;
using dk.nita.saml20.config;

namespace dk.nita.saml20
{
    /// <summary>
    /// Encapsulates the ArtificatResponse schema class
    /// </summary>
    public class Saml20ArtifactResponse
    {
        private ArtifactResponse _artifactResponse;

        /// <summary>
        /// Initializes a new instance of the <see cref="Saml20ArtifactResponse"/> class.
        /// </summary>
        public Saml20ArtifactResponse()
        {
            _artifactResponse = new ArtifactResponse();
            _artifactResponse.Version = Saml20Constants.Version;
            _artifactResponse.ID = "id" + Guid.NewGuid().ToString("N");
            _artifactResponse.Issuer = new NameID();
            _artifactResponse.IssueInstant = DateTime.Now;
            _artifactResponse.Status = new Status();
            _artifactResponse.Status.StatusCode = new StatusCode();
        }

        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        /// <value>The issuer.</value>
        public string Issuer
        {
            get { return _artifactResponse.Issuer.Value; }
            set { _artifactResponse.Issuer.Value = value; }
        }

        /// <summary>
        /// Gets or sets InResponseTo.
        /// </summary>
        /// <value>The in response to.</value>
        public string InResponseTo
        {
            get { return _artifactResponse.InResponseTo; }
            set { _artifactResponse.InResponseTo = value; }
        }

        /// <summary>
        /// Gets or sets the SAML element.
        /// </summary>
        /// <value>The SAML element.</value>
        public XmlElement SamlElement
        {
            get { return _artifactResponse.Any;  }
            set { _artifactResponse.Any = value;  }
        }

        /// <summary>
        /// Gets the ID.
        /// </summary>
        /// <value>The ID.</value>
        public string ID
        {
            get { return _artifactResponse.ID; }
        }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>The status code.</value>
        public string StatusCode
        {
            get { return _artifactResponse.Status.StatusCode.Value; }
            set { _artifactResponse.Status.StatusCode.Value = value; }
        }

        /// <summary>
        /// Returns the ArtifactResponse as an XML document.
        /// </summary>
        public XmlDocument GetXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(Serialization.SerializeToXmlString(_artifactResponse));
            return doc;
        }

        /// <summary>
        /// Gets a default instance of this class with proper values set.
        /// </summary>
        /// <returns></returns>
        public static Saml20ArtifactResponse GetDefault()
        {
            Saml20ArtifactResponse result = new Saml20ArtifactResponse();

            SAML20FederationConfig config = SAML20FederationConfig.GetConfig();

            if (config.ServiceProvider == null || string.IsNullOrEmpty(config.ServiceProvider.ID))
                throw new Saml20FormatException(Resources.ServiceProviderNotSet);

            result.Issuer = config.ServiceProvider.ID;

            return result;
        }
    }
}