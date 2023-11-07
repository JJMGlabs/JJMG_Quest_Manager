using System.Windows;
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
            serviceCollection.AddWpfBlazorWebView();

            serviceCollection.AddSharedLibrary();

            Resources.Add("services", serviceCollection.BuildServiceProvider());
        }
    }
}
