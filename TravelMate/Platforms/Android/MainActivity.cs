using Android.App;
using Android.Content.PM;
using Android.OS;

namespace TravelMate
{
    [Activity(Theme = "@style/SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetTheme(Resource.Style.SplashTheme);

            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#7ea3ab"));
        }
    }
}
