using System;
using Xamarin.Auth;

namespace Symplified.Auth
{
	public class SymplifiedAuthenticator : WebRedirectAuthenticator
	{
		public SymplifiedAuthenticator (Uri initialUrl, Uri redirectUrl)
			:base (initialUrl, redirectUrl)
		{
			;
		}
	}
}

