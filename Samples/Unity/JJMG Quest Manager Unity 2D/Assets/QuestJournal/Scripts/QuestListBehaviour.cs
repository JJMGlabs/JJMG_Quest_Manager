using Assets.QuestJournalApplication.QuestJournal;
using QuestManagerApi.Controllers;
using QuestManagerSharedResources.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class QuestListBehaviour : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset m_ItemAsset;

    private ListView _questList;

    public List<Quest> Quests;

    //Frequent rebuild was presenting a noticeable impact on performance, coroutine restricts this to value of this timer
    private const float rebuildTimerSeconds = 2;

    private void OnEnable()
    {
        Quests = LocalDbQuestController.GetAllQuestsFromDatabase(Application.dataPath,Constants.QuestDb.QuestDbJsonFile);

        _questList = GetComponent<UIDocument>().rootVisualElement.Q<ListView>("QuestList");

        Action<VisualElement, int> bindItem = (e, i) => e.Q<Label>("QuestName").text = Quests[i].Name;
        var uiDocument = GetComponent<UIDocument>();
        _questList.makeItem = m_ItemAsset.CloneTree;
        _questList.bindItem = bindItem;
        _questList.itemsSource = Quests;    
        _questList.selectionChanged += Debug.Log;

        StartCoroutine("RebuildRoutine");
    }

    IEnumerator RebuildRoutine()
    {
        for (; ; )
        {
            if (_questList != null)
                _questList.Rebuild();
            yield return new WaitForSeconds(rebuildTimerSeconds);
        }
    }
}
