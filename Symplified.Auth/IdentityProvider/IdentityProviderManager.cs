using System;

namespace Symplified.Auth
{
	public class IdentityProviderManager
	{
		private static readonly IdentityProviderManager _instance = new IdentityProviderManager ();

		private IdentityProviderManager (){}

		public static IdentityProviderManager Instance {
			get {
				return _instance;
			}
		}

		public void LoadIdentityProviders (string xml) {

		}
	}
}

