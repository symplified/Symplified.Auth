using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
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

			symplifiedSamlSection = new Section ("SAML 2.0 Service Provider Proxy");
			symplifiedSamlSection.Add (new StyledStringElement ("Assertion Login", LoginToSymplifiedSamlSpProxy));
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

		public void LoginToSymplifiedSamlSpProxy ()
		{
			Saml20SpProxyAuthenticator authenticator = new Saml20SpProxyAuthenticator (
//				new Uri ("https://sympidp-dev-ed.my.salesforce.com"),
//				new Uri ("https://login.salesforce.com/services/oauth2/token")
				new Uri ("http://54.235.215.29/Shibboleth.sso/Login"),
				new Uri ("http://54.235.215.29/Shibboleth.sso/SAML2/POST")
			);

			authenticator.Completed += (s, e) => {
				loginViewController.DismissViewController (true, null);

				if (!e.IsAuthenticated) {
					samlLoginStatusStringElement.Caption = "Not authorized";
				}
				else {
					SamlAccount account = (SamlAccount)e.Account;
					Saml20Assertion assertion = account.Assertion;

					XmlDocument xDoc = new XmlDocument ();
					xDoc.PreserveWhitespace = true;
					xDoc.Load ("idp.symplified.net.metadata.xml");

					Saml20MetadataDocument metadata = new Saml20MetadataDocument (xDoc);
					List<AsymmetricAlgorithm> trustedIssuers = new List<AsymmetricAlgorithm>(1);

					foreach (KeyDescriptor key in metadata.Keys)
					{
						System.Security.Cryptography.Xml.KeyInfo ki = 
							(System.Security.Cryptography.Xml.KeyInfo) key.KeyInfo;
						foreach (KeyInfoClause clause in ki)
						{
							AsymmetricAlgorithm aa = XmlSignatureUtils.ExtractKey(clause);
							trustedIssuers.Add(aa);
						}
					}

					try {
						assertion.CheckValid (trustedIssuers);

						samlLoginStatusStringElement.Caption = String.Format ("Name: {0}", assertion.Subject.Value);
						samlLoginStatusStringElement.GetActiveCell ().BackgroundColor = UIColor.Green;

						string urlencode = account.GetBearerAssertionAuthorizationGrantParams ();
						Console.WriteLine (urlencode);
					}
					catch (Saml20Exception samlEx) {
						Console.WriteLine (samlEx);
						samlLoginStatusStringElement.Caption = String.Format ("Name: {0}", assertion.Subject.Value);
						samlLoginStatusStringElement.GetActiveCell ().BackgroundColor = UIColor.Red;
					}
					catch (Exception ex) {
						Console.WriteLine (ex);
						samlLoginStatusStringElement.Caption = String.Format ("Name: {0}", assertion.Subject.Value);
						samlLoginStatusStringElement.GetActiveCell ().BackgroundColor = UIColor.Red;
					}
				}

				loginViewController.ReloadData ();
			};

			UIViewController vc = authenticator.GetUI ();
			loginViewController.PresentViewController (vc, true, null);
		}
	}
}

