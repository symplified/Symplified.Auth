using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Symplified.Auth
{
	/// <summary>
	/// Identity token.
	/// </summary>
	public abstract class IdentityToken
	{
		public IdentityToken () {}

		/// <summary>
		/// Initializes a new instance of the <see cref="Symplified.Auth.IdentityToken"/> class.
		/// </summary>
		/// <param name="token">Token.</param>
		public IdentityToken (string token) {
			this.Token = token;
		}

		/// <summary>
		/// Gets or sets the token.
		/// </summary>
		/// <value>The token.</value>
		public abstract string Token { get; set; }

		/// <summary>
		/// Gets the bearer assertion header.
		/// </summary>
		/// <returns>The bearer assertion header.</returns>
		public abstract string GetBearerAssertionHeader ();
	}
}

