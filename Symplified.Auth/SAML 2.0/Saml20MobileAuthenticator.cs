using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Auth;
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
			HttpWebRequest hreq = new HttpWebRequest (new Uri ("google.com"));
			hreq.AllowAutoRedirect = true;
			hreq.CachePolicy = new RequestCachePolicy (RequestCacheLevel.NoCacheNoStore);			
			hreq.Method = "POST";
			hreq.UserAgent = "Symplified Mobile SDK 1.0";
		}
	}
}

