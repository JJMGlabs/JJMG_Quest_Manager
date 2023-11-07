using QuestManagerSharedResources.Model;
using SharedLibrary.Data.Enum;

namespace SharedLibrary.Data.Callback
{
    public class QuestDialogCallback
    {
        public DialogState CallbackState;
        public Quest Quest;

        public QuestDialogCallback()
        {

        }

        public QuestDialogCallback(DialogState callbackState = DialogState.NULL, Quest quest = null)
        {
            this.CallbackState = callbackState;
            Quest = quest;
        }
    }
}
