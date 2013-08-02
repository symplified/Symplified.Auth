using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

using Symplified.Auth;
using Symplified.Auth.iOS;
using dk.nita.saml20;

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

			symplifiedSamlSection = new Section ("Symplified SAML");
			symplifiedSamlSection.Add (new StyledStringElement ("SAML Login", LoginToSymplifiedSaml));
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
					loginViewController.ReloadData();
					return;
				}
			};

			UIViewController vc = authenticator.GetUI ();
			loginViewController.PresentViewController (vc, true, null);
		}

		public void LoginToSymplifiedSaml ()
		{
			SAML20Authenticator authenticator = new SAML20Authenticator (
//				new Uri ("https://sympidp-dev-ed.my.salesforce.com"),
//				new Uri ("https://login.salesforce.com")
				new Uri ("http://ec2-23-22-199-50.compute-1.amazonaws.com/Shibboleth.sso/Login"),
				new Uri ("http://ec2-23-22-199-50.compute-1.amazonaws.com/Shibboleth.sso/SAML2/POST")
			);

			authenticator.Completed += (s, e) => {
				loginViewController.DismissViewController (true, null);

				if (!e.IsAuthenticated) {
					samlLoginStatusStringElement.Caption = "Not authorized";
				}
				else {
					SamlAccount account = (SamlAccount)e.Account;
					Saml20Assertion assertion = account.Assertion;

					samlLoginStatusStringElement.Caption = String.Format ("Username: {0}", assertion.Subject.Value);
				}

				loginViewController.ReloadData ();
			};

			UIViewController vc = authenticator.GetUI ();
			loginViewController.PresentViewController (vc, true, null);
		}
	}
}

