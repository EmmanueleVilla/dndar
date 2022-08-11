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
using System.Diagnostics;

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

    public IEnumerator ConfirmMovement(int destinationX, int destinationY, int damage, int speed)
    {
        ActionsManager.SetActions(new List<IAvailableAction>());
        var end = NextMovementAvailableCells.First(edge =>
            edge.Destination.X == destinationX
            && edge.Destination.Y == destinationY
            && edge.CanEndMovementHere == true
            && edge.Damage == damage
            && edge.Speed == speed
            );
        var GameEvents = Battle.MoveTo(end);
        yield return StartCoroutine(UIManager.ShowGameEvents(GameEvents));
        ExitMovementMode();
    }

    public void NextTurn()
    {
        Battle.NextTurn();
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

    #region SPELLS

    bool InSpellMode = false;
    ISpell spell = null;
    public void EnterSpellMode(RequestSpellAction action)
    {
        InSpellMode = true;
        spell = action.Spell;
        ActionsManager.SetActions(
            new List<IAvailableAction>() {
                new CancelSpellAction()
            });
        UIManager.HighlightCells(action.ReachableCells);
    }

    internal void ExitSpellMode()
    {
        InSpellMode = false;
        ActionsManager.SetActions(new List<IAvailableAction>());
        this.StartCoroutine(SetAvailableActions());
        UIManager.ResetCellsUI();
    }

    internal IEnumerator ConfirmSpell(ConfirmSpellAction confirmSpellAction)
    {
        ActionsManager.SetActions(new List<IAvailableAction>());
        var events = Battle.Spell(confirmSpellAction);
        yield return this.StartCoroutine(UIManager.ShowGameEvents(events));
        ExitSpellMode();
    }

    #endregion

    #region ATTACK
    bool InAttackMode = false;
    RequestAttackAction requestedAttack;

    public void EnterAttackMode(RequestAttackAction action)
    {
        InAttackMode = true;
        requestedAttack = action;
        ActionsManager.SetActions(
            new List<IAvailableAction>()
            {
                new CancelAttackAction()
            });
        UIManager.HighlightCells(action.ReachableCells);
    }

    public void ExitAttackMode()
    {
        InAttackMode = false;
        ActionsManager.SetActions(new List<IAvailableAction>());
        this.StartCoroutine(SetAvailableActions());
        UIManager.ResetCellsUI();
    }

    internal IEnumerator ConfirmAttack(ConfirmAttackAction confirmAttackAction)
    {
        ActionsManager.SetActions(new List<IAvailableAction>());
        var events = Battle.Attack(confirmAttackAction);
        yield return this.StartCoroutine(UIManager.ShowGameEvents(events));
        ExitAttackMode();
    }

    #endregion

    #region MOVEMENT

    internal void UseAbility(IAvailableAction availableAction)
    {
        Battle.UseAbility(availableAction);
        ActionsManager.SetActions(new List<IAvailableAction>());
        this.StartCoroutine(SetAvailableActions());
    }
    bool InMovementMode = false;
    public void EnterMovementMode()
    {
        InMovementMode = true;
        ActionsManager.SetActions(
            new List<IAvailableAction>() {
                new CancelMovementAction()
            });
        StartMovementMode();
    }
    List<MemoryEdge> NextMovementAvailableCells = new List<MemoryEdge>();
    void StartMovementMode()
    {
        NextMovementAvailableCells = Battle.GetReachableCells().Where(x => x.Speed > 0 && x.CanEndMovementHere).ToList();
        UIManager.HighlightMovement(NextMovementAvailableCells);
    }

    public void ExitMovementMode()
    {
        Log.text += "\nExitMovementMode from " + (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name;
        InMovementMode = false;
        ActionsManager.SetActions(new List<IAvailableAction>());
        this.StartCoroutine(SetAvailableActions());
        UIManager.ResetCellsUI();
        NextMovementAvailableCells.Clear();
    }
    #endregion

    public Material[] Colors;
    public void OnCellClicked(int x, int y)
    {
        Log.text += "\nOnCellClicked " + x + "," + y;
        if (InSpellMode)
        {
            var actions = new List<IAvailableAction>();
            actions.Add(new ConfirmSpellAction(Battle.GetCreatureInTurn().Id, spell)
            {
                Target = map.GetCellInfo(y, x)
            }); ;
            actions.Add(new CancelSpellAction());
            ActionsManager.SetActions(actions);
        }

        if (InAttackMode)
        {
            var creature = map.GetOccupantCreature(y, x);
            if (creature != null)
            {
                var actions = new List<IAvailableAction>();
                actions.Add(new ConfirmAttackAction()
                {
                    TargetCreature = creature.Id,
                    AttackingCreature = Battle.GetCreatureInTurn().Id,
                    Attack = requestedAttack.Attack,
                    ActionEconomy = requestedAttack.ActionEconomy
                });
                actions.Add(new CancelAttackAction());
                ActionsManager.SetActions(actions);
            }
        }
        if (InMovementMode && NextMovementAvailableCells.Any(edge => edge.Destination.X == y && edge.Destination.Y == x))
        {
            Log.text += "\nInMovementMode";
            //check if there are multiple paths
            var ends = NextMovementAvailableCells.Where(edge => edge.Destination.X == y && edge.Destination.Y == x).OrderBy(x => x.Damage).ToList();
            var actions = new List<IAvailableAction>();
            UIManager.ResetCellsUI();
            UIManager.HighlightMovement(NextMovementAvailableCells);
            int index = 0;
            foreach (var end in ends)
            {
                UIManager.ShowPath(Battle.GetPathTo(end), end, Colors[index]);
                actions.Add(new ConfirmMovementAction() { Damage = end.Damage, DestinationX = end.Destination.X, DestinationY = end.Destination.Y, Speed = end.Speed });
                index++;
                if (index == Colors.Count())
                {
                    index = 0;
                }
            }
            actions.Add(new CancelMovementAction());
            ActionsManager.SetActions(actions);
        }
    }
}
