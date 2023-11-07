# Quest Manager API

This is the core management part of the quest system, the code here is used to create quests and questlines. This will not usally be used in a client application to prevent breaking the game(Instead use the QuestManagerClientAPI)

The IQuestDbConnection is the blueprint for any type of database connection, a json file is the likely default, but much more complex db connections can be created.