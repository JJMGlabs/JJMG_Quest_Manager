using UnityEngine;
using UnityEngine.UIElements;

namespace QuestJournal.UI
{
    public class QuestJournalTabController
    {
        private readonly VisualElement _root;
        private readonly Button _questsTabButton;
        private readonly Button _settingsTabButton;
        private readonly VisualElement _questsTabContent;
        private readonly VisualElement _settingsTabContent;
        private SettingsPageController _settingsPageController;

        public QuestJournalTabController(VisualElement root)
        {
            _root = root;
            _questsTabButton = _root.Q<Button>("QuestsTabButton");
            _settingsTabButton = _root.Q<Button>("SettingsTabButton");
            _questsTabContent = _root.Q<VisualElement>("QuestsTabContent");
            _settingsTabContent = _root.Q<VisualElement>("SettingsTabContent");

            _questsTabButton.clicked += ShowQuestsTab;
            _settingsTabButton.clicked += ShowSettingsTab;

            ShowQuestsTab(); // Default to Quests tab
        }

        private void ShowQuestsTab()
        {
            _questsTabContent.style.display = DisplayStyle.Flex;
            _settingsTabContent.style.display = DisplayStyle.None;
            
            _questsTabButton.RemoveFromClassList("tabButton--inactive");
            _questsTabButton.AddToClassList("tabButton--active");
            
            _settingsTabButton.RemoveFromClassList("tabButton--active");
            _settingsTabButton.AddToClassList("tabButton--inactive");
        }

        private void ShowSettingsTab()
        {
            _questsTabContent.style.display = DisplayStyle.None;
            _settingsTabContent.style.display = DisplayStyle.Flex;
            
            _questsTabButton.RemoveFromClassList("tabButton--active");
            _questsTabButton.AddToClassList("tabButton--inactive");
            
            _settingsTabButton.RemoveFromClassList("tabButton--inactive");
            _settingsTabButton.AddToClassList("tabButton--active");

            // Lazily load settings controller when tab is opened
            if (_settingsPageController == null)
            {
                var settingsPageElement = _settingsTabContent.Q<VisualElement>("SettingsPage");
                if (settingsPageElement != null)
                {
                    _settingsPageController = new SettingsPageController(settingsPageElement);
                }
            }
        }
    }
}
