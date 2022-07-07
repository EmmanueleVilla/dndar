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
        //Log.text += "\nBattle " + Battle;
        //GameStarted?.Invoke(this, EventArgs.Empty);
        //Log.text += "\nDrawing map";
        yield return StartCoroutine(UIManager.DrawMap(map));
        Initiatives = Battle.Init(map);
        //InitiativesRolled?.Invoke(this, Initiatives);
        StartTurn();
    }

    private void StartTurn()
    {
        //Log.text += "\nStartTurn 1";
        var creature = Battle.GetCreatureInTurn();
        //Log.text += "\nStartTurn 2";
        //Log.text += "\nStart turn of " + creature.GetType().ToString().Split('.').Last();
        //TurnStarted?.Invoke(this, creature);
        ActionsManager.SetActions(new List<IAvailableAction>());
        //Log.text += "\nStartTurn 3";
        this.StartCoroutine(AIPlay());
        /*
        if (creature.Loyalty == Loyalties.Ally)
        {
            //Log.text += "\nStartTurn ally";
            //this.StartCoroutine(SetAvailableActions());
        }
        else
        {
            //Log.text += "\nStartTurn AI";
            this.StartCoroutine(AIPlay());
        }
        */
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
