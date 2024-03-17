using Assets.QuestJournalApplication.QuestJournal;
using QuestManagerApi.Controllers;
using QuestManagerSharedResources.Model;
using QuestProgressionManager.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class DbBindingManager : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset m_ItemAsset;

    private ListView _questList;
    private VisualElement _questInfoPanel;

    public List<Quest> Quests;

    private QuestProgressionManagerClient ProgressionManager;

    private void OnEnable()
    {
        ProgressionManager = new QuestProgressionManagerClient(Application.dataPath + "//saveFiles", Application.dataPath, Constants.QuestDb.QuestDbJsonFile);
        Quests = ProgressionManager.GetAllQuests();

        _questList = GetComponent<UIDocument>().rootVisualElement.Q<ListView>("QuestList");
        _questInfoPanel = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("QuestInfoPanel");

        BindQuestList();

        BindQuestInfoPanel(Quests.First());

        _questList.selectionChanged += BindQuestInfoPanelSelectionChangedEvent;
    }

    private void BindQuestInfoPanel(Quest quest)
    {
        if (quest == null)
            return;

        _questInfoPanel.Q<Label>("QuestName").text = quest.Name;
        _questInfoPanel.Q<ScrollView>("Description").contentContainer.Q<Label>("DescriptionBody").text = quest.Description;
    }

    private void BindQuestInfoPanelSelectionChangedEvent(IEnumerable<object> selectedItems)
    {
        if (selectedItems == null && selectedItems.Count() <= 0)
            return;

        var firstSelectedItem = selectedItems.First() as Quest;
        BindQuestInfoPanel(firstSelectedItem);
    }

    private void BindQuestList()
    {
        Action<VisualElement, int> bindItem = (e, i) => e.Q<Label>("QuestName").text = Quests[i].Name;
        var uiDocument = GetComponent<UIDocument>();
        _questList.makeItem = m_ItemAsset.CloneTree;
        _questList.bindItem = bindItem;
        _questList.itemsSource = Quests;
        _questList.selectionChanged += Debug.Log;

        if (Constants.DbBindingSettings.RebuildUpdate)
            StartCoroutine("RebuildRoutine");
    }

    IEnumerator RebuildRoutine()
    {
        for (; ;)
        {
            if (_questList != null)
                _questList.Rebuild();
            yield return new WaitForSeconds(Constants.DbBindingSettings.RebuildTimerSeconds);
        }
    }
}
