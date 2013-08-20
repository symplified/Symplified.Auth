using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

using Symplified.Auth;
using Symplified.Auth.iOS;
using dk.nita.saml20;
using dk.nita.saml20.Validation;
using dk.nita.saml20.Schema.Metadata;
using dk.nita.saml20.Utils;
using dk.nita.saml20.config;
using dk.nita.saml20.Schema.XmlDSig;

using Xamarin.Utilities;

namespace Symplified.Auth.iOS.Sample
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;

		Section symplifiedTokenSection;

		Section symplifiedSamlSection;

		StringElement tokenLoginStatusStringElement, samlLoginStatusStringElement;

		DialogViewController loginViewController;

		UIViewController vc;

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

			symplifiedTokenSection = new Section ("Symplified Token");
			symplifiedTokenSection.Add (new StyledStringElement ("Token Login", LoginToSymplifiedToken));
			symplifiedTokenSection.Add (tokenLoginStatusStringElement = new StringElement (String.Empty));

			symplifiedSamlSection = new Section ("SAML 2.0 Mobile Service Provider");
			symplifiedSamlSection.Add (new StyledStringElement ("Assertion Login", LoginWithIdentityProvider));
			symplifiedSamlSection.Add (samlLoginStatusStringElement = new StringElement (String.Empty));

			loginViewController = new DialogViewController (UITableViewStyle.Grouped, new RootElement ("Symplified.Auth.Token") { 
				symplifiedTokenSection,
				symplifiedSamlSection,
			});
			
			window.RootViewController = new UINavigationController (loginViewController);
			window.MakeKeyAndVisible ();

			return true;
		}

		public void LoginToSymplifiedToken ()
		{
			SymplifiedAuthenticator authenticator = new SymplifiedAuthenticator (
				new Uri ("https://idp.symplified.net"),
				new Uri ("https://idp.symplified.net/portal/mobile/applications.html")
			);

			authenticator.Completed += (s,e) =>
			{
				loginViewController.DismissViewController (true, null);

				if (!e.IsAuthenticated) {
					tokenLoginStatusStringElement.Caption = "Not authorized";
				}
				else {
					tokenLoginStatusStringElement.Caption = "Authorized";
					tokenLoginStatusStringElement.GetActiveCell ().BackgroundColor = UIColor.Green;
				}

				loginViewController.ReloadData();
			};

			vc = authenticator.GetUI ();
			loginViewController.PresentViewController (vc, true, null);
		}

		public void LoginWithIdentityProvider ()
		{
			XmlDocument xDoc = new XmlDocument ();
			xDoc.PreserveWhitespace = true;
			xDoc.Load ("idp.symplified.net.metadata.xml");

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

					samlLoginStatusStringElement.Caption = String.Format ("Name: {0}", authenticatedAccount.Assertion.Subject.Value);
					samlLoginStatusStringElement.GetActiveCell ().BackgroundColor = UIColor.Green;
				}

				loginViewController.ReloadData ();
			};

			vc = authenticator.GetUI ();
			loginViewController.PresentViewController (vc, true, null);
		}
	}
}

