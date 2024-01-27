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


    void LateUpdate()
    {
        if(_questList != null)
        _questList.Rebuild();
    }

    //Add logic that interacts with the UI controls in the `OnEnable` methods
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
    }
}
