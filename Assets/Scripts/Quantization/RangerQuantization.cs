using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DndCore.Map;
using Logic.Core.Creatures;
using Logic.Core.Creatures.Bestiary;

public class RangerQuantization : CharacterQuantization
{
    public override ICreature GetCreature()
    {
        return new HumanMaleRanger();
    }
}
