using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public Vector3 fixedRotation;
    public bool isFixingZXRotation;

    private void Update()
    {
        if(isFixingZXRotation)
        {
            this.transform.eulerAngles = new Vector3(fixedRotation.x, this.transform.eulerAngles.y, fixedRotation.z);
        }
    }
}
