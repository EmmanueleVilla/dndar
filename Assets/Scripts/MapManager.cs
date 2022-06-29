using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject RightHandRoot;
    public ReferencePlaneCollider[] ReferencePlanes;

    Vector3 fixedRotation;
    bool isFixingZXRotation;

    public void ResetRefPlane()
    {
        foreach (var collider in ReferencePlanes)
        {
            collider.Reset();
        }
    }

    public void EnterMovementMode()
    {
        ExitReferenceMode();
        fixedRotation = transform.eulerAngles;
        transform.parent = RightHandRoot.transform;
        isFixingZXRotation = true;
    }

    public void EnterReferenceMode()
    {
        ExitMovementMode();
        foreach (var collider in ReferencePlanes)
        {
            collider.EnterReferenceMode();
        }
    }

    public void ExitReferenceMode()
    {
        foreach (var collider in ReferencePlanes)
        {
            collider.ExitReferenceMode();
        }
    }

    public void ExitMovementMode()
    {
        transform.parent = null;
        isFixingZXRotation = false;
    }

    private void Update()
    {
        if (isFixingZXRotation)
        {
            this.transform.eulerAngles = new Vector3(fixedRotation.x, this.transform.eulerAngles.y, fixedRotation.z);
        }
    }

}
