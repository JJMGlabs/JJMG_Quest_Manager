
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.QuestSubObjects;
using QuestManagerTests.Builders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuestManager.Managers.Tests
{
    [TestClass()]
    public class QuestDbConnectionTests
    {
        private static QuestDbConnection ArrangeConnection()
        {
            var dbOptions = new DbConnectionOptionsBuilder()
                .SetConnectionBasePath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create))
                .SetDbName(@"\JJMGquestManager")
                .SetCollectionName(@"\questDb.json")
                .Build();

            var connection = new QuestDbConnection(Options.Create(dbOptions));
            return connection;
        }

        [TestMethod()]
        public void QuestDbConnectionTest()
        {
            try
            {
                QuestDbConnection sut = ArrangeConnection();
            }
            catch (Exception)
            {
                Assert.Fail();
            }

        }

        [TestMethod()]
        public void GetAllQuestsTest()
        {
            QuestDbConnection sut = ArrangeConnection();
            var result = sut.GetAllQuests();
            Assert.IsInstanceOfType(result, typeof(List<Quest>));
        }



        [TestMethod()]
        public void CreateQuestTest()
        {
            QuestDbConnection sut = ArrangeConnection();
            var result = sut.CreateQuest(new Quest());
            Assert.IsTrue(!string.IsNullOrEmpty(result.Id));
        }

        [TestMethod()]
        public void SaveQuestDbChangesTest()
        {
            QuestDbConnection sut = ArrangeConnection();
            var result = sut.SaveQuestDbChanges();
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public void SaveQuestDbChanges_ValidateDataOnNextReadTest()
        {
            QuestDbConnection sut = ArrangeConnection();

            var quests = new List<Quest>() {
                new Quest() { Name = "test 1" },
                new Quest() { Name = "test 2" },
                new Quest() { Name = "test 3" },
            };

            quests[0].AddSubObject(new QuestMeasurement() { Name = "Name" });

            var overwriteResult = sut.OverwriteAllQuests(quests);
            Assert.IsTrue(overwriteResult.Count == quests.Count);


            var result = sut.SaveQuestDbChanges();
            Assert.IsTrue(result.IsSuccess);

            sut = null;
            sut = ArrangeConnection();

            var savedQuests = sut.GetAllQuests();

            Assert.AreEqual(quests.Count, savedQuests.Count);
            Assert.AreEqual(quests[0].Name, savedQuests[0].Name);
            Assert.AreEqual(quests[0].QuestMeasurements[0].Name, savedQuests[0].QuestMeasurements[0].Name);
        }

        [TestMethod()]
        public void OverwriteAllQuestsTest()
        {
            QuestDbConnection sut = ArrangeConnection();
            var quests = new List<Quest>() {
                new Quest() { Name = "test 1" },
                new Quest() { Name = "test 2" },
                new Quest() { Name = "test 3" },
            };
            var result = sut.OverwriteAllQuests(quests);
            Assert.IsTrue(result.Count == quests.Count);
        }


        [TestMethod()]
        public void CreateQuestsTest()
        {
            QuestDbConnection sut = ArrangeConnection();
            var quests = new List<Quest>() {
                new Quest(),
                new Quest(),
                new Quest(),
            };
            var result = sut.CreateQuests(quests);
            Assert.IsTrue(result.Count == quests.Count);
        }

        [TestMethod()]
        public void GetQuestTest()
        {
            QuestDbConnection sut = ArrangeConnection();
            var result = sut.CreateQuest(new Quest());
            string newID = result.Id;
            var retreivedQuest = sut.GetQuest(newID);
            Assert.IsTrue(!string.IsNullOrEmpty(retreivedQuest.Id));
        }

        [TestMethod()]
        public void UpdateQuestTest()
        {
            string updateName = "PostUpdate";
            QuestDbConnection sut = ArrangeConnection();
            var result = sut.CreateQuest(new Quest() { Name = "PreUpdate" });
            string newID = result.Id;
            var retreivedQuest = sut.GetQuest(newID);
            retreivedQuest.Name = updateName;
            var updateResult = sut.UpdateQuest(retreivedQuest);
            Assert.IsTrue(updateResult.Name == updateName);
        }

        [TestMethod()]
        public void UpdateQuestsTest()
        {
            string updateName = "PostUpdate";
            string orignalName = "PreUpdate";

            QuestDbConnection sut = ArrangeConnection();

            List<Quest> questsCreate = new List<Quest>();
            for (int i = 0; i < 3; i++)
                questsCreate.Add(new Quest() { Name = $"{orignalName}{i}" });

            var result = sut.CreateQuests(questsCreate);

            for (int i = 0; i < result.Count; i++)
            {
                Quest quest = result[i];
                quest.Name = $"{updateName}{i}";
            }

            var retreivedQuest = sut.UpdateQuests(result);

            Assert.IsTrue(retreivedQuest.All(q => q.Name.Contains(updateName)));
        }

        [TestMethod()]
        public void DeleteQuestTest()
        {
            QuestDbConnection sut = ArrangeConnection();
            var created = sut.CreateQuest(new Quest());
            string newID = created.Id;
            var result = sut.DeleteQuest(newID);
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public void DeleteQuestTest_GetQuestsAndCheckId()
        {
            QuestDbConnection sut = ArrangeConnection();
            var created = sut.CreateQuest(new Quest());
            string newID = created.Id;
            var result = sut.DeleteQuest(newID);
            var quests = sut.GetAllQuests();
            Assert.IsTrue(!quests.Any(q => q.Id == newID));
        }

        [TestMethod()]
        public void DeleteQuestsTest()
        {
            QuestDbConnection sut = ArrangeConnection();

            List<Quest> questsCreate = new List<Quest>();
            for (int i = 0; i < 3; i++)
                questsCreate.Add(new Quest() { Name = $"{"toBeDeleted"}{i}" });

            var result = sut.CreateQuests(questsCreate);

            var deleteResults = sut.DeleteQuests(result);

            Assert.IsTrue(deleteResults.All(x => x.IsSuccess));
        }

        [TestMethod()]
        public void DeleteQuestsTest_StringIds()
        {
            QuestDbConnection sut = ArrangeConnection();

            List<Quest> questsCreate = new List<Quest>();
            for (int i = 0; i < 3; i++)
                questsCreate.Add(new Quest() { Name = $"{"toBeDeleted"}{i}" });

            var result = sut.CreateQuests(questsCreate);

            var deleteResults = sut.DeleteQuests(result.Select(q => q.Id).ToArray());

            Assert.IsTrue(deleteResults.All(x => x.IsSuccess));
        }
    }
}