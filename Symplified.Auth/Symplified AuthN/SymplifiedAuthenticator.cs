using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Utilities;

#if PLATFORM_IOS
using JSON = System.Json;
using Symplified.Auth.iOS;
using MonoTouch.Foundation;
using PlatformCookie = MonoTouch.Foundation.NSHttpCookie;
#elif PLATFORM_ANDROID

#endif


namespace Symplified.Auth
{
	public class SymplifiedAuthenticator : WebRedirectAuthenticator
	{
		public SymplifiedAuthenticator (Uri initialUrl, Uri redirectUrl)
			:base (initialUrl, redirectUrl)
		{
			;
		}

		protected override void OnPageEncountered (Uri url, System.Collections.Generic.IDictionary<string, string> query, System.Collections.Generic.IDictionary<string, string> fragment)
		{
			base.OnPageEncountered (url, query, fragment);
		}

		public override void OnPageLoaded (Uri url)
		{
			base.OnPageLoaded (url);
		}

		public override void OnPageLoading (Uri url)
		{
			base.OnPageLoading (url);
		}

		protected override void OnRedirectPageLoaded (Uri url, System.Collections.Generic.IDictionary<string, string> query, System.Collections.Generic.IDictionary<string, string> fragment)
		{
			object[] platformCookies = null;

#if PLATFORM_IOS
			platformCookies = NSHttpCookieStorage.SharedStorage.Cookies;
#elif PLATFORM_ANDROID
			platformCookies = null;
#endif
			CookieCollection cookieCollection = CookieConverter.ConvertToCLRCookies (platformCookies);
			CookieContainer cc = new CookieContainer (cookieCollection.Count);
			cc.Add (cookieCollection);

			RequestUserKeychainAsync (url).ContinueWith (task => {
				if (task.IsFaulted) {
					OnError (task.Exception);
				} else {

					var httpResponse = task.Result as HttpWebResponse;
					JSON.JsonObject jsonObject = null;

					using (var s = httpResponse.GetResponseStream ()) {
						using (var r = new StreamReader (s, Encoding.UTF8)) {
							jsonObject = (JSON.JsonObject)JSON.JsonObject.Parse (r.ReadToEnd ());
						}
					}

					OnSucceeded (new Account (jsonObject["username"], cc));
				}
			}, TaskScheduler.FromCurrentSynchronizationContext ());
		}

		protected Task<WebResponse> RequestUserKeychainAsync (Uri keychainUrl)
		{
			var req = WebRequest.Create (keychainUrl);
			req.Method = "POST";

			return Task
				.Factory
					.FromAsync<WebResponse> (req.BeginGetResponse, req.EndGetResponse, null);
		}
	}
}

