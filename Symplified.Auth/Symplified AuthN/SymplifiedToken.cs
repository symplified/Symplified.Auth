using System;
using System.Net;
using System.Text;

namespace Symplified.Auth
{
	public class SymplifiedToken : IdentityToken
	{
		public static readonly string COOKIE_IDENTIFIER = "singlepoint";

		private Cookie _cookie;

		public SymplifiedToken ()
		{
		}

		public SymplifiedToken (string token) 
			: base(token) 
		{
			this._cookie = new Cookie (COOKIE_IDENTIFIER, token, "", "");
		}

		public SymplifiedToken (Cookie cookie)
		{
			this._cookie = cookie;
		}

		public override string Token {
			get {
				return _cookie.ToString ();
			}
			set {
				_cookie = new Cookie (COOKIE_IDENTIFIER, value, "", "");
			}
		}

		public virtual Cookie ToCookie () {
			return _cookie;
		}

		public override string GetBearerAssertionHeader ()
		{
			Encoding e = new UTF8Encoding ();
			return "Bearer: " + Convert.ToBase64String (e.GetBytes (this.Token));
		}
	}
}

