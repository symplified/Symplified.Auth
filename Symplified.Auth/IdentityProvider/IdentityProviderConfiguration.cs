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

	[XmlRoot("IdentityProviderConfiguration", Namespace="http://www.symplified.com", IsNullable=false)]
	public class IdentityProviderConfiguration
	{
		public IdentityProviderConfiguration ()
		{
		}

		[XmlElement(ElementName="Name")]
		public string Name { get; set; }

		[XmlElement(DataType="ID", ElementName="ID")]
		public string Id { get; set; }

		[XmlElement(DataType="anyURI", ElementName="URI")]
		public Uri Uri { get; set; }

		[XmlElement(DataType="string", ElementName="AuthNMethod")]
		public IdentityProviderAuthenticationMethod AuthenticationMethod { get; set; }

		[XmlElement(DataType="base64Binary")]
		public X509Chain CertificateChain { get; set; }

		public static IdentityProviderConfiguration ImportProvider (string xml) {
			return null;
		}

		public static List<IdentityProviderConfiguration> ImportProviderCollection (string xml) {
			return null;
		}
	}
}

