using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DndCore.Map;
using Logic.Core.Creatures;

public abstract class CharacterQuantization : MonoBehaviour
{
    public class CreatureInfo {
        public ICreature Creature;
        public int X;
        public int Y;
    }
    public abstract ICreature GetCreature();
    
    public CreatureInfo ToMap()
    {
        var rotation = transform.localEulerAngles.y;
        rotation = Mathf.Round(rotation / 90) * 90;
        var creature = GetCreature();
        creature.Init();
        if (rotation == 0)
        {
            return new CreatureInfo { Creature = creature, X = 0, Y = 0 };
        }
        if (rotation == 90)
        {
            return new CreatureInfo { Creature = creature, X = 0, Y = 1 };
        }
        if (rotation == 180)
        {
            return new CreatureInfo { Creature = creature, X = 1, Y = 1 };
        }
        if (rotation == 270)
        {
            return new CreatureInfo { Creature = creature, X = 1, Y = 0 };
        }

        return null;
    }
}
