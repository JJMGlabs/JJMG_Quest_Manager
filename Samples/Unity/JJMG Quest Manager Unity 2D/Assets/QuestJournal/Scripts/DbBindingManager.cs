using Assets.QuestJournalApplication.QuestJournal;
using QuestManagerApi.Controllers;
using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.QuestSubObjects;
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
    VisualTreeAsset m_QuestListItemAsset;

    [SerializeField]
    VisualTreeAsset m_MeasurementItemAsset;
    [SerializeField]
    VisualTreeAsset m_OutcomeItemAsset;
    [SerializeField]
    VisualTreeAsset m_PrereqItemAsset;

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
        
        BindSubObjectList(_questInfoPanel.Q<ListView>("Measurements"), quest.QuestMeasurements);
        BindSubObjectList(_questInfoPanel.Q<ListView>("Outcomes"), quest.QuestOutcomes);
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
        _questList.makeItem = m_QuestListItemAsset.CloneTree;
        _questList.bindItem = bindItem;
        _questList.itemsSource = Quests;
        _questList.selectionChanged += Debug.Log;

        if (Constants.DbBindingSettings.RebuildUpdate)
            StartCoroutine("RebuildRoutine");
    }

    //private void BindSubObjectList(ListView targetList, List<QuestMeasurement> bindvalue)
    //{
    //    if (bindvalue == null)
    //    {
    //        targetList.itemsSource = null;
    //        targetList.style.display = DisplayStyle.None;
    //        return;
    //    }
    //    targetList.style.display = DisplayStyle.Flex;
    //    Action<VisualElement, int> bindItem = (e, i) => BindMeasureItem(e, i, bindvalue);
    //    targetList.makeItem = m_MeasurementItemAsset.CloneTree;
    //    targetList.bindItem = bindItem;
    //    targetList.itemsSource = bindvalue;
    //    targetList.selectionChanged += Debug.Log;
    //}
    private void BindSubObjectList<T>(ListView targetList, List<T> bindvalue)
    {
        if (bindvalue == null)
        {
            targetList.itemsSource = null;
            targetList.style.display = DisplayStyle.None;
            return;
        }

        targetList.style.display = DisplayStyle.Flex;

        Action<VisualElement, int> bindItem;

        if (typeof(T) == typeof(QuestMeasurement))
        {
            bindItem = (e, i) => BindMeasureItem(e, i, bindvalue as List<QuestMeasurement>);
            targetList.makeItem = m_MeasurementItemAsset.CloneTree;
        }
        else if (typeof(T) == typeof(QuestOutcome))
        {
            bindItem = (e, i) => BindOutcomeItem(e, i, bindvalue as List<QuestOutcome>);
            targetList.makeItem = m_OutcomeItemAsset.CloneTree;
        }
        else if (typeof(T) == typeof(QuestPrerequisite))
        {
            bindItem = (e, i) => BindPrerequisiteItem(e, i, bindvalue as List<QuestPrerequisite>);
            targetList.makeItem = m_PrereqItemAsset.CloneTree;
        }
        else
            return;


        
        targetList.bindItem = bindItem;
        targetList.itemsSource = bindvalue;
        targetList.selectionChanged -= Debug.Log;  // Prevent multiple subscriptions
        targetList.selectionChanged += Debug.Log;
    }

    private void BindMeasureItem(VisualElement element, int index, List<QuestMeasurement> bindvalue)
    {
        element.Q<Label>("MeasureName").text = bindvalue[index].Name;
        element.Q<Label>("MeasureState").text = bindvalue[index].MeasurementReached ? "Completed":"InProgress";
        element.Q<Label>("MeasureValue").text = "Measuring: " + bindvalue[index].Measurement;
        element.Q<Label>("MeasureProgress").text = "Progress: " + bindvalue[index].ProgressValue;
        element.Q<Label>("MeasureTarget").text = "Target: " +bindvalue[index].TargetValue;
    }

    private void BindPrerequisiteItem(VisualElement element, int index, List<QuestPrerequisite> bindvalue)
    {
        element.Q<Label>("MeasureName").text = bindvalue[index].Name;
        var Status = "Prerequisite has ";
        Status += bindvalue[index].isPrerequisiteMet ? "been met" : "is not met";
        Status += bindvalue[index].isPrerequisiteCanceled ? " but is canceled" : "";
        element.Q<Label>("Status").text = Status;
        element.Q<Label>("MeasureValue").text = "Measuring: " + bindvalue[index].Measurement;
        element.Q<Label>("MeasureProgress").text = "Progress: " + bindvalue[index].ProgressValue;
        element.Q<Label>("MeasureTarget").text = "Target: " + bindvalue[index].TargetValue;
    }

    private void BindOutcomeItem(VisualElement element, int index, List<QuestOutcome> bindvalue)
    {
        element.Q<Label>("OutcomeName").text = bindvalue[index].Name;
        element.Q<Label>("Accepted").text = bindvalue[index].Accepted ? "Accepted" : "Available";
        element.Q<Label>("Repeats").text = bindvalue[index].RepeatOutcome ? "Repeating" : "";
        element.Q<Label>("MeasurementDependancies").text = "Measuring: " + bindvalue[index].MeasurementDependancyIds[0];
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
