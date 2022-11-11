using Android.App;
using Android.Content;
using Android.Content.PM;

namespace GoogleDriveIIntegration.Droid
{
    [Activity(Label = "CustomUrlSchemeInterceptorActivity", NoHistory = true, LaunchMode = LaunchMode.SingleTop)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "com.googleusercontent.apps.334177818474-mc4kpcnp9g04ssd2hs13ei6jcvjlfdr1",
        DataPath = "/oauth2redirect"
    )]
    public class CustomUrlSchemeInterceptorActivity : Xamarin.Essentials.WebAuthenticatorCallbackActivity { }
}