using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestManager.Configuration;
using QuestManager.Managers;
using SharedLibrary.Data.Configuration;

namespace SharedLibrary
{
    public static class StartupInjection
    {
        public static void AddSharedLibrary(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.ConfigureWritable<DbConnectionOptions>(configuration.GetSection(Constants.QuestDbConfigurationSection));

            services.AddSingleton<IQuestDbConnection, QuestDbConnection>();
        }
    }
}
