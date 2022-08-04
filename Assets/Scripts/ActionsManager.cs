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
using Logic.Core.Battle;
using Logic.Core.Battle.Actions;
using Logic.Core.Battle.Actions.Attacks;
using Logic.Core.Battle.Actions.Movement;
using Logic.Core.Battle.Actions.Spells;
using UnityEngine.UI;
using System.Diagnostics;

public class ActionsManager : MonoBehaviour
{
    public GameManager GameManager;
    public GameObject Play;
    public GameObject[] Buttons;

    List<IAvailableAction> Actions;
    public void Start()
    {
        foreach (var button in Buttons)
        {
            button.gameObject.SetActive(false);
        }
    }

    public void SetActions(List<IAvailableAction> actions)
    {
        Play.SetActive(false);
        Actions = actions;
        foreach (var button in Buttons)
        {
            button.gameObject.SetActive(false);
        }

        for (int i = 0; i < actions.Count && i < Buttons.Length; i++)
        {
            Buttons[i].gameObject.SetActive(true);
            Buttons[i].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = actions[i].Description;
        }
        Play.SetActive(true);
    }

    public TextMeshProUGUI Log;
    public void SelectAction(int index)
    {
        Log.text += "\nSelect action " + index + " from " + (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name;
        switch (Actions[index].ActionType)
        {
            case ActionsTypes.RequestMovement:
                GameManager.EnterMovementMode();
                break;
            case ActionsTypes.CancelMovement:
                Log.text += "\nCancelMovement";
                GameManager.ExitMovementMode();
                break;
            case ActionsTypes.ConfirmMovement:
                var action = Actions[index] as ConfirmMovementAction;
                //this.StartCoroutine(GameManager.ConfirmMovement(action.DestinationX, action.DestinationY, action.Damage, action.Speed));
                break;
            case ActionsTypes.EndTurn:
                //GameManager.NextTurn();
                break;
            case ActionsTypes.RequestAttack:
                //GameManager.EnterAttackMode(Actions[index] as RequestAttackAction);
                break;
            case ActionsTypes.CancelAttack:
                //GameManager.ExitAttackMode();
                break;
            case ActionsTypes.ConfirmAttack:
                var confirmAttackAction = Actions[index] as ConfirmAttackAction;
                //this.StartCoroutine(GameManager.ConfirmAttack(confirmAttackAction));
                break;
            case ActionsTypes.RequestSpell:
                //GameManager.EnterSpellMode(Actions[index] as RequestSpellAction);
                break;
            case ActionsTypes.ConfirmSpell:
                //this.StartCoroutine(GameManager.ConfirmSpell(Actions[index] as ConfirmSpellAction));
                break;
            case ActionsTypes.CancelSpell:
                //GameManager.ExitSpellMode();
                break;
            default:
                //GameManager.UseAbility(Actions[index]);
                break;
        }
    }
}
