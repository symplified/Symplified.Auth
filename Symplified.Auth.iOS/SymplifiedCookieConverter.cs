using System;
using System.Collections;
using System.Net;
using MonoTouch.Foundation;

namespace Symplified.Auth
{
	public class SymplifiedCookieConverter : CookieConverter
	{
		public SymplifiedCookieConverter () {}

		public Cookie[] ConvertToCLRCookies (object[] platformCookies)
		{
			ArrayList clrCookies = new ArrayList (platformCookies.Length);

			foreach (NSHttpCookie cookie in platformCookies) {
				clrCookies.Add (cookie.ConvertToCLRCookie ());
			}

			return (clrCookies.Count > 0) ? (Cookie[])clrCookies.ToArray () : null;
		}
	}
}

