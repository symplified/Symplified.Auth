using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

using Symplified.Auth;
using Xamarin.Auth;
using Xamarin.Utilities;
using dk.nita.saml20;

namespace OAuthSaml2BearerExampleiOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;

		Section symplifiedSamlSection;

		StyledStringElement samlLoginStatusStringElement;
		StyledStringElement accessTokenStringElement;
		StyledStringElement scopeStringElement;
		JsonStringElement jsonResponseElement;

		DialogViewController loginViewController;

		class JsonStringElement : StyledMultilineElement
		{
			public JsonStringElement (string title)
				: base (title)
			{

			}

			public override float GetHeight (UITableView tableView, NSIndexPath indexPath)
			{
				if (string.IsNullOrEmpty (Caption)) {
					return 44.0f;
				} else {
					NSString s = new NSString (this.Caption);

					float height = s.StringSize (
						UIFont.SystemFontOfSize (12.0f), 
						new System.Drawing.SizeF (280.0f, 5000.0f), 
						UILineBreakMode.WordWrap
						).Height;

					return height;
				}
			}
		}

		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			symplifiedSamlSection = new Section ("Salesforce OAuth/SAML2 Grant");
			symplifiedSamlSection.Add (new StyledStringElement ("Exchange Assertion", PerformSalesforceOAuthSaml2Grant));

			samlLoginStatusStringElement = new StyledStringElement (String.Empty, "SAML 2.0 Assertion Subject", UITableViewCellStyle.Subtitle);
			samlLoginStatusStringElement.Lines = 1;
			samlLoginStatusStringElement.LineBreakMode = UILineBreakMode.TailTruncation;
			samlLoginStatusStringElement.Font = UIFont.SystemFontOfSize (12.0f);
			samlLoginStatusStringElement.SubtitleFont = UIFont.SystemFontOfSize (10.0f);
			symplifiedSamlSection.Add (samlLoginStatusStringElement);

			accessTokenStringElement = new StyledStringElement ("", "OAuth 2 Access Token", UITableViewCellStyle.Subtitle);
			accessTokenStringElement.Lines = 1;
			accessTokenStringElement.LineBreakMode = UILineBreakMode.TailTruncation;
			accessTokenStringElement.Font = UIFont.SystemFontOfSize (12.0f);
			accessTokenStringElement.SubtitleFont = UIFont.SystemFontOfSize (10.0f);
			symplifiedSamlSection.Add (accessTokenStringElement);

			scopeStringElement = new StyledStringElement ("", "Scope", UITableViewCellStyle.Subtitle);
			scopeStringElement.Lines = 1;
			scopeStringElement.LineBreakMode = UILineBreakMode.TailTruncation;
			scopeStringElement.Font = UIFont.SystemFontOfSize (12.0f);
			scopeStringElement.SubtitleFont = UIFont.SystemFontOfSize (10.0f);
			symplifiedSamlSection.Add (scopeStringElement);

			jsonResponseElement = new JsonStringElement ("");
			jsonResponseElement.LineBreakMode = UILineBreakMode.WordWrap;
			jsonResponseElement.Font = UIFont.SystemFontOfSize (12.0f);
			symplifiedSamlSection.Add (jsonResponseElement);

			RootElement menuRoot = new RootElement ("OAuthSaml2BearerExampleiOS");
			menuRoot.Add (symplifiedSamlSection);
			menuRoot.UnevenRows = true;

			loginViewController = new DialogViewController (UITableViewStyle.Grouped, menuRoot);

			window.RootViewController = loginViewController;
			window.MakeKeyAndVisible ();
			
			return true;
		}

		public void PerformSalesforceOAuthSaml2Grant ()
		{
			XmlDocument xDoc = new XmlDocument ();
			xDoc.PreserveWhitespace = true;
			xDoc.Load ("salesforce-oauthsaml2-idp-metadata.xml");

			Saml20MetadataDocument idpMetadata = new Saml20MetadataDocument (xDoc);

			Saml20Authenticator authenticator = new Saml20Authenticator (
				"Symplified.Auth.iOS.Sample",
				idpMetadata
				);

			authenticator.Completed += (s, e) => {
				loginViewController.DismissViewController (true, null);

				if (!e.IsAuthenticated) {
					samlLoginStatusStringElement.Caption = "Not authorized";
					samlLoginStatusStringElement.GetActiveCell ().BackgroundColor = UIColor.Red;
				}
				else {
					SamlAccount authenticatedAccount = (SamlAccount)e.Account;
					samlLoginStatusStringElement.Caption = authenticatedAccount.Assertion.Subject.Value;
					samlLoginStatusStringElement.GetActiveCell ().BackgroundColor = UIColor.Green;

					authenticatedAccount.GetBearerAssertionAuthorizationGrant (
						new Uri ("https://login.salesforce.com/services/oauth2/token")
						).ContinueWith (t => {
						if (!t.IsFaulted) {
							accessTokenStringElement.Caption = t.Result ["access_token"];
							scopeStringElement.Caption = t.Result ["scope"];

							BeginInvokeOnMainThread (delegate {
								loginViewController.ReloadData ();
								ListSalesforceResources (t.Result ["instance_url"], t.Result ["access_token"]);
							});
						}
						else {
							Console.WriteLine ("error");
						}
					});
				}

				loginViewController.ReloadData ();
			};

			UIViewController vc = authenticator.GetUI ();
			loginViewController.PresentViewController (vc, true, null);
		}

		public void ListSalesforceResources (string instanceUrl, string accessToken)
		{
			string endpointUrl = instanceUrl + "/services/data/v26.0";

			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (endpointUrl);
			request.Headers.Add ("Authorization", String.Format ("Bearer {0}", accessToken));
			request.Headers.Add ("X-PrettyPrint", "1");
			request.ContentType = "application/json";
			request.Method = "GET";
			request.Accept = "*/*";

			request.GetResponseAsync ().ContinueWith (t => {

				jsonResponseElement.Caption = t.Result.GetResponseText ();

				BeginInvokeOnMainThread (delegate {
					loginViewController.ReloadData ();
				});
			});
		}
	}
}

