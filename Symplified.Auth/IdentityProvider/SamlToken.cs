using System;
using System.Globalization;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Symplified.Auth
{
	[XmlRoot("SAMLToken")]
	public class SamlToken : IdentityToken
	{
		protected string token;
		protected SamlAssertion assertion;

		public SamlToken ()
		{
		}

		public override string Token {
			get {
				return token;
			}
			set {
				this.token = value;
			}
		}

		public string ToXmlString () {
			using (XmlDictionaryWriter xdw = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (new StringWriter ()))) {

				// mgile 19-JUL-2013
				// Assumption: Mono Framework v2.10
				//
				// NOTE: SamlSerializer is not implemented in Mono
				// however the WriteXml method does not actually use the
				// SamlSerializer other than checking for null, so we 
				// can still use the SamlAssertion WriteXml method if
				// we pass an empty, but non-null, SamlSerializer instance.
				SamlSerializer sser = new SamlSerializer ();

				SecurityTokenSerializer sts = (SecurityTokenSerializer) new object ();

				assertion.WriteXml (xdw, sser, sts);

				return xdw.ToString ();
			}
		}

		public static void ParseXmlString (string xml)
		{
			XmlDictionaryReader xmlDictionaryReader = (XmlDictionaryReader)XmlDictionaryReader.Create (new System.IO.MemoryStream (UTF8Encoding.Default.GetBytes (xml)));
			SamlAssertion assertion = new SamlSerializer ().LoadAssertion (xmlDictionaryReader, null, null);
			Console.WriteLine ("assertion: {0}", xml);
		}

		public override string GetBearerAssertionHeader ()
		{
			string xml = this.ToXmlString ();
			Encoding e = new UTF8Encoding ();
			string base64Assertion = Convert.ToBase64String (e.GetBytes (xml));

			return "Bearer: " + base64Assertion;
		}
	}
}

