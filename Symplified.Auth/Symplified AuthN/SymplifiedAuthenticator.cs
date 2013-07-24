using System;
using System.Collections;
using System.Net;
using Xamarin.Auth;

#if PLATFORM_IOS
using Symplified.Auth.iOS;
using MonoTouch.Foundation;
using PlatformCookie = MonoTouch.Foundation.NSHttpCookie;
#elif PLATFORM_ANDROID

#endif


namespace Symplified.Auth
{
	public class SymplifiedAuthenticator : WebRedirectAuthenticator
	{
		public SymplifiedAuthenticator (Uri initialUrl, Uri redirectUrl)
			:base (initialUrl, redirectUrl)
		{
			;
		}

		protected virtual Cookie[] SymplifiedCookies { get; private set; }

		protected override void OnPageEncountered (Uri url, System.Collections.Generic.IDictionary<string, string> query, System.Collections.Generic.IDictionary<string, string> fragment)
		{
			base.OnPageEncountered (url, query, fragment);
		}

		public override void OnPageLoaded (Uri url)
		{
			base.OnPageLoaded (url);
		}

		public override void OnPageLoading (Uri url)
		{
			base.OnPageLoading (url);
		}

		protected override void OnRedirectPageLoaded (Uri url, System.Collections.Generic.IDictionary<string, string> query, System.Collections.Generic.IDictionary<string, string> fragment)
		{
			object[] platformCookies = null;

#if PLATFORM_IOS
			platformCookies = NSHttpCookieStorage.SharedStorage.Cookies;
#elif PLATFORM_ANDROID
			platformCookies = null;
#endif
			CookieCollection cookieCollection = CookieConverter.ConvertToCLRCookies (platformCookies);
			CookieContainer cc = new CookieContainer (cookieCollection.Count);
			cc.Add (cookieCollection);
			OnSucceeded (new Account ("", cc));
		}
	}
}

