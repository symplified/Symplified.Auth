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
	[XmlRoot("IdentityProvider", Namespace="http://idp.symplified.com", IsNullable=false)]
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
		/// <value>The URL.</value>
		[XmlElement(DataType="anyURI", ElementName="URL")]
		public Uri URL { get; set; }

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
		public static IdentityProviderConfiguration ImportProvider (XmlElement xml) {
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

		/// <summary>
		/// Imports the provider.
		/// </summary>
		/// <returns>The provider.</returns>
		/// <param name="pathname">Pathname.</param>
		public static IdentityProviderConfiguration ImportProvider (string pathname) {
			return null;
		}

		/// <summary>
		/// Exports the identity provider configuration.
		/// </summary>
		/// <param name="idpConfiguration">Idp configuration.</param>
		public static void ExportProvider (IdentityProviderConfiguration idpConfiguration, out XmlElement xml) {
			xml = null;
		}
	
		/// <summary>
		/// Creates the identity provider.
		/// </summary>
		/// <returns><c>true</c>, if identity provider was created, <c>false</c> otherwise.</returns>
		/// <param name="name">Name.</param>
		/// <param name="id">Identifier.</param>
		/// <param name="url">URL.</param>
		/// <param name="authMethod">Auth method.</param>
		/// <param name="idpConfiguration">Idp configuration.</param>
		public static void CreateIdentityProvider (
			string name,
			string id,
			Uri url,
			IdentityProviderAuthenticationMethod authMethod,
			out IdentityProviderConfiguration idpConfiguration
			)
		{
			idpConfiguration = null;
		}
	}
}

