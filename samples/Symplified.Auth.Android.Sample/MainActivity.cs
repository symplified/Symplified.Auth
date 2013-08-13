using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
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
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			authenticationStatus = FindViewById<TextView> (Resource.Id.textView1);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			
			button.Click += LoginWithIdentityProvider;
		}

		private TextView authenticationStatus;
		private readonly TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

		public void LoginWithIdentityProvider (object sender, EventArgs e)
		{
			XmlDocument xDoc = new XmlDocument ();
			xDoc.PreserveWhitespace = true;
			xDoc.Load (Assets.Open ("idp.symplified.net.metadata.xml"));

			Saml20MetadataDocument idpMetadata = new Saml20MetadataDocument (xDoc);

			Saml20Authenticator authenticator = new Saml20Authenticator (
				"Symplified.Auth.Android.Sample",
				idpMetadata
			);

			authenticator.Completed += (s, ee) => {
				if (!ee.IsAuthenticated) {
					this.authenticationStatus.Text = "Not authorized";
				}
				else {
					SamlAccount authenticatedAccount = (SamlAccount)ee.Account;
					this.authenticationStatus.Text = String.Format ("Subject: {0}", authenticatedAccount.Assertion.Subject.Value);
				}
			};

			var intent = authenticator.GetUI (this);
			StartActivityForResult (intent, 42);
		}
	}
}


