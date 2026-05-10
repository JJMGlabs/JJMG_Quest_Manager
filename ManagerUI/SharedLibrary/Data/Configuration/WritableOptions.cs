using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SharedLibrary.Data.Configuration
{
    public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
    {
        private readonly IHostEnvironment _environment;
        private readonly IOptionsMonitor<T> _options;
        private readonly string _section;
        private readonly string _file;

        public WritableOptions(
            IHostEnvironment environment,
            IOptionsMonitor<T> options,
            string section,
            string file)
        {
            _environment = environment;
            _options = options;
            _section = section;
            _file = file;
        }

        public T Value => _options.CurrentValue;
        public T Get(string name) => _options.Get(name);

        public void Update(Action<T> applyChanges)
        {
            var filePath = ResolveSettingsPath();

            var jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(filePath));
            var sectionObject = jObject.TryGetValue(_section, out JToken section) ?
                JsonConvert.DeserializeObject<T>(section.ToString()) : (Value ?? new T());

            applyChanges(sectionObject);

            jObject[_section] = JObject.Parse(JsonConvert.SerializeObject(sectionObject));
            File.WriteAllText(filePath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
        }

        private string ResolveSettingsPath()
        {
            if (_environment != null)
            {
                var fileProvider = _environment.ContentRootFileProvider;
                var fileInfo = fileProvider.GetFileInfo(_file);
                if (fileInfo.Exists)
                    return fileInfo.PhysicalPath;
            }

            string startupPath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(startupPath, _file);
            if (File.Exists(filePath))
                return filePath;

            var fallbackPath = Path.Combine(startupPath, "appsettings.json");
            if (!string.Equals(_file, "appsettings.json", StringComparison.OrdinalIgnoreCase)
                && File.Exists(fallbackPath))
            {
                return fallbackPath;
            }

            return filePath;
        }
    }
}
