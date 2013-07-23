using System;
using Xamarin.Auth;

namespace Symplified.Auth
{
	public class SAML11Authenticator : WebRedirectAuthenticator
	{
		public SAML11Authenticator (Uri initialUrl, Uri redirectUrl)
			: base (initialUrl, redirectUrl)
		{
			;
		}
	}
}

