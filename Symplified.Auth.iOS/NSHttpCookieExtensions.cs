using System;
using System.Net;
using MonoTouch.Foundation;
using Symplified.Auth;

namespace Symplified.Auth.iOS
{
	/// <summary>
	/// NS http cookie extensions.
	/// </summary>
	public static class NSHttpCookieExtensions
	{
		/// <summary>
		/// Converts to CLR cookie.
		/// </summary>
		/// <returns>The to CLR cookie.</returns>
		/// <param name="self">Self.</param>
		public static Cookie ConvertToCLRCookie (this NSHttpCookie self)
		{
			if (self == null) {
				return null;
			}
#if DEBUG
			Console.WriteLine ("Name: {0}", self.Name);
			Console.WriteLine ("Value: {0}", self.Value);
			Console.WriteLine ("Version: {0}", self.Version);
			Console.WriteLine ("ExpiresDate: {0}", self.ExpiresDate);
			Console.WriteLine ("Domain: {0}", self.Domain);
			Console.WriteLine ("Path: {0}", self.Path);
			Console.WriteLine ("IsSecure: {0}", self.IsSecure);
			Console.WriteLine ("IsHttpOnly: {0}", self.IsHttpOnly);
#endif
			System.Net.Cookie newCookie = new System.Net.Cookie()
			{
				Name = self.Name,
				Value = self.Value,
				Version = (int) self.Version,
				Expires = (self.ExpiresDate != null) ? (DateTime)self.ExpiresDate : DateTime.MaxValue,
				Domain = self.Domain,
				Path = self.Path,
//				Port = (self.PortList.Length > 0) ? self.PortList[0].ToString() : null,
				Secure = self.IsSecure,
				HttpOnly = self.IsHttpOnly
			};


			return newCookie;
		}
	}
}

