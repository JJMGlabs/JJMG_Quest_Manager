using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestManager.Configuration;
using QuestManager.Managers;
using QuestManager.Managers.Interfaces;
using SharedLibrary.Data.Configuration;

namespace SharedLibrary
{
    public static class StartupInjection
    {
        public static void AddSharedLibrary(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.ConfigureWritable<QuestDbConnectionOptions>(configuration.GetSection(Constants.QuestDbConfigurationSection), Constants.QuestDbConfigurationSection);
            services.ConfigureWritable<QuestLineDbConnectionOptions>(configuration.GetSection(Constants.QuestLineDbConfigurationSection), Constants.QuestLineDbConfigurationSection);
            services.AddScoped<IQuestDbConnection, QuestDbConnection>();
            services.AddScoped<IQuestlineDbConnection, QuestlineDbConnection>();
            services.AddScoped<IQuestlineQuestRelationshipConnection, QuestlineDbConnection>();
        }
    }
}
