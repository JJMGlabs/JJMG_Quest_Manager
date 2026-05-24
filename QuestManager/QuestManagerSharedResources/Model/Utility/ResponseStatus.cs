namespace QuestManagerSharedResources.Model.Utility
{
    /// <summary>
    /// The result of a write operation, indicating success or failure and providing a message.
    /// </summary>
    public class ResponseStatus
    {
        public bool IsSuccess { get; private set; }
        public string ResponseMessage { get; private set; }
        public ResponseStatus(bool isSuccess, string responseMessage)
        {
            IsSuccess = isSuccess;
            ResponseMessage = responseMessage;
        }
    }
}
