using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

using Symplified.Auth;

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
				new Uri ("https://home.symplified.net"),
				new Uri ("https://home.symplified.net/portal/mobile/applications.html")
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

		}
	}
}

