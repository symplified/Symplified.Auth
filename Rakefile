require "rake/clean"

CLEAN.include "*.xam"
CLEAN.include "xamarin-component"

COMPONENT = "Symplified.Auth-1.0.xam"

file "xamarin-component/xamarin-component.exe" do
	puts "* Downloading xamarin-component..."
	mkdir "xamarin-component"
	sh "curl -L https://components.xamarin.com/submit/xpkg > xamarin-component.zip"
	sh "unzip -o xamarin-component.zip -d xamarin-component"
	sh "rm xamarin-component.zip"
end

task :default => "xamarin-component/xamarin-component.exe" do
	line = <<-END
	mono xamarin-component/xamarin-component.exe create-manually #{COMPONENT} \
		--name="Symplified Mobile Identity Management SDK" \
		--summary="Add enterprise Identity and Access Management (IAM) to your mobile apps." \
		--publisher="Symplified, Inc." \
		--website="http://github.com/symplified/Symplified.Auth" \
		--details="Details.md" \
		--license="LICENSE.md" \
		--getting-started="GettingStarted.md" \
		--icon="icons/Symplified.Auth_128x128.png" \
		--icon="icons/Symplified.Auth_512x512.png" \
		--library="ios":"Symplified.Auth.iOS/bin/Release/Symplified.Auth.iOS.dll" \
        --library="ios":"Symplified.Auth.iOS/bin/Release/Xamarin.Auth.iOS.dll" \
		--library="android":"Symplified.Auth.Android/bin/Release/Symplified.Auth.Android.dll" \
        --library="android":"Symplified.Auth.Android/bin/Release/Xamarin.Auth.Android.dll" \
		--sample="iOS Sample. Demonstrates usage of the Symplified.Auth SAML 2.0 authentication mechanism on iOS.":"samples/Symplified.Auth.iOS.Sample/Symplified.Auth.iOS.Sample.sln" \
		--sample="Android Sample. Demonstrates usage of the Symplified.Auth SAML 2.0 authentication mechanism on Android":"samples/Symplified.Auth.Android.Sample/Symplified.Auth.Android.Sample.sln" \
        --sample="iOS OAuth SAML2 Bearer Assertion Grant Example. Demonstrates the usage of a SAML 2.0 assertion to request an OAuth2 access token for use with the Salesforce REST API":"samples/OAuthSaml2BearerExample-iOS/OAuthSaml2BearerExample-iOS.sln"
		END
	puts "* Creating #{COMPONENT}..."
	puts line.strip.gsub "\t\t", "\\\n    "
	sh line, :verbose => false
	puts "* Created #{COMPONENT}"
end
