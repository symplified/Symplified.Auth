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

		StringElement samlLoginStatusStringElement;
		StyledStringElement accessTokenStringElement;
		StyledStringElement scopeStringElement;
		StyledStringElement jsonResponseElement;

		DialogViewController loginViewController;

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
			symplifiedSamlSection.Add (samlLoginStatusStringElement = new StringElement (String.Empty));

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

			jsonResponseElement = new StyledStringElement ("");
			jsonResponseElement.Lines = 20;
			jsonResponseElement.LineBreakMode = UILineBreakMode.WordWrap;
			jsonResponseElement.Font = UIFont.SystemFontOfSize (12.0f);
			symplifiedSamlSection.Add (jsonResponseElement);

			loginViewController = new DialogViewController (UITableViewStyle.Grouped, new RootElement ("OAuthSaml2BearerExampleiOS") { 
				symplifiedSamlSection,
			});

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
					samlLoginStatusStringElement.Caption = String.Format ("Subject: {0}", authenticatedAccount.Assertion.Subject.Value);
					samlLoginStatusStringElement.GetActiveCell ().BackgroundColor = UIColor.Green;

					authenticatedAccount.GetBearerAssertionAuthorizationGrant (
						new Uri ("https://login.salesforce.com/services/oauth2/token")
						).ContinueWith (t => {
						if (!t.IsFaulted) {
							accessTokenStringElement.Caption = t.Result ["access_token"];
							scopeStringElement.Caption = t.Result ["scope"];

							BeginInvokeOnMainThread (delegate {
								loginViewController.ReloadData ();
								ListSalesforceResources (t.Result ["access_token"]);
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

		public void ListSalesforceResources (string accessToken)
		{
			WebRequest request = WebRequest.Create ("https://na1.salesforce.com/services/data/v26.0/");
			request.Headers.Add ("Authorization", String.Format ("Bearer {0}", WebEx.HtmlEncode (accessToken)));



			jsonResponseElement.Caption = request.GetResponse ().GetResponseText ();
			Console.WriteLine (jsonResponseElement.Caption);
//			request.GetResponseAsync ().ContinueWith (t => {
//
//				jsonResponseElement.Caption = t.Result.GetResponseText ();
//
//				BeginInvokeOnMainThread (delegate {
//					loginViewController.ReloadData ();
//				});
//			});


			// TODO: Add access token information to Account
//			var request = new OAuth2Request ("GET", new Uri ("https://na1.salesforce.com/services/data/v26.0/"), null, e.Account);
//			request.GetResponseAsync().ContinueWith (t => {
//				if (t.IsFaulted)
//					facebookStatus.Caption = "Error: " + t.Exception.InnerException.Message;
//				else if (t.IsCanceled)
//					facebookStatus.Caption = "Canceled";
//				else
//				{
//					var obj = JsonValue.Parse (t.Result.GetResponseText());
//					facebookStatus.Caption = "Logged in as " + obj["name"];
//				}
//
//				dialog.ReloadData();
//			}, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}
}

