using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{

    public enum TileTypes
    {
        Sewer_Floor,
        Sewer_Wall,
        Sewer_PipedWall,
        Sewer_Sluice_Straight,
        Sewer_Sluice_Curve,
        Sewer_Sluice_End,
        Sewer_Sluice_Cross,
        Sewer_Angle_Convex,
        Sewer_Angle_Concave,
        Sewer_Ladder,
        RoughStone_Arc,
        RoughStone_Angle_Concave,
        RoughStone_Angle_Convex,
        RoughStone_Door,
        RoughStone_Floor,
        RoughStone_Stairs_High,
        RoughStone_Stairs_Low,
        RoughStone_Wall,
        None,
        Objects_Coffin,
        Objects_Column,
        Objects_Crate,
        Objects_Crates,
        Objects_Plates,
        Objects_Statue,
        Character_Monk,
        Character_Ranger,
        Character_Wizard,
        Character_Fighter
    }

    public TileTypes TileType;

    public int Snap;
}
