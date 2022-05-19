using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCollider : MonoBehaviour
{
    public GameObject TileManager;

    public void Rotate()
    {
        TileManager.transform.Rotate(Vector3.up * 90);
    }
}
