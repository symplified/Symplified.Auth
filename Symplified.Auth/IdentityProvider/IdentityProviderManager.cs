using System;
using System.Collections.Generic;
using System.Xml;

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

		public void LoadIdentityProviders (XmlElement xml) {

		}

		public void LoadIdentityProviders (string pathname, 
		                                   out List<IdentityProviderConfiguration> idpConfigurations) {
			idpConfigurations = null;
		}
	}
}

