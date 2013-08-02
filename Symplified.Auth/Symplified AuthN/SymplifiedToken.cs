using System;
using System.Net;
using System.Text;

namespace Symplified.Auth
{
	/// <summary>
	/// Symplified token.
	/// </summary>
	public class SymplifiedToken : IdentityToken
	{
		/// <summary>
		/// The Symplified cookie name.
		/// </summary>
		public static readonly string COOKIE_IDENTIFIER = "singlepoint";

		private Cookie _cookie;

		public SymplifiedToken ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Symplified.Auth.SymplifiedToken"/> class.
		/// </summary>
		/// <param name="token">Token.</param>
		public SymplifiedToken (string token) 
			: base(token) 
		{
			this._cookie = new Cookie (COOKIE_IDENTIFIER, token, "", "");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Symplified.Auth.SymplifiedToken"/> class.
		/// </summary>
		/// <param name="cookie">Cookie.</param>
		public SymplifiedToken (Cookie cookie)
		{
			this._cookie = cookie;
		}

		/// <summary>
		/// Gets or sets the token.
		/// </summary>
		/// <value>The token.</value>
		public override string Token {
			get {
				return _cookie.ToString ();
			}
			set {
				_cookie = new Cookie (COOKIE_IDENTIFIER, value, "", "");
			}
		}

		/// <summary>
		/// Tos the cookie.
		/// </summary>
		/// <returns>The cookie.</returns>
		public virtual Cookie ToCookie () {
			return _cookie;
		}

		/// <summary>
		/// Gets the bearer assertion header.
		/// </summary>
		/// <returns>The bearer assertion header.</returns>
		public override string GetBearerAssertionHeader ()
		{
			Encoding e = new UTF8Encoding ();
			return "Bearer: " + Convert.ToBase64String (e.GetBytes (this.Token));
		}
	}
}

