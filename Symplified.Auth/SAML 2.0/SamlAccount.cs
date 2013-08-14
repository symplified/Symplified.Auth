using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Auth;
using Xamarin.Utilities;
using dk.nita.saml20;

using JSON = Newtonsoft.Json;

namespace Symplified.Auth
{
	/// <summary>
	/// A representation of a user account created and authenticated via SAML 2.0.
	/// </summary>
	public class SamlAccount : Account
	{
		private Saml20Assertion _saml20Assertion;

		private XmlElement _samlResponse;


		/// <summary>
		/// urn:ietf:params:oauth:grant-type:saml2-bearer
		/// </summary>
		public static string AUTHORIZATION_GRANT_TYPE = "urn:ietf:params:oauth:grant-type:saml2-bearer";

		/// <summary>
		/// urn:ietf:params:oauth:client-assertion-type:saml2-bearer
		/// </summary>
		public static string CLIENT_ASSERTION_TYPE = "urn:ietf:params:oauth:client-assertion-type:saml2-bearer";

		/// <summary>
		/// Initializes a new instance of the <see cref="Symplified.Auth.SamlAccount"/> class.
		/// </summary>
		public SamlAccount () : base () {}

		/// <summary>
		/// Initializes a new instance of the <see cref="Symplified.Auth.SamlAccount"/> class.
		/// </summary>
		/// <param name="assertion">Assertion.</param>
		public SamlAccount (Saml20Assertion assertion, XmlElement samlResponse)
		{
			this._saml20Assertion = assertion;
			this.Username = assertion.Subject.Value;
			this._samlResponse = samlResponse;
		}

		/// <summary>
		/// Gets or sets the assertion.
		/// </summary>
		/// <value>The assertion.</value>
		public Saml20Assertion Assertion
		{
			get {
				return _saml20Assertion;
			}
			set {
				_saml20Assertion = value;
				this.Username = _saml20Assertion.Subject.Value;
			}
		}

		/// <summary>
		/// Gets or sets the SAML XML response.
		/// </summary>
		/// <value>The saml response.</value>
		public XmlElement SamlResponse {
			get {
				return _samlResponse;
			}
			set {
				_samlResponse = value;
			}
		}

		/// <summary>
		/// Gets the bearer assertion authorization grant parameters. This is typically
		/// used to request an OAuth access token.
		/// </summary>
		/// <returns>The URL-encoded assertion parameters.</returns>
		public string GetBearerAssertionAuthorizationGrantParams ()
		{
			StringBuilder args = new StringBuilder ();

			args.AppendFormat ("grant_type={0}", HttpUtility.UrlEncode(AUTHORIZATION_GRANT_TYPE));

			string base64Assertion = SamlAccount.ToBase64ForUrlString (
				Encoding.UTF8.GetBytes (_saml20Assertion.XmlAssertion.OuterXml)
				);

			args.AppendFormat ("&assertion={0}", base64Assertion);
			return args.ToString ();
		}

		/// <summary>
		/// Gets the bearer assertion client authentication parameters.
		/// </summary>
		/// <returns>The URL-encoded client assertion parameters.</returns>
		public string GetBearerAssertionClientAuthenticationParams ()
		{
			StringBuilder args = new StringBuilder ();

			args.AppendFormat ("client_assertion_type={0}", HttpUtility.UrlEncode (CLIENT_ASSERTION_TYPE));

			string base64Assertion = SamlAccount.ToBase64ForUrlString (
				Encoding.UTF8.GetBytes (_saml20Assertion.XmlAssertion.OuterXml)
				);

			args.AppendFormat ("&client_assertion={0}", base64Assertion);

			return args.ToString ();	
		}

		/// <summary>
		/// Gets the bearer assertion authorization grant.
		/// </summary>
		/// <returns>The bearer assertion authorization grant.</returns>
		/// <param name="tokenEndpoint">The <see cref="System.Uri"/> that describes the endpoint that is capable of handling 
		/// OAuth bearer assertion authorization grants.</param>
		public Task<IDictionary<string,string>> GetBearerAssertionAuthorizationGrant (Uri tokenEndpoint)
		{
			WebRequest request = WebRequest.Create (tokenEndpoint);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";

			string htmlParams = GetBearerAssertionAuthorizationGrantParams ();
#if DEBUG
			Console.WriteLine (htmlParams);
#endif
			byte[] paramBytes = UTF8Encoding.Default.GetBytes (htmlParams);

			request.ContentLength = paramBytes.Length;
			request.GetRequestStream ().Write (paramBytes, 0, paramBytes.Length);

			return request.GetResponseAsync ().ContinueWith (t => {

				IDictionary<string,string> jsonResponseValues = 
					JSON.JsonConvert.DeserializeObject<Dictionary<string,string>> (t.Result.GetResponseText ());

				return jsonResponseValues;
			});
		}

		/// <summary>
		/// Serialize this account into a string that can be deserialized.
		/// </summary>
		public override string Serialize ()
		{
			return base.Serialize ();
		}

		/// <summary>
		/// Tos the base64 for URL string.
		/// </summary>
		/// <returns>The base64 for URL string.</returns>
		/// <param name="input">Input.</param>
		public static string ToBase64ForUrlString(byte[] input)
		{
			StringBuilder result = new StringBuilder(Convert.ToBase64String(input).TrimEnd('='));
			result.Replace('+', '-');
			result.Replace('/', '_');
			return result.ToString();
		}
	}
}

