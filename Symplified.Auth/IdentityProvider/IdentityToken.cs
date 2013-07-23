using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Symplified.Auth
{
	public abstract class IdentityToken
	{
		public IdentityToken () {}

		public IdentityToken (string token) {
			this.Token = token;
		}

		public abstract string Token { get; set; }

		public abstract string GetBearerAssertionHeader ();
	}
}

