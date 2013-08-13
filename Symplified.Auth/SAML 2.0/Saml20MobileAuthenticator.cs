using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using Xamarin.Auth;
using Xamarin.Utilities;
using dk.nita.saml20;
using dk.nita.saml20.Schema.Core;
using dk.nita.saml20.Schema.Metadata;

namespace Symplified.Auth
{
	/// <summary>
	/// Saml20 mobile authenticator.
	/// </summary>
	public class Saml20MobileAuthenticator
	{
//		public static readonly string USER_AGENT = "Symplified.Auth/1.0 Xamarin.Auth/1.1";

		public static readonly string USER_AGENT = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.8; rv:24.0) Gecko/20100101 Firefox/24.0";

		private List<IDPSSODescriptor> _identityProviders;

		/// <summary>
		/// Initializes a new instance of the <see cref="Symplified.Auth.Saml20MobileAuthenticator"/> class.
		/// </summary>
		public Saml20MobileAuthenticator ()
		{
		}

		public Saml20MobileAuthenticator (List<IDPSSODescriptor> identityProviders) 
		{

		}

		public List<IDPSSODescriptor> IdentityProviders
		{
			get {
				return _identityProviders;
			}
			set {
				_identityProviders = value;
			}
		}

		public IDPSSODescriptor identityProviderFromId (string Id)
		{
			return null;
		}

		public void Login (Saml20AuthnRequest authnRequest, AsyncCallback responseHandler)
		{
			HttpWebRequest hreq = new HttpWebRequest (new Uri("https://idp.symplified.net/IdPServlet?idp_id=1v5uitrsv6u1b"));
			hreq.AllowAutoRedirect = true;
#if PLATFORM_IOS
			hreq.CachePolicy = new RequestCachePolicy (RequestCacheLevel.NoCacheNoStore);			
#endif
			hreq.Method = "POST";
			hreq.UserAgent = USER_AGENT;
			hreq.ContentType = "application/x-www-form-urlencoded";

			byte[] xmlBytes = UTF8Encoding.Default.GetBytes (authnRequest.GetXml ().OuterXml);
			string base64XmlString = SamlAccount.ToBase64ForUrlString (xmlBytes);

			hreq.ContentLength = base64XmlString.Length;

			hreq.GetRequestStream ().Write (
				UTF8Encoding.Default.GetBytes (base64XmlString), 
				0, 
				(int) hreq.ContentLength
				);

			using (HttpWebResponse response = hreq.GetResponse () as HttpWebResponse) {

				string text = response.GetResponseText ();
				Console.WriteLine (text);
			}
		}
	}
}

