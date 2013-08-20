using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Utilities;

#if PLATFORM_IOS
using JSON = Newtonsoft.Json;
using Symplified.Auth.iOS;
using MonoTouch.Foundation;
using PlatformCookie = MonoTouch.Foundation.NSHttpCookie;
#elif PLATFORM_ANDROID
using JSON = System.Json;
//using Symplified.Auth.Android;
using PlatformCookie = Org.Apache.Http.Cookies.ICookie;
#endif


namespace Symplified.Auth
{
	/// <summary>
	/// Symplified authenticator.
	/// </summary>
	public class SymplifiedAuthenticator : WebRedirectAuthenticator
	{
		private const string COOKIE_NAME = "singlepoint";

		private bool _redirected = false;

		private const string KEYCHAIN_ENDPOINT = "/KeychainRetrievalServlet";

		public SymplifiedAuthenticator (Uri initialUrl, Uri redirectUrl)
			:base (initialUrl, redirectUrl)
		{
#if PLATFORM_ANDROID
			throw new NotSupportedException ("Android does not currently support Symplified cookie-based login");
#endif
		}

		/// <summary>
		/// Raised when a new page has been loaded.
		/// </summary>
		/// <param name="url">URL of the page.</param>
		/// <param name="query">The parsed query of the URL.</param>
		/// <param name="fragment">The parsed fragment of the URL.</param>
		protected override void OnPageEncountered (Uri url, System.Collections.Generic.IDictionary<string, string> query, System.Collections.Generic.IDictionary<string, string> fragment, System.Collections.Generic.IDictionary<string, string> formParams)
		{
			base.OnPageEncountered (url, query, fragment, formParams);
		}

		/// <summary>
		/// Raises the page loaded event.
		/// </summary>
		/// <param name="url">URL.</param>
		public override void OnPageLoaded (Uri url)
		{
			base.OnPageLoaded (url);
		}

		/// <summary>
		/// Event handler called when a new page is being loaded in the web browser.
		/// </summary>
		/// <param name="url">The URL of the page.</param>
		/// <param name="formParams">Form parameters.</param>
		public override void OnPageLoading (Uri url, IDictionary<string,string> formParams)
		{
			base.OnPageLoading (url, formParams);
		}

		/// <summary>
		/// Raised when the redirect page has been loaded.
		/// </summary>
		/// <param name="url">URL of the page.</param>
		/// <param name="query">The parsed query of the URL.</param>
		/// <param name="fragment">The parsed fragment of the URL.</param>
		/// <param name="formParams">Form parameters.</param>
		protected override void OnRedirectPageLoaded (Uri url, System.Collections.Generic.IDictionary<string, string> query, System.Collections.Generic.IDictionary<string, string> fragment, System.Collections.Generic.IDictionary<string, string> formParams)
		{
			lock (this) {
				if (_redirected) {
					return;
				} else {
					_redirected = true;
				}
			}

			object[] platformCookies = null;

#if PLATFORM_IOS
			platformCookies = NSHttpCookieStorage.SharedStorage.Cookies;
#elif PLATFORM_ANDROID
			platformCookies = null;
#endif

#if PLATFORM_IOS || PLATFORM_ANDROID
			CookieCollection cookieCollection = CookieConverter.ConvertToCLRCookies (platformCookies);
			CookieContainer cc = new CookieContainer (cookieCollection.Count);
			cc.Add (cookieCollection);

			var c = cc.GetCookies (new Uri ("https://idp.symplified.net"))
						.Cast<Cookie> ()
						.FirstOrDefault (x => x.Name == "singlepoint-auth-error");

			if (c != null && !string.Equals (c, string.Empty)) {

				/* TODO: Check the URI for:
				 * singlepoint-portal-event=auth-failed&singlepoint-auth-error=NOT_AUTHENTICATED
				 */

				OnError (new AuthException ("NOT_AUTHENTICATED"));
			}

			Uri keychainUri = new Uri (url.GetLeftPart (UriPartial.Authority) + KEYCHAIN_ENDPOINT);

			RequestUserKeychainAsync (keychainUri, cc).ContinueWith (task => {
				if (task.IsFaulted) {
					OnError (task.Exception);
				} else {
					OnSucceeded (new Account ("sdkuser", cc));
				}
			}, TaskScheduler.FromCurrentSynchronizationContext ());
#endif
		}

		/// <summary>
		/// Requests the user keychain async.
		/// </summary>
		/// <returns>The user keychain async.</returns>
		/// <param name="keychainUrl">Keychain URL.</param>
		protected Task<WebResponse> RequestUserKeychainAsync (Uri keychainUrl, CookieContainer cc)
		{
			var req = (HttpWebRequest)WebRequest.Create (keychainUrl);
			req.Method = "GET";
			req.CookieContainer = cc;

			return Task
				.Factory
					.FromAsync<WebResponse> (req.BeginGetResponse, req.EndGetResponse, null);
		}
	}
}

