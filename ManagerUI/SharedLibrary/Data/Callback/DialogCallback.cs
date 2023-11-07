using SharedLibrary.Data.Enum;

namespace SharedLibrary.Data.Callback
{
    public class DialogCallback<T>
    {
        public DialogState CallbackState;
        public T CallbackObject;

        public DialogCallback()
        {

        }

        public DialogCallback(T callbackObject, DialogState callbackState = DialogState.NULL)
        {
            this.CallbackState = callbackState;
            CallbackObject = callbackObject;
        }
    }
}
