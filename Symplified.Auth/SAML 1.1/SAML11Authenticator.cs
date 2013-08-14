using System;
using Xamarin.Auth;

namespace Symplified.Auth
{
	/// <summary>
	/// SAML 1.1 authenticator. (Not Implemented)
	/// </summary>
	public class SAML11Authenticator : WebRedirectAuthenticator
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Symplified.Auth.SAML11Authenticator"/> class.
		/// </summary>
		/// <param name="initialUrl">Initial URL.</param>
		/// <param name="redirectUrl">Redirect URL.</param>
		public SAML11Authenticator (Uri initialUrl, Uri redirectUrl)
			: base (initialUrl, redirectUrl)
		{
			throw new NotImplementedException ();
		}
	}
}

