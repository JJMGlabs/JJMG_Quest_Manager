using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestManager.Configuration;
using QuestManager.Managers;

namespace SharedLibrary
{
    public static class StartupInjection
    {
        public static void AddSharedLibrary(this IServiceCollection services)
        {
            services.AddOptions<DbConnectionOptions>()
                .Configure<IConfiguration>(
                (options, configuration) =>
                configuration.GetSection(Constants.QuestDbConfigurationSection).Bind(options));



            services.AddSingleton<IQuestDbConnection, QuestDbConnection>();
        }
    }
}
