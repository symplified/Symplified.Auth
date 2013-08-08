using System;
using System.Collections.Generic;
using System.Linq;
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
			symplifiedSamlSection.Add (new StyledStringElement ("SAML Login", LoginToSymplifiedSamlSpProxy));
			symplifiedSamlSection.Add (samlLoginStatusStringElement = new StringElement (String.Empty));

			loginViewController = new DialogViewController (UITableViewStyle.Grouped, new RootElement ("Symplified.Auth.Token") { 
				symplifiedTokenSection,
				symplifiedSamlSection,
			});
			
			window.RootViewController = new UINavigationController (loginViewController);
			window.MakeKeyAndVisible ();


			try {
				XmlDocument xDoc = new XmlDocument ();
				xDoc.PreserveWhitespace = true;
//				xDoc.Load ("metadata-ADLER.xml");
				xDoc.Load ("tfobs-demo-idp-metadata.xml");


				Saml20MetadataDocument doc = new Saml20MetadataDocument (xDoc);

				Console.WriteLine (doc.ToXml ());
			}
			catch (Exception e) {
				Console.WriteLine (e);
			}

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
				new Uri ("https://sympidp-dev-ed.my.salesforce.com"),
				new Uri ("https://login.salesforce.com/services/oauth2/token")
//				new Uri ("http://54.235.215.29/Shibboleth.sso/Login"),
//				new Uri ("http://54.235.215.29/Shibboleth.sso/SAML2/POST")
			);

			authenticator.Completed += (s, e) => {
				loginViewController.DismissViewController (true, null);

				if (!e.IsAuthenticated) {
					samlLoginStatusStringElement.Caption = "Not authorized";
				}
				else {
					SamlAccount account = (SamlAccount)e.Account;
					Saml20Assertion assertion = account.Assertion;

					samlLoginStatusStringElement.Caption = String.Format ("Name: {0}", assertion.Subject.Value);

					string urlencode = account.GetBearerAssertionAuthorizationGrantParams ();
					Console.WriteLine (urlencode);
				}

				loginViewController.ReloadData ();
			};

			UIViewController vc = authenticator.GetUI ();
			loginViewController.PresentViewController (vc, true, null);
		}

		public static readonly string idpXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><md:EntityDescriptor xmlns:md=\"urn:oasis:names:tc:SAML:2.0:metadata\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" entityID=\"https://sympidp-dev-ed.my.salesforce.com\" validUntil=\"2023-08-06T19:10:09.985Z\">\n   <md:SPSSODescriptor AuthnRequestsSigned=\"true\" WantAssertionsSigned=\"true\" protocolSupportEnumeration=\"urn:oasis:names:tc:SAML:2.0:protocol\">\n      <md:KeyDescriptor use=\"signing\">\n         <ds:KeyInfo>\n            <ds:X509Data>\n               <ds:X509Certificate>MIIFBzCCA++gAwIBAgIQDJ4ihF+4VYzLxb+qASp7IzANBgkqhkiG9w0BAQUFADCBvDELMAk\nGA1UEBhMCVVMxFzAVBgNVBAoTDlZlcmlTaWduLCBJbmMuMR8wHQYDVQQLExZWZXJpU2lnbi\nBUcnVzdCBOZXR3b3JrMTswOQYDVQQLEzJUZXJtcyBvZiB1c2UgYXQgaHR0cHM6Ly93d3cud\nmVyaXNpZ24uY29tL3JwYSAoYykxMDE2MDQGA1UEAxMtVmVyaVNpZ24gQ2xhc3MgMyBJbnRl\ncm5hdGlvbmFsIFNlcnZlciBDQSAtIEczMB4XDTExMTIwNzAwMDAwMFoXDTEzMTIwNzIzNTk\n1OVowgY4xCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpDYWxpZm9ybmlhMRYwFAYDVQQHFA1TYW\n4gRnJhbmNpc2NvMR0wGwYDVQQKFBRTYWxlc2ZvcmNlLmNvbSwgSW5jLjEUMBIGA1UECxQLQ\nXBwbGljYXRpb24xHTAbBgNVBAMUFHByb3h5LnNhbGVzZm9yY2UuY29tMIGfMA0GCSqGSIb3\nDQEBAQUAA4GNADCBiQKBgQDMoSWW4dBiVScWbXno3C6n2+qR/0O+eE4lzT0Y1go53Pk+Skn\n9sUu43Z+uZ8lOXDqmLiScTaB43ePbqIAUYimqCR9aYCLmSeNwhs68dsxcyDVqm5XIr2OZsr\nLikhNkKPno+0fuoyOWbA35kRxBFXL66tEYlF8ETIT6G3kqt7CGVwIDAQABo4IBszCCAa8wC\nQYDVR0TBAIwADALBgNVHQ8EBAMCBaAwQQYDVR0fBDowODA2oDSgMoYwaHR0cDovL1NWUklu\ndGwtRzMtY3JsLnZlcmlzaWduLmNvbS9TVlJJbnRsRzMuY3JsMEQGA1UdIAQ9MDswOQYLYIZ\nIAYb4RQEHFwMwKjAoBggrBgEFBQcCARYcaHR0cHM6Ly93d3cudmVyaXNpZ24uY29tL3JwYT\nAoBgNVHSUEITAfBglghkgBhvhCBAEGCCsGAQUFBwMBBggrBgEFBQcDAjByBggrBgEFBQcBA\nQRmMGQwJAYIKwYBBQUHMAGGGGh0dHA6Ly9vY3NwLnZlcmlzaWduLmNvbTA8BggrBgEFBQcw\nAoYwaHR0cDovL1NWUkludGwtRzMtYWlhLnZlcmlzaWduLmNvbS9TVlJJbnRsRzMuY2VyMG4\nGCCsGAQUFBwEMBGIwYKFeoFwwWjBYMFYWCWltYWdlL2dpZjAhMB8wBwYFKw4DAhoEFEtruS\niWBgy70FI4mymsSweLIQUYMCYWJGh0dHA6Ly9sb2dvLnZlcmlzaWduLmNvbS92c2xvZ28xL\nmdpZjANBgkqhkiG9w0BAQUFAAOCAQEAVq0AapffwqicpyAu41f5pWDn7FPjgIt6lirqwo7t\nLRMpxFuYKIMg+wvioJQ8DJ8mNyw+JnZDPxdVjDSkE2Lb+5Z5P9vKbD833jqKP5vniMMvHRf\ntlkCqP/AI/9z6jomgQtfm3WbI7elTFJvDwA+/VdxgU86mKRpalMWDB545GxVFiO6AZ/8dvA\npoHVHTQBfrckk9JCrH++Wq3EmErKcxzsY8LItC8qCl5HtgJy160fII0ZdF8hV5vKlrHQpo9\n1L0c1pn+z5RB+kt8GIreME2rU3WEmtZglBKrlw3ik0sXL2CO/GCAzbh7YWkEfXtE3GcGh7N\nxcHB+08lZiJzKwN/yg==</ds:X509Certificate>\n            </ds:X509Data>\n         </ds:KeyInfo>\n      </md:KeyDescriptor>\n      <md:NameIDFormat>urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified</md:NameIDFormat>\n      <md:AssertionConsumerService Binding=\"urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST\" Location=\"https://login.salesforce.com\" index=\"0\" isDefault=\"true\"/>\n   </md:SPSSODescriptor>\n</md:EntityDescriptor>";

		public static readonly string adlerXml = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n<md:EntityDescriptor entityID=\"ADLER_SAML20_ID\" ID=\"qbydMazTKFnlWCmhqoiTn7vMm65\" xmlns:md=\"urn:oasis:names:tc:SAML:2.0:metadata\"><ds:Signature xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\">\n<ds:SignedInfo>\n<ds:CanonicalizationMethod Algorithm=\"http://www.w3.org/2001/10/xml-exc-c14n#\"/>\n<ds:SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"/>\n<ds:Reference URI=\"#qbydMazTKFnlWCmhqoiTn7vMm65\">\n<ds:Transforms>\n<ds:Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/>\n<ds:Transform Algorithm=\"http://www.w3.org/2001/10/xml-exc-c14n#\"/>\n</ds:Transforms>\n<ds:DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"/>\n<ds:DigestValue>d7IKBZZih3Pt1cDd3vFKFBGxM9U=</ds:DigestValue>\n</ds:Reference>\n</ds:SignedInfo>\n<ds:SignatureValue>\nOB+GcYYJRDK7cdh4QLrAkdORx/Es7SFsxVEVvm0FIWyTid0NP8Ta0H3doKVxZzi9Qfp1eW8sc2No\nIdRXwmdbwo8DpSWoy/JsK4Gin27ZSr2pzxhBPXMxJygcqoPRRCoha0XBHOn4EIZFkafF8fj1Xwjq\n+CQ7gjgrjs99eEF5haY=\n</ds:SignatureValue>\n<ds:KeyInfo>\n<ds:X509Data>\n<ds:X509Certificate>\nMIIB6zCCAVSgAwIBAgIGARf4bf/yMA0GCSqGSIb3DQEBBQUAMDkxCzAJBgNVBAYTAkRLMRIwEAYD\nVQQKEwlTYWZld2hlcmUxFjAUBgNVBAMTDVNhZmV3aGVyZVBpbmcwHhcNMDgwMjA4MDk0MzU0WhcN\nMDkwMjA3MDk0MzU0WjA5MQswCQYDVQQGEwJESzESMBAGA1UEChMJU2FmZXdoZXJlMRYwFAYDVQQD\nEw1TYWZld2hlcmVQaW5nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCpIQrFHuEW0a14Rw5p\ntCdL8pjjhE2DNWZy8IZHFM5QRFxV2MYyCCma6a/7FeRjfFQjqQwKjzrtDCwGrJDJCrjzenw3Oy5U\nj3eYD2jypxVXz8ZL9HLtuBMtuHrOOrN4eD2KgjYPWVr38wP07jQb9P8f4zviPNRea/EEHYzFxfs6\nOQIDAQABMA0GCSqGSIb3DQEBBQUAA4GBAIQrFoI+Xw7LvT3XKJ1kAVTfPZ6+QoNemu4Zv72Zd29M\nuwdcgr7BRA9o+XqA3n1kvmvXAk2cE8C41xBncjBY7/ubgDt8/cQgYycEwkHfnY2JclZNshiZCx/t\n/HQdOQN2Q3qFvX1xaeXMMpYk8HEufI4BGZP3gZCCzdnAs4q6yHsf\n</ds:X509Certificate>\n</ds:X509Data>\n<ds:KeyValue>\n<ds:RSAKeyValue>\n<ds:Modulus>\nqSEKxR7hFtGteEcOabQnS/KY44RNgzVmcvCGRxTOUERcVdjGMggpmumv+xXkY3xUI6kMCo867Qws\nBqyQyQq483p8NzsuVI93mA9o8qcVV8/GS/Ry7bgTLbh6zjqzeHg9ioI2D1la9/MD9O40G/T/H+M7\n4jzUXmvxBB2MxcX7Ojk=\n</ds:Modulus>\n<ds:Exponent>AQAB</ds:Exponent>\n</ds:RSAKeyValue>\n</ds:KeyValue>\n</ds:KeyInfo>\n</ds:Signature><md:IDPSSODescriptor protocolSupportEnumeration=\"urn:oasis:names:tc:SAML:2.0:protocol\"><md:KeyDescriptor use=\"signing\"><ds:KeyInfo xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"><ds:X509Data><ds:X509Certificate>MIIB6zCCAVSgAwIBAgIGARf4bf/yMA0GCSqGSIb3DQEBBQUAMDkxCzAJBgNVBAYTAkRLMRIwEAYDVQQKEwlTYWZld2hlcmUxFjAUBgNVBAMTDVNhZmV3aGVyZVBpbmcwHhcNMDgwMjA4MDk0MzU0WhcNMDkwMjA3MDk0MzU0WjA5MQswCQYDVQQGEwJESzESMBAGA1UEChMJU2FmZXdoZXJlMRYwFAYDVQQDEw1TYWZld2hlcmVQaW5nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCpIQrFHuEW0a14Rw5ptCdL8pjjhE2DNWZy8IZHFM5QRFxV2MYyCCma6a/7FeRjfFQjqQwKjzrtDCwGrJDJCrjzenw3Oy5Uj3eYD2jypxVXz8ZL9HLtuBMtuHrOOrN4eD2KgjYPWVr38wP07jQb9P8f4zviPNRea/EEHYzFxfs6OQIDAQABMA0GCSqGSIb3DQEBBQUAA4GBAIQrFoI+Xw7LvT3XKJ1kAVTfPZ6+QoNemu4Zv72Zd29Muwdcgr7BRA9o+XqA3n1kvmvXAk2cE8C41xBncjBY7/ubgDt8/cQgYycEwkHfnY2JclZNshiZCx/t/HQdOQN2Q3qFvX1xaeXMMpYk8HEufI4BGZP3gZCCzdnAs4q6yHsf</ds:X509Certificate></ds:X509Data></ds:KeyInfo></md:KeyDescriptor><md:KeyDescriptor use=\"encryption\"><ds:KeyInfo xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"><ds:X509Data><ds:X509Certificate>MIIB6zCCAVSgAwIBAgIGARf4bf/yMA0GCSqGSIb3DQEBBQUAMDkxCzAJBgNVBAYTAkRLMRIwEAYDVQQKEwlTYWZld2hlcmUxFjAUBgNVBAMTDVNhZmV3aGVyZVBpbmcwHhcNMDgwMjA4MDk0MzU0WhcNMDkwMjA3MDk0MzU0WjA5MQswCQYDVQQGEwJESzESMBAGA1UEChMJU2FmZXdoZXJlMRYwFAYDVQQDEw1TYWZld2hlcmVQaW5nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCpIQrFHuEW0a14Rw5ptCdL8pjjhE2DNWZy8IZHFM5QRFxV2MYyCCma6a/7FeRjfFQjqQwKjzrtDCwGrJDJCrjzenw3Oy5Uj3eYD2jypxVXz8ZL9HLtuBMtuHrOOrN4eD2KgjYPWVr38wP07jQb9P8f4zviPNRea/EEHYzFxfs6OQIDAQABMA0GCSqGSIb3DQEBBQUAA4GBAIQrFoI+Xw7LvT3XKJ1kAVTfPZ6+QoNemu4Zv72Zd29Muwdcgr7BRA9o+XqA3n1kvmvXAk2cE8C41xBncjBY7/ubgDt8/cQgYycEwkHfnY2JclZNshiZCx/t/HQdOQN2Q3qFvX1xaeXMMpYk8HEufI4BGZP3gZCCzdnAs4q6yHsf</ds:X509Certificate></ds:X509Data></ds:KeyInfo></md:KeyDescriptor><md:SingleLogoutService Location=\"https://adler:9031/idp/SLO.saml2\" Binding=\"urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect\"/><md:SingleLogoutService Location=\"https://adler:9031/idp/SLO.saml2\" Binding=\"urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST\"/><md:SingleSignOnService Location=\"https://adler:9031/idp/SSO.saml2\" Binding=\"urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST\"/><md:SingleSignOnService Location=\"https://adler:9031/idp/SSO.saml2\" Binding=\"urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect\"/><saml:Attribute NameFormat=\"urn:oasis:names:tc:SAML:2.0:attrname-format:basic\" Name=\"urn:surname\" xmlns:saml=\"urn:oasis:names:tc:SAML:2.0:assertion\"/><saml:Attribute NameFormat=\"urn:oasis:names:tc:SAML:2.0:attrname-format:basic\" Name=\"urn:oid:0.9.2342.19200300.100.1.1\" xmlns:saml=\"urn:oasis:names:tc:SAML:2.0:assertion\"/><saml:Attribute NameFormat=\"urn:oasis:names:tc:SAML:2.0:attrname-format:basic\" Name=\"urn:oid:2.5.4.4\" xmlns:saml=\"urn:oasis:names:tc:SAML:2.0:assertion\"/></md:IDPSSODescriptor><md:AttributeAuthorityDescriptor protocolSupportEnumeration=\"urn:oasis:names:tc:SAML:2.0:protocol\"><md:AttributeService Binding=\"urn:oasis:names:tc:SAML:2.0:bindings:SOAP\" Location=\"https://adler:9031/idp/attrsvc.ssaml2\"/></md:AttributeAuthorityDescriptor><md:ContactPerson contactType=\"administrative\"><md:Company>Safewhere</md:Company><md:GivenName>Niels</md:GivenName><md:SurName>Flensted-Jensen ÆØÅ</md:SurName><md:EmailAddress>nfj@safewhere.net</md:EmailAddress><md:TelephoneNumber>70225885</md:TelephoneNumber></md:ContactPerson></md:EntityDescriptor>";
	}
}

