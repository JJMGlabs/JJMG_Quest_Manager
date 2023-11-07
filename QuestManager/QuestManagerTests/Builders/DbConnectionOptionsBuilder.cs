using QuestManager.Configuration;

namespace QuestManagerTests.Builders
{
    public class DbConnectionOptionsBuilder : DbConnectionOptions
    {        
        private string _basePath;
        private string _dbName;
        private string _collectionName;

        public DbConnectionOptionsBuilder SetConnectionBasePath(string basePath)
        {
            _basePath = basePath;
            return this;
        }

        public DbConnectionOptionsBuilder SetDbName(string dbName)
        {
            _dbName = dbName;
            return this;
        }

        public DbConnectionOptionsBuilder SetCollectionName(string collectionName)
        {
            _collectionName = collectionName;
            return this;
        }

        public DbConnectionOptions Build()
        {
            return new DbConnectionOptions()
            {
                BasePath = _basePath,
                DbName = _dbName,
                CollectionName = _collectionName
            };
        }
    }
}
