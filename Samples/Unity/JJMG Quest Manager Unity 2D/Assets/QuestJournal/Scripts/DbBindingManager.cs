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

    public List<Quest> Quests;

    private QuestProgressionManagerClient ProgressionManager;

    private void OnEnable()
    {
        ProgressionManager = new QuestProgressionManagerClient(Application.dataPath + "//saveFiles", Application.dataPath, Constants.QuestDb.QuestDbJsonFile);
        Quests = ProgressionManager.GetAllQuests();

        _questList = GetComponent<UIDocument>().rootVisualElement.Q<ListView>("QuestList");

        BindQuestList();

        _questList.selectionChanged += BindQuestInfoPanelSelectionChangedEvent;

    }

    private void BindQuestInfoPanelSelectionChangedEvent(IEnumerable<object> selectedItems)
    {
        if (selectedItems == null && selectedItems.Count() <= 0)
            return;

        var firstSelectedItem = selectedItems.First() as Quest;
            //Pass the data of the Objects into the bindings

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
