using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.XR.Oculus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using static TileManager;
using System;
using System.Text;
using Logic.Core.Map.Impl;
using DndCore.Map;
using Logic.Core.Creatures;
using Logic.Core.Creatures.Bestiary;
using DndCore.DI;
using DndCore.Map;
using DndCore.Utils.Log;
using Logic.Core.Actions;
using Logic.Core.Battle;
using Logic.Core.Battle.Actions;
using Logic.Core.Battle.Actions.Attacks;
using Logic.Core.Battle.Actions.Movement;
using Logic.Core.Battle.Actions.Spells;
using Logic.Core.Creatures;
using Logic.Core.Creatures.Abilities.Spells;
using Logic.Core.Creatures.Bestiary;
using Logic.Core.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Assets.Scripts.Jobs;

public class GameManager : MonoBehaviour
{
    public ActionsManager ActionsManager;
    public UIManager UIManager;
    public IDndBattle Battle;
    public IMap map;
    private List<int> Initiatives;
    TextMeshProUGUI Log;
    public IEnumerator StartGame(IMap map)
    {
        Log = GameObject.FindGameObjectsWithTag("Log")[0].GetComponent<TextMeshProUGUI>();
        //Log.text += "\nSTART GAME";
        Battle = DndModule.Get<IDndBattle>();
        yield return StartCoroutine(UIManager.DrawMap(map));
        Initiatives = Battle.Init(map);
        StartTurn();
    }

    private void StartTurn()
    {
        var creature = Battle.GetCreatureInTurn();
        ActionsManager.SetActions(new List<IAvailableAction>());
        if (creature.Loyalty == Loyalties.Ally)
        {
            this.StartCoroutine(SetAvailableActions());
        }
        else
        {
            this.StartCoroutine(AIPlay());
        }
    }

    IEnumerator SetAvailableActions()
    {
        var jobData = new AvailableActionsJob();

        JobHandle handle = jobData.Schedule();

        while (!handle.IsCompleted)
        {
            yield return null;
        }

        handle.Complete();

        ActionsManager.SetActions(Battle.GetAvailableActions());
    }

    IEnumerator AIPlay()
    {
        //Log.text += "\nAIPlay 1";
        var jobData = new AIPlayJob();

        JobHandle handle = jobData.Schedule();

        while (!handle.IsCompleted)
        {
            yield return null;
        }

        //Log.text += "\nAIPlay 2";
        handle.Complete();

        //Log.text += "\nAIPlay 3";
        yield return StartCoroutine(UIManager.ShowGameEvents(Battle.Events));

        Battle.NextTurn();
        StartTurn();
    }
}
