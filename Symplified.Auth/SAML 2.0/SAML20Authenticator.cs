using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Auth;

namespace Symplified.Auth
{
	public class SAML20Authenticator : WebRedirectAuthenticator
	{
		public SAML20Authenticator (Uri initialUrl, Uri redirectUrl)
			: base (initialUrl, redirectUrl)
		{

		}

		public override Task<Uri> GetInitialUrlAsync ()
		{
			return base.GetInitialUrlAsync ();
		}

		public override void OnPageLoading (Uri url)
		{
			base.OnPageLoading (url);
		}

		protected override void OnPageEncountered (Uri url, IDictionary<string, string> query, IDictionary<string, string> fragment)
		{
			base.OnPageEncountered (url, query, fragment);
		}
		protected override void OnRedirectPageLoaded (Uri url, System.Collections.Generic.IDictionary<string, string> query, System.Collections.Generic.IDictionary<string, string> fragment)
		{
			base.OnRedirectPageLoaded (url, query, fragment);
		}

		protected override void OnBrowsingCompleted ()
		{
			base.OnBrowsingCompleted ();
		}

		protected virtual Task<Dictionary<string,string>> RequestAssertionAsync () {
			return null;
		}
	}
}

