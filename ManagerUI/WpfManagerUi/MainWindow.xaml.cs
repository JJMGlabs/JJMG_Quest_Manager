using System;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary;
using SharedLibrary.Data;

namespace WpfManagerUi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var serviceCollection = new ServiceCollection();

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Add appsettings.json
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true) // Add environment-specific settings
                .AddEnvironmentVariables()
                .Build();

            serviceCollection.AddSingleton(configuration);
            var configManager = new ConfigurationManager();

            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddSingleton(configManager);
            serviceCollection.AddSharedLibrary(configManager);

            ServiceProvider provider = serviceCollection.BuildServiceProvider();

            Resources.Add("services", provider);
        }
    }
}
