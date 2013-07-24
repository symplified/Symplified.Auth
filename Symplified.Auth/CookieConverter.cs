using System;
using System.Collections;
using System.Net;

#if PLATFORM_IOS
using Symplified.Auth.iOS;
using PlatformCookie = MonoTouch.Foundation.NSHttpCookie;
#elif PLATFORM_ANDROID

#endif

namespace Symplified.Auth
{
	public class CookieConverter
	{
		public static CookieCollection ConvertToCLRCookies (object [] platformCookies)
		{
			CookieCollection clrCookies = new CookieCollection ();

			foreach (PlatformCookie cookie in platformCookies) {
				clrCookies.Add (cookie.ConvertToCLRCookie ());
			}

			return (clrCookies.Count > 0) ? clrCookies : null;
		}
	}
}

