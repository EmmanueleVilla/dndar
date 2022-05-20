using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedYUI : MonoBehaviour
{

    public int Z = 1;
    void Update()
    {
        this.transform.position = new Vector3(this.transform.position.x, 0, this.transform.position.z);
    }
}
