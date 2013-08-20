## The Symplified Identity Provider (IdP) Sandbox

Use our SAML 2.0 environment with a hosted IdP and test account to quickly build a functional app. Swap in your production IdP when you're ready to deploy.

Need help understanding single sign-on terminology, like IdP and SP? The folks at Salesforce.com have provided a good overview:  [About Identity Providers and Service Providers](http://login.salesforce.com/help/doc/en/identity_provider_about.htm).



## 0. Acquire SAML 2.0 metadata

SAML metadata provides information about the identity provider (IdP) used for the authentication and single sign-on service. It is an XML document containing data such as server URIs, protocols, certificates, and so on.

We've provided an example to get you up and running.

```xml
	<?xml version="1.0" encoding="UTF-8" ?>
	<!--
	Metadata for Symplified IdP: https://idp.symplified.net/IdPServlet?idp_id=1v5uitrsv6u1b
	-->

	<EntityDescriptor
		xmlns="urn:oasis:names:tc:SAML:2.0:metadata"
		xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
		xmlns:ds="http://www.w3.org/2000/09/xmldsig#"
		xmlns:shibmd="urn:mace:shibboleth:metadata:1.0"
		xsi:schemaLocation="urn:oasis:names:tc:SAML:2.0:metadata saml-schema-metadata-2.0.xsd
							urn:mace:shibboleth:metadata:1.0 shibboleth-metadata-1.0.xsd
							urn:oasis:names:tc:SAML:metadata:ui sstc-saml-metadata-ui-v1.0.xsd
							http://www.w3.org/2000/09/xmldsig# xmldsig-core-schema.xsd"
		validUntil="2020-01-01T00:00:00Z"
		entityID="1v5uitrsv6u1b">
		<IDPSSODescriptor protocolSupportEnumeration="urn:oasis:names:tc:SAML:2.0:protocol">
			<Extensions>
				<shibmd:Scope>symplified.net</shibmd:Scope>
				<mdui:UIInfo xmlns:mdui="urn:oasis:names:tc:SAML:metadata:ui">
					<mdui:DisplayName xml:lang="en">Symplified Identity Provider</mdui:DisplayName>
					<mdui:InformationURL xml:lang="en">https://idp.symplified.net</mdui:InformationURL>
					<mdui:Logo height="77" width="192" xml:lang="en">http://www.symplified.com/img/layout/logo.png</mdui:Logo>
					<mdui:Logo height="16" width="16" xml:lang="en">https://home.symplified.net/resources/favicon.ico</mdui:Logo>
				</mdui:UIInfo>
			</Extensions>
			<KeyDescriptor>
				<ds:KeyInfo>
					<ds:X509Data>
						<ds:X509Certificate>
							MIICszCCAZsCBgFANhz5GTANBgkqhkiG9w0BAQsFADAdMRswGQYDVQQDExJpZHAu
							c3ltcGxpZmllZC5uZXQwHhcNMTMwNzMxMTkwMzIzWhcNMTcwNzMxMTkwMzIzWjAd
							MRswGQYDVQQDExJpZHAuc3ltcGxpZmllZC5uZXQwggEiMA0GCSqGSIb3DQEBAQUA
							A4IBDwAwggEKAoIBAQCogo3bmeJ/Qdn/VZtJHFZNttNu02fdu7ViuLTbpzaNL6TL
							HCDFL3L3o/+rMhxbKDSSpMBcpTFTUb++DAAfZXgElb4KjC5IHo33Q/b+CWs9UnH2
							qtAUsP7OsyPz8jTDfNrMGDn2eELtOlZc53mgN4gy5T0YJR/eJu0DDO8jEjngHqZc
							zo8zsWIDmqjFhGgTSWF4oLkKvyMCh67Vww1o3my9YHN3cBgV3wGRUZZk8meL0Vxs
							d1QMSoRMU299k48uFI5RKbf6fqSTjScaipzmDCOq+0m+G0rfETPWGzWQsmdgMQ7z
							sSmADXd7X/Kc10tUcUpx1VV3+Aw5L6wnXwlFtOCbAgMBAAEwDQYJKoZIhvcNAQEL
							BQADggEBAD5zh9A7UkHT6pKS4uSjlqjMoPTfYrg2ewhom38pbgimjqmOudquLQSj
							Shp9rQJLveUFO2pLFPJQsCrSDibtV31am0+U+J+yVp1C97natv7+eoeHue0fZy4y
							G4k8sokB9m0Mdb3SklXOl18+AMa8l0hvOxxVDcX5ebPLe1nONBRuTV4k/JdQfEVj
							vu+nGn+Y69ExHN7HjN66q9fT3DR+BcfFWJRzHaLWK+GzAaS6YPJ/F+6+iCF36rD7
							Bw1VtMhCXMRb8ReVOy7DgeYgM+XxdVL5gndOaBE4Oaxc9PhgKou36PDi+yAd+EGz
							kbIDXEIOF6ILuRCkOuhr+23UY/dVPu4=
						</ds:X509Certificate>
					</ds:X509Data>
				</ds:KeyInfo>
			</KeyDescriptor>

			<ArtifactResolutionService index="2"
				Binding="urn:oasis:names:tc:SAML:2.0:bindings:SOAP"
				Location="https://idp.symplified.net/IdPServlet?idp_id=1v5uitrsv6u1b"/>

			<NameIDFormat>urn:mace:shibboleth:1.0:nameIdentifier</NameIDFormat>
			<NameIDFormat>urn:oasis:names:tc:SAML:2.0:nameid-format:transient</NameIDFormat>

			<SingleSignOnService Binding="urn:mace:shibboleth:1.0:profiles:AuthnRequest"
				Location="https://idp.symplified.net/IdPServlet?idp_id=1v5uitrsv6u1b"/>
			<SingleSignOnService Binding="urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"
				Location="https://idp.symplified.net/IdPServlet?idp_id=1v5uitrsv6u1b"/>
			<SingleSignOnService Binding="urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"
				Location="https://idp.symplified.net/IdPServlet?idp_id=1v5uitrsv6u1b"/>
		</IDPSSODescriptor>
	</EntityDescriptor>
```


## 1. Create and configure an IdP

Let's get the website information required to use Symplified's IdP for your application. We'll load the  load the XML document containing SAML 2.0 metadata, and send it off to a metadata parser:

```csharp
	XmlDocument xDoc = new XmlDocument ();
	xDoc.PreserveWhitespace = true; // This is important do not remove
	xDoc.Load ("idp.symplified.net.metadata.xml");

	Saml20MetadataDocument idpMetadata = new Saml20MetadataDocument (xDoc);
```

## 2. Create and configure a SAML 2.0 authenticator

To verify an assertion that returns from the IdP, we'll configure an authenticator using the IdP metadata:

```csharp
	Saml20Authenticator authenticator = new Saml20Authenticator (
		"Symplified.Auth.iOS.Sample",
		idpMetadata
	);
```

The authenticator will:

* Create a SAML assertion
* Send it to the IdP
* Get an assertion back
  The assertion is issued depending on conditions such as the user's log in state
* Verify the signature on the assertion
* Request resource


## 3. Authenticate the user via the IdP


Although third-party authenticators control their own UI, you decide how to show the authenticator's UI on the screen. You can manage how the authentication UI is presented–modally, in navigation controllers, in popovers, and so on.

Prior to displaying the UI, we must first listen for the `Completed` event which triggers when user successfully authenticates or cancels. Find out whether the authentication succeeded by examining the `IsAuthenticated` property of `eventArgs`:


```csharp
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
```

All the information collected from a successful authentication is accessible in `eventArgs.Account`.

We are now ready to display the login UI from `ViewDidAppear` on iOS:

```csharp
	UIViewController vc = authenticator.GetUI ();
	loginViewController.PresentViewController (vc, true, null);
```

The `GetUI` method returns `UINavigationControllers` on iOS, and `Intents` on Android. Here is how we would write the code to display the UI from `OnCreate`:

```csharp
	var intent = authenticator.GetUI (this);
	StartActivityForResult (intent, 42);
```



## 4. Store the account


The Symplified Mobile Developer SDK securely stores `Account` objects so you don't always have to reauthenticate the user. The `AccountStore` class is in charge of storing `Account` information, supported by the [Keychain](https://developer.apple.com/library/ios/#documentation/security/Reference/keychainservices/Reference/reference.html) on iOS and a [KeyStore](http://developer.android.com/reference/java/security/KeyStore.html) on Android:

```csharp
	// On iOS:
	AccountStore.Create ().Save (eventArgs.Account, "idp.symplified.net");

	// On Android:
	AccountStore.Create (this).Save (eventArgs.Account, "idp.sympliifed.net");
```

Saved Accounts are uniquely identified wiht a key composed of the account's `Username` property and a "Service ID". The "Service ID" is any string that is used when retrieving accounts from the store.

If an `Account` was saved earlier, calling `Save` again will overwrite it. This is helpful for services that expire the credentials stored in the account object.


## 5. Retrieve stored accounts


You can get all `Account` objects stored for a given service:

```csharp
	// On iOS:
	IEnumerable<Account> accounts = AccountStore.Create ().FindAccountsForService ("idp.sympliifed.net");

	// On Android:
	IEnumerable<Account> accounts = AccountStore.Create (this).FindAccountsForService ("idp.sympliifed.net");
```


## Next Steps

### Exchange SAML 2.0 assertion for OAuth 2.0

You can bridge SAML 2.0 to REST APIs using the Symplified Mobile Developer SDK. For example, you can enable a third-party IdP such as [Salesforce](http://login.salesforce.com/help/doc/en/identity_provider_about.htm), then  federate authentication.

```csharp
	SamlAccount authenticatedAccount = (SamlAccount)eventArgs.Account;

	authenticatedAccount.GetBearerAssertionAuthorizationGrant (
		new Uri ("https://login.salesforce.com/services/oauth2/token")
	).ContinueWith (t => {
		if (!t.IsFaulted) {
			accessTokenStringElement.Caption = t.Result ["access_token"];
			scopeStringElement.Caption = t.Result ["scope"];

			BeginInvokeOnMainThread (delegate {
				loginViewController.ReloadData ();
				ListSalesforceResources (t.Result ["instance_url"], t.Result ["access_token"]);
			});
		}
		else {
			Console.WriteLine ("error");
		}
	});
```
