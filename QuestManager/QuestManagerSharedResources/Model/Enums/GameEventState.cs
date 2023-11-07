namespace QuestManagerSharedResources.Model.Enums
{
    //Describes the state of in game quests
    public enum GameEventState
    {
        Triggered,//Event has triggered
        Suspended,//The Event has been suspended and will not complete after being triggered
        Completed,//The Event has completed
        Failed//The event has failed or will fail to trigger
    };
}
