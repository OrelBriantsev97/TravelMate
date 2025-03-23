using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;  
using Mapsui.UI.Maui;

namespace TravelMate
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Dangrek-Regular.ttf", "Dangrek");
                    fonts.AddFont("PlaywriteITModerna-VariableFont_wght.ttf", "PlayWrite");
                    fonts.AddFont("Pacifico-Regular.ttf", "Pacifico");
                    fonts.AddFont("SourGummy-VariableFont_wdth.ttf", "SourGummy");
                    fonts.AddFont("Chilanka-Regular.ttf", "Chi");
                    fonts.AddFont("Niconne-Regular.ttf", "Nic");

                })
                .UseSkiaSharp(); 
                



#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
