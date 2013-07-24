using System;
using System.Net;
using MonoTouch.Foundation;
using Symplified.Auth;

namespace Symplified.Auth.iOS
{
	public static class NSHttpCookieExtensions
	{
		public static Cookie ConvertToCLRCookie (this NSHttpCookie self)
		{
			System.Net.Cookie newCookie = new System.Net.Cookie()
			{
				Name = self.Name,
				Value = self.Value,
				Version = (int) self.Version,
				Expires = self.ExpiresDate,
				Domain = self.Domain,
				Path = self.Path,
				Port = self.PortList[0].ToString(),
				Secure = self.IsSecure,
				HttpOnly = self.IsHttpOnly
			};

			return newCookie;
		}
	}
}

