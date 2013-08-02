using System;
using Xamarin.Auth;

namespace Symplified.Auth
{
	/// <summary>
	/// SAML 1.1 authenticator.
	/// </summary>
	public class SAML11Authenticator : WebRedirectAuthenticator
	{
		public SAML11Authenticator (Uri initialUrl, Uri redirectUrl)
			: base (initialUrl, redirectUrl)
		{
			;
		}

		protected override void OnRedirectPageLoaded (Uri url, System.Collections.Generic.IDictionary<string, string> query, System.Collections.Generic.IDictionary<string, string> fragment, System.Collections.Generic.IDictionary<string,string> formParams)
		{
			base.OnRedirectPageLoaded (url, query, fragment, formParams);
		}
	}
}

