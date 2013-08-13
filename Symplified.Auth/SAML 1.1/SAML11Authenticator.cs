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
			throw new NotImplementedException ();
		}
	}
}

