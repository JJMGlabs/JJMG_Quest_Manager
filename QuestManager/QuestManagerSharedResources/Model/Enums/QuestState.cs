namespace QuestManagerSharedResources.Model.Enums
{
    //Tracks the state of quest progression
    //NOTE the concept for current and active is for quest setups where a quest is activly tracked while not a currently focused 
    public enum QuestState
    {
        NONE,//The quest is not trackable and is not available to players
        CURRENT,//The quest is ACTIVE but also has met all prerequisites
        FAILED,//The quest is failed and will not be tracked
        COMPLETE,//The quest is not trackable and player has completed it
        ACTIVE//Available for players and actively trackable(whether a player has unlocked its prerequisites or not)
    };
}
