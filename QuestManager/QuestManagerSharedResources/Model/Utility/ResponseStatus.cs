namespace QuestManagerSharedResources.Model.Utility
{
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
