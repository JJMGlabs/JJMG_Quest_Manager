using Assets.QuestJournalApplication.QuestJournal;
using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.QuestSubObjects;
using QuestProgressionManager.Managers;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

using QuestJournal.UI;

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

    private QuestJournalTabController _tabController;

    public List<Quest> Quests;
    public List<QuestlineMetadata> Questlines;

    private bool _showQuestlines = false;
    private Button _questlineToggleButton;
    private Dictionary<string, bool> _questlineExpanded = new Dictionary<string, bool>();

    private QuestProgressionManagerClient ProgressionManager;
    private QuestlineManager _questlineManager;

    private SettingsManager.SettingEntry _questDbEntry;
    private SettingsManager.SettingEntry _questlineDbEntry;

    private void OnEnable()
    {
        InitializeSettings();
        InitializeDatabase();
        SetupUI();
        BindQuestList();

        if (Quests != null && Quests.Count > 0)
            BindQuestInfoPanel(Quests.First());

        _questList.selectionChanged += BindQuestInfoPanelSelectionChangedEvent;
    }

    private void InitializeSettings()
    {
        var settingsEntries = SettingsManager.Load();
        SettingsManager.SettingEntry questDbEntry = null;
        SettingsManager.SettingEntry questlineDbEntry = null;
        if (settingsEntries != null && settingsEntries.Count > 0)
        {
            questDbEntry = settingsEntries.Find(e => string.Equals(e.SectionName, Constants.SettingsSections.QuestDb, StringComparison.OrdinalIgnoreCase));
            questlineDbEntry = settingsEntries.Find(e => string.Equals(e.SectionName, Constants.SettingsSections.QuestlineDb, StringComparison.OrdinalIgnoreCase));
        }

        _questDbEntry = questDbEntry;
        _questlineDbEntry = questlineDbEntry;
    }

    private void InitializeDatabase()
    {
        if (_questDbEntry != null)
        {
            var dbConnPath = _questDbEntry.FolderPath;
            var dbCollectionName = Path.GetFileNameWithoutExtension(_questDbEntry.FileName);
            ProgressionManager = new QuestProgressionManagerClient(Application.dataPath + "//saveFiles", dbConnPath, dbCollectionName, _questDbEntry.Delimiter);
        }
        else
        {
            ProgressionManager = new QuestProgressionManagerClient(Application.dataPath + "//saveFiles", Application.dataPath, Constants.QuestDb.QuestDbJsonFile);
        }

        Quests = ProgressionManager.GetAllQuests();

        // Load questlines (if any)
        try
        {
            _questlineManager = new QuestlineManager(ProgressionManager, _questlineDbEntry?.FolderPath ?? string.Empty, _questlineDbEntry != null ? Path.GetFileNameWithoutExtension(_questlineDbEntry.FileName) : string.Empty, _questlineDbEntry?.Delimiter ?? string.Empty);
            Questlines = _questlineManager.GetAllQuestlines();
        }
        catch
        {
            Questlines = new List<QuestlineMetadata>();
            _questlineManager = new QuestlineManager(ProgressionManager);
        }
    }

    private void SetupUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _tabController = new QuestJournalTabController(root);

        _questList = root.Q<ListView>("QuestList");
        _questInfoPanel = root.Q<VisualElement>("QuestInfoPanel");

        _questlineToggleButton = root.Q<Button>("QuestlineToggleButton");
        if (_questlineToggleButton != null)
        {
            _questlineToggleButton.clicked += () =>
            {
                _showQuestlines = !_showQuestlines;
                BindQuestList();
            };
        }

        // Show toggle if questlines exist
        if (_questlineToggleButton != null)
            _questlineToggleButton.style.display = (Questlines != null && Questlines.Count > 0) ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void BindQuestInfoPanel(Quest quest)
    {
        if (quest == null)
            return;

        _questInfoPanel.Q<Label>("QuestName").text = quest.Name;
        _questInfoPanel.Q<ScrollView>("Description").contentContainer.Q<Label>("DescriptionBody").text = quest.Description;

        BindSubObjectList(_questInfoPanel.Q<ListView>("Prerequisites"), quest.QuestPrerequisites);
        BindSubObjectList(_questInfoPanel.Q<ListView>("Measurements"), quest.QuestMeasurements);
        BindSubObjectList(_questInfoPanel.Q<ListView>("Outcomes"), quest.QuestOutcomes);
    }

    private void BindQuestInfoPanelSelectionChangedEvent(IEnumerable<object> selectedItems)
    {
        if (selectedItems == null || !selectedItems.Any())
            return;

        var first = selectedItems.First();
        if (first is Quest q)
        {
            BindQuestInfoPanel(q);
            return;
        }

        var wrapper = first as QuestListItemWrapper;
        if (wrapper != null)
        {
            if (wrapper.IsHeader)
            {
                return;
            }

            BindQuestInfoPanel(wrapper.Quest);
            return;
        }
    }

    private void BindQuestList()
    {
        var uiDocument = GetComponent<UIDocument>();
        _questList.makeItem = m_QuestListItemAsset.CloneTree;

        if (_showQuestlines && Questlines != null && Questlines.Count > 0)
        {
            // Build grouped source
            var wrapped = BuildGroupedQuestList();
            _questList.itemsSource = wrapped;
            Action<VisualElement, int> bindItem = (e, i) =>
            {
                var src = _questList.itemsSource as System.Collections.IList;
                if (src == null || i < 0 || i >= src.Count)
                    return;
                var item = src[i] as QuestListItemWrapper;
                if (item == null)
                    return;
                var headerElement = e.Q<VisualElement>("QuestName");
                headerElement.userData = item.HeaderId;
                headerElement.UnregisterCallback<PointerDownEvent>(OnHeaderPointerDown);
                headerElement.RegisterCallback<PointerDownEvent>(OnHeaderPointerDown);
                var chevronLabel = e.Q<Label>("Chevron");
                var nameLabel = e.Q<Label>("QuestName");
                if (item.IsHeader)
                {
                    nameLabel.text = item.HeaderTitle;
                    var expanded = false;
                    _questlineExpanded.TryGetValue(item.HeaderId, out expanded);
                    if (chevronLabel != null)
                        chevronLabel.text = expanded ? "▾" : "▸";
                }
                else
                {
                    if (chevronLabel != null)
                        chevronLabel.text = "";
                    nameLabel.text = item.Quest?.Name ?? "";
                }
            };
            _questList.bindItem = bindItem;
        }
        else
        {
            _questList.itemsSource = Quests;
            Action<VisualElement, int> bindItem = (e, i) =>
            {
                var src = _questList.itemsSource as System.Collections.IList;
                if (src == null || i < 0 || i >= src.Count)
                    return;
                var q = src[i] as Quest;
                e.Q<Label>("QuestName").text = q?.Name ?? string.Empty;
            };
            _questList.bindItem = bindItem;
        }

        _questList.selectionChanged -= Debug.Log;
        _questList.selectionChanged += Debug.Log;

        if (Constants.DbBindingSettings.RebuildUpdate)
            StartCoroutine("RebuildRoutine");
    }

    private List<QuestListItemWrapper> BuildGroupedQuestList()
    {
        var grouper = new QuestListGrouper(Questlines, _questlineManager, _questlineExpanded);
        return grouper.BuildGroupedList(Quests);
    }

    private void BindSubObjectList<T>(ListView targetList, List<T> bindvalue)
    {
        if (bindvalue == null || bindvalue.Count == 0)
        {
            targetList.itemsSource = null;
            targetList.style.display = DisplayStyle.None;
            return;
        }
        targetList.style.display = DisplayStyle.Flex;

        // Always set itemsSource first to ensure bind callbacks observe the current source
        targetList.itemsSource = bindvalue;

        Action<VisualElement, int> bindItem = null;

        if (typeof(T) == typeof(QuestMeasurement))
        {
            targetList.makeItem = m_MeasurementItemAsset.CloneTree;
            bindItem = (e, i) => BindMeasurementItem(e, i, targetList);
        }
        else if (typeof(T) == typeof(QuestOutcome))
        {
            targetList.makeItem = m_OutcomeItemAsset.CloneTree;
            bindItem = (e, i) => BindOutcomeItem(e, i, targetList);
        }
        else if (typeof(T) == typeof(QuestPrerequisite))
        {
            targetList.makeItem = m_PrereqItemAsset.CloneTree;
            bindItem = (e, i) => BindPrerequisiteItem(e, i, targetList);
        }
        else
            return;

        targetList.bindItem = bindItem;
        targetList.selectionChanged -= Debug.Log;  // Prevent multiple subscriptions
        targetList.selectionChanged += Debug.Log;
    }

    private void BindMeasurementItem(VisualElement e, int i, ListView listView)
    {
        var src = listView.itemsSource as System.Collections.IList;
        if (src == null || i < 0 || i >= src.Count) return;
        var item = src[i] as QuestMeasurement;
        if (item == null) return;
        e.Q<Label>("MeasureName").text = item.Name;
        e.Q<Label>("MeasureState").text = item.MeasurementReached ? "Completed" : "InProgress";
        e.Q<Label>("MeasureValue").text = "Measuring: " + item.Measurement;
        e.Q<Label>("MeasureProgress").text = "Progress: " + item.ProgressValue;
        e.Q<Label>("MeasureTarget").text = "Target: " + item.TargetValue;
    }

    private void BindOutcomeItem(VisualElement e, int i, ListView listView)
    {
        var src = listView.itemsSource as System.Collections.IList;
        if (src == null || i < 0 || i >= src.Count) return;
        var item = src[i] as QuestOutcome;
        if (item == null) return;
        e.Q<Label>("OutcomeName").text = item.Name;
        e.Q<Label>("Accepted").text = item.Accepted ? "Accepted" : "Available";
        e.Q<Label>("Repeats").text = item.RepeatOutcome ? "Repeating" : "";
        string measurementIds = (item.MeasurementDependancyIds != null && item.MeasurementDependancyIds.Count > 0)
            ? "Measuring: " + string.Join(", ", item.MeasurementDependancyIds)
            : "";
        e.Q<Label>("MeasurementDependancies").text = measurementIds;
    }

    private void BindPrerequisiteItem(VisualElement e, int i, ListView listView)
    {
        var src = listView.itemsSource as System.Collections.IList;
        if (src == null || i < 0 || i >= src.Count) return;
        var item = src[i] as QuestPrerequisite;
        if (item == null) return;
        e.Q<Label>("Name").text = item.Name;
        var Status = "Prerequisite has ";
        Status += item.isPrerequisiteMet ? "been met" : "is not met";
        Status += item.isPrerequisiteCanceled ? " but is canceled" : "";
        e.Q<Label>("Status").text = Status;
        e.Q<Label>("MeasureValue").text = "Measuring: " + item.Measurement;
        e.Q<Label>("MeasureProgress").text = "Progress: " + item.ProgressValue;
        e.Q<Label>("MeasureTarget").text = "Target: " + item.TargetValue;
    }

    private void OnHeaderPointerDown(PointerDownEvent evt)
    {
        var element = evt.currentTarget as VisualElement;
        var id = element?.userData as string;
        if (string.IsNullOrEmpty(id))
            return;

        var current = false;
        _questlineExpanded.TryGetValue(id, out current);
        _questlineExpanded[id] = !current;
        BindQuestList();
        evt.StopImmediatePropagation();
    }

    IEnumerator RebuildRoutine()
    {
        for (; ; )
        {
            if (_questList != null)
                _questList.Rebuild();
            yield return new WaitForSeconds(Constants.DbBindingSettings.RebuildTimerSeconds);
        }
    }
}
