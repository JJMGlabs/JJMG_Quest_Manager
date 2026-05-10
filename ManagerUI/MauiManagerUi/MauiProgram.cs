using SharedLibrary.Data;
using Microsoft.AspNetCore.Components.WebView.Maui;
using SharedLibrary;
using Microsoft.Extensions.Configuration;

namespace MauiManagerUi
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
                });

            builder.Services.AddMauiBlazorWebView();

            #if DEBUG
                builder.Services.AddBlazorWebViewDeveloperTools();
#endif

            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true); // Add appsettings.json
            builder.Configuration.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true); // Add environment-specific settings
            builder.Configuration.AddEnvironmentVariables();

            builder.Services.AddSharedLibrary(builder.Configuration);


            return builder.Build();
        }
    }
}