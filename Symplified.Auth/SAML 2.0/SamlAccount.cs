using System;
using Xamarin.Auth;
using dk.nita.saml20;

namespace Symplified.Auth
{
	/// <summary>
	/// Saml account.
	/// </summary>
	public class SamlAccount : Account
	{
		private Saml20Assertion _saml20Assertion;

		public SamlAccount () : base () {}

		/// <summary>
		/// Initializes a new instance of the <see cref="Symplified.Auth.SamlAccount"/> class.
		/// </summary>
		/// <param name="assertion">Assertion.</param>
		public SamlAccount (Saml20Assertion assertion)
		{
			this._saml20Assertion = assertion;
			this.Username = assertion.Subject.Value;
		}

		public Saml20Assertion Assertion
		{
			get {
				return _saml20Assertion;
			}
			set {
				_saml20Assertion = value;
				this.Username = _saml20Assertion.Subject.Value;
			}
		}
	}
}

