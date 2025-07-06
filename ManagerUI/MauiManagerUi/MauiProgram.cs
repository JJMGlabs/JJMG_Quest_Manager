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

            IConfiguration configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Add appsettings.json
.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true) // Add environment-specific settings
.AddEnvironmentVariables()
    .Build();
            builder.Services.AddSingleton(configuration);
            var configManager = new ConfigurationManager();

            builder.Services.AddSharedLibrary(configManager);


            return builder.Build();
        }
    }
}