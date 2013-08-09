using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Xml;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Symplified.Auth;
using Symplified.Auth.Android;
using dk.nita.saml20;
using dk.nita.saml20.Validation;
using dk.nita.saml20.Schema.Metadata;
using dk.nita.saml20.Utils;
using dk.nita.saml20.config;
using dk.nita.saml20.Schema.XmlDSig;

namespace Symplified.Auth.Android.Sample
{
	[Activity (Label = "Symplified.Auth.Android.Sample", MainLauncher = true)]
	public class MainActivity : Activity
	{
		int count = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			
			button.Click += LoginToSymplifiedSamlSpProxy;
		}

		private TextView authenticationStatus;
		private readonly TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

		public void LoginToSymplifiedSamlSpProxy (object sender, EventArgs e)
		{
			Saml20SpProxyAuthenticator authenticator = new Saml20SpProxyAuthenticator (
				//				new Uri ("https://sympidp-dev-ed.my.salesforce.com"),
				//				new Uri ("https://login.salesforce.com/services/oauth2/token")
				new Uri ("http://54.235.215.29/Shibboleth.sso/Login"),
				new Uri ("http://54.235.215.29/Shibboleth.sso/SAML2/POST")
				);

			authenticator.Completed += (s, ee) => {


				if (!ee.IsAuthenticated) {
					this.authenticationStatus.Text = "Not authorized";
				}
				else {
					SamlAccount account = (SamlAccount)ee.Account;
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

						this.authenticationStatus.Text = String.Format ("Name: {0}", assertion.Subject.Value);

						string urlencode = account.GetBearerAssertionAuthorizationGrantParams ();
						Console.WriteLine (urlencode);
					}
					catch (Saml20Exception samlEx) {
						Console.WriteLine (samlEx);
						this.authenticationStatus.Text = String.Format ("Name: {0}", assertion.Subject.Value);
					}
					catch (Exception ex) {
						Console.WriteLine (ex);
						this.authenticationStatus.Text = String.Format ("Name: {0}", assertion.Subject.Value);
					}
				}
			};

			var intent = authenticator.GetUI (this);
			StartActivityForResult (intent, 42);
		}
	}
}


