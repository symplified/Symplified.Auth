using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Auth;
using dk.nita.saml20;
using dk.nita.saml20.Schema.Core;

namespace Symplified.Auth
{
	/// <summary>
	/// SAML 2.0 authenticator.
	/// </summary>
	public class Saml20SpProxyAuthenticator : WebRedirectAuthenticator
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Symplified.Auth.Saml20SpProxyAuthenticator"/> class.
		/// </summary>
		/// <param name="initialUrl">Initial URL.</param>
		/// <param name="redirectUrl">Redirect URL.</param>
		public Saml20SpProxyAuthenticator (Uri initialUrl, Uri redirectUrl)
			: base (initialUrl, redirectUrl)
		{

		}

		/// <summary>
		/// Method that returns the initial URL to be displayed in the web browser.
		/// </summary>
		/// <returns>A task that will return the initial URL.</returns>
		public override Task<Uri> GetInitialUrlAsync ()
		{
			return base.GetInitialUrlAsync ();
		}

		/// <summary>
		/// Event handler called when a new page is being loaded in the web browser.
		/// </summary>
		/// <param name="url">The URL of the page.</param>
		/// <param name="formParams">Form parameters.</param>
		public override void OnPageLoading (Uri url, IDictionary<string,string> formParams)
		{
			base.OnPageLoading (url, formParams);
		}

		/// <summary>
		/// Raised when a new page has been loaded.
		/// </summary>
		/// <param name="url">URL of the page.</param>
		/// <param name="query">The parsed query of the URL.</param>
		/// <param name="fragment">The parsed fragment of the URL.</param>
		/// <param name="formParams">Form parameters.</param>
		protected override void OnPageEncountered (Uri url, IDictionary<string, string> query, IDictionary<string, string> fragment, IDictionary<string, string> formParams)
		{
			base.OnPageEncountered (url, query, fragment, formParams);
		}

		/// <summary>
		/// Raised when the redirect page has been loaded.
		/// </summary>
		/// <param name="url">URL of the page.</param>
		/// <param name="query">The parsed query of the URL.</param>
		/// <param name="fragment">The parsed fragment of the URL.</param>
		/// <param name="formParams">Form parameters.</param>
		protected override void OnRedirectPageLoaded (Uri url, System.Collections.Generic.IDictionary<string, string> query, System.Collections.Generic.IDictionary<string, string> fragment, IDictionary<string, string> formParams)
		{
			string base64SamlAssertion = formParams.ContainsKey ("SAMLResponse") ? formParams ["SAMLResponse"] : string.Empty;
			byte[] xmlSamlAssertionBytes = Convert.FromBase64String (base64SamlAssertion);
			string xmlSamlAssertion = System.Text.UTF8Encoding.Default.GetString (xmlSamlAssertionBytes);

			XmlDocument xDoc = new XmlDocument ();
			xDoc.PreserveWhitespace = true;
			xDoc.LoadXml (xmlSamlAssertion);
		
			XmlElement responseElement = (XmlElement)xDoc.SelectSingleNode ("//*[local-name()='Response']");

			Console.WriteLine ("{0}", responseElement.OuterXml);

			XmlElement assertionElement = (XmlElement)xDoc.SelectSingleNode ("//*[local-name()='Assertion']");
			if (assertionElement != null) {
				Console.WriteLine ("{0}", assertionElement.OuterXml);

				Saml20Assertion samlAssertion = new Saml20Assertion (assertionElement, null, AssertionProfile.Core, false, false);
				Assertion a = samlAssertion.Assertion;

				SamlAccount sa = new SamlAccount (samlAssertion, responseElement);
				OnSucceeded (sa);
			}
			else {
				OnError ("No SAML Assertion Found");                                                                                                                                                                          ;
			}
		}

		/// <summary>
		/// Raises the browsing completed event.
		/// </summary>
		protected override void OnBrowsingCompleted ()
		{
			base.OnBrowsingCompleted ();
		}

		/// <summary>
		/// Requests the assertion async.
		/// </summary>
		/// <returns>The assertion async.</returns>
		protected virtual Task<Dictionary<string,string>> RequestAssertionAsync () {
			return null;
		}
	}
}

