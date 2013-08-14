<<<<<<< HEAD
## 0. Acquire SAML 2.0 metadata

SAML metadata provides information about the identity provider (IdP) used for the authentication and single sign-on service. It is an XML document containing data such as server URIs, protocols, certificates, and so on.

We've provided an example to get you up and running. 

```csharp
Mike, please insert code here.
```

> Need help understanding single sign-on terminology, like SAML, IdP, and SP? Please see <[About Identity Providers and Service Providers](http://login.salesforce.com/help/doc/en/identity_provider_about.htm) on the Salesforce.com web site for more information.


## 1. Create and configure an IdP

Let's get the website information required to use Symplified's IdP for your application. We'll load the  load the XML document containing SAML 2.0 metadata, and send it off to a metadata parser:

```csharp
Mike, please insert code (4 lines) here.
```

## 2. Create and configure a SAML 2.0 authenticator

To verify an assertion that returns from the IdP, we'll configure an authenticator using the IdP metadata: 

```csharp
Mike, please insert code here.
```

The authenticator will: 

* Create a SAML assertion
* Send it to the IdP
* Get an assertion back 
  The assertion is issued depending on conditions such as the user's log in state 
* Verify the signature on the assertion
* Request resource
 

## 3. Authenticate the user via the IdP

_Here, we log the user into the IdP._

_Logging into the IdP. Similar language. get to complete? info about the session. Auth or not._

While authenticators manage their own UI, it's up to you to initially present the authenticator's UI on the screen. This lets you control how the authentication UI is displayed–modally, in navigation controllers, in popovers, etc.

Before we present the UI, we need to start listening to the `Completed` event which fires when the user successfully authenticates or cancels. You can find out if the authentication succeeded by testing the `IsAuthenticated` property of `eventArgs`:

```csharp
auth.Completed += (sender, eventArgs) => {
	// We presented the UI, so it's up to us to dimiss it on iOS.
	DismissViewController (true, null);

	if (eventArgs.IsAuthenticated) {
		// Use eventArgs.Account to do wonderful things
	} else {
		// The user cancelled
	}
};
```

All the information gathered from a successful authentication is available in `eventArgs.Account`.

Now we're ready to present the login UI from `ViewDidAppear` on iOS:

```csharp
PresentViewController (auth.GetUI (), true, null);
```

The `GetUI` method returns `UINavigationControllers` on iOS, and `Intents` on Android. On Android, we would write the following code to present the UI from `OnCreate`:

```csharp
StartActivity (auth.GetUI (this));



## 4. Store the account

You can fetch all `Account` objects stored for a given service:

```csharp
Mike, please insert code here.
```

It's that easy.




## 5. Retrieve stored accounts

Xamarin.Auth includes OAuth 1.0 and OAuth 2.0 authenticators, providing support for thousands of popular services. For services that use traditional username/password authentication, you can roll your own authenticator by deriving from `FormAuthenticator`.

If you want to authenticate against an ostensibly unsupported service, fear not – Xamarin.Auth is extensible! It's very easy to create your own authenticators – just derive from any of the existing authenticators and start overriding methods.




## 6. Exchange SAML 2.0 assertion for OAuth 2.0

_Provide pointers to salesforce. Functionality is provided. Bridging SAML to REST APIs._

_Provide link to sample app._

Xamarin.Auth includes OAuth 1.0 and OAuth 2.0 authenticators, providing support for thousands of popular services. For services that use traditional username/password authentication, you can roll your own authenticator by deriving from `FormAuthenticator`.

If you want to authenticate against an ostensibly unsupported service, fear not – Xamarin.Auth is extensible! It's very easy to create your own authenticators – just derive from any of the existing authenticators and start overriding methods.

=======
Getting Started...
>>>>>>> 5cb10b56ea38c1324e9662178b9d2b8ca5774264
