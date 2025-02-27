namespace QuestManager.Configuration
{
    public class DbConnectionOptions
    {
        //In a file this is the path to the folder, for a server it is hosting location
        public string BasePath { get; set; } = string.Empty;
        //In a file this is the actual name of the folder, for a server it is the database
        public string DbName { get; set; } = string.Empty;
        //In a file this is the file name, for a server this is the collection
        public string CollectionName { get; set; } = string.Empty;
        //(Optional)In a file this is the field name or delimiter within the file, for a server this is a document ID
        public string RootObject { get; set; } = string.Empty;
    }

    public class QuestDbConnectionOptions : DbConnectionOptions { }

    public class QuestLineDbConnectionOptions : DbConnectionOptions { }
}
