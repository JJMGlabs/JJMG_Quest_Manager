using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace SharedLibrary.Data.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureWritable<T>(
            this IServiceCollection services,
            IConfigurationSection section,
            string sectionName
            ) where T : class, new()
        {
            services.Configure<T>(section);

            //take value from here first
            services.AddOptions<T>()
                .Configure<IConfiguration>(
    (options, configuration) =>
    configuration.GetSection(sectionName).Bind(options));

            

            services.AddSingleton<IWritableOptions<T>>(provider =>
            {
                var environment = provider.GetService<IHostEnvironment>();
                var options = provider.GetService<IOptionsMonitor<T>>();
                return new WritableOptions<T>(environment, options, section.Key, GetAppSettingsFileName(environment));
            });
        }

        private static string GetAppSettingsFileName(IHostEnvironment environment)
        {
                string environmentName = (environment != null) ? environment.EnvironmentName.ToLower() :
                        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return $"appsettings.{environmentName}.json";
        }
    }
}
