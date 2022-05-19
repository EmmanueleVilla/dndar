using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    private Vector3 position;

    void Start()
    {
        position = this.transform.position;
    }

    public void UpdatePosition(Vector3 newPos) {
        position = newPos;
    }

    public void MoveWithDelta(Vector3 delta) {
        this.transform.position = position + delta;
    }

}
