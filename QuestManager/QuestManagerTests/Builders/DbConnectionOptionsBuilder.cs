using QuestManager.Configuration;

namespace QuestManagerTests.Builders
{

    //TODO ajust for support of multiple option in other tests
    public class QuestDbConnectionOptionsBuilder : DbConnectionOptions
    {        
        private string _basePath;
        private string _dbName;
        private string _collectionName;

        public QuestDbConnectionOptionsBuilder SetConnectionBasePath(string basePath)
        {
            _basePath = basePath;
            return this;
        }

        public QuestDbConnectionOptionsBuilder SetDbName(string dbName)
        {
            _dbName = dbName;
            return this;
        }

        public QuestDbConnectionOptionsBuilder SetCollectionName(string collectionName)
        {
            _collectionName = collectionName;
            return this;
        }

        public QuestDbConnectionOptions Build()
        {
            return new QuestDbConnectionOptions()
            {
                BasePath = _basePath,
                DbName = _dbName,
                CollectionName = _collectionName
            };
        }
    }
}
