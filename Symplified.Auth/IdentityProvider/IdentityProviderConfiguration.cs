using Mono.Security.X509;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Symplified.Auth
{
	/// <summary>
	/// Identity provider authentication method.
	/// </summary>
	public enum IdentityProviderAuthenticationMethod
	{
		SAML_1_1,
		SAML_2_0,
		SYMPLIFIED,
		SCIM,
		OPENID_CONNECT,
		OPENID,
		OAUTH_1_0a,
		OAUTH_2_0
	}

	/// <summary>
	/// Identity provider configuration.
	/// </summary>
	[XmlRoot("IdentityProviderConfiguration", Namespace="http://www.symplified.com", IsNullable=false)]
	public class IdentityProviderConfiguration
	{
		public IdentityProviderConfiguration ()
		{
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[XmlElement(ElementName="Name")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		[XmlElement(DataType="ID", ElementName="ID")]
		public string Id { get; set; }

		/// <summary>
		/// Gets or sets the URI.
		/// </summary>
		/// <value>The URI.</value>
		[XmlElement(DataType="anyURI", ElementName="URI")]
		public Uri Uri { get; set; }

		/// <summary>
		/// Gets or sets the authentication method.
		/// </summary>
		/// <value>The authentication method.</value>
		[XmlElement(DataType="string", ElementName="AuthNMethod")]
		public IdentityProviderAuthenticationMethod AuthenticationMethod { get; set; }

		/// <summary>
		/// Gets or sets the certificate chain.
		/// </summary>
		/// <value>The certificate chain.</value>
		[XmlElement(DataType="base64Binary")]
		public X509Chain CertificateChain { get; set; }

		/// <summary>
		/// Imports the provider.
		/// </summary>
		/// <returns>The provider.</returns>
		/// <param name="xml">Xml.</param>
		public static IdentityProviderConfiguration ImportProvider (string xml) {
			return null;
		}

		/// <summary>
		/// Imports the provider collection.
		/// </summary>
		/// <returns>The provider collection.</returns>
		/// <param name="xml">Xml.</param>
		public static List<IdentityProviderConfiguration> ImportProviderCollection (string xml) {
			return null;
		}
	}
}

