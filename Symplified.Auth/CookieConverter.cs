using System;
using System.Collections;
using System.Net;

#if PLATFORM_IOS
using Symplified.Auth.iOS;
using PlatformCookie = MonoTouch.Foundation.NSHttpCookie;
#elif PLATFORM_ANDROID
//using Symplified.Auth.Android;
using PlatformCookie = Org.Apache.Http.Cookies.IClientCookie;
#endif

namespace Symplified.Auth
{
	/// <summary>
	/// Cookie converter.
	/// </summary>
	public class CookieConverter
	{
		/// <summary>
		/// Converts to CLR cookies.
		/// </summary>
		/// <returns>The to CLR cookies.</returns>
		/// <param name="platformCookies">Platform cookies.</param>
		public static CookieCollection ConvertToCLRCookies (object [] platformCookies)
		{
			CookieCollection clrCookies = new CookieCollection ();
#if PLATFORM_IOS
			foreach (PlatformCookie cookie in platformCookies) {
				clrCookies.Add (cookie.ConvertToCLRCookie ());
			}
#elif PLATFORM_ANDROID
			throw new NotImplementedException ();
#endif
			return (clrCookies.Count > 0) ? clrCookies : null;
		}
	}
}

