using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferencePlaneCollider : MonoBehaviour
{
    public GameObject RightHandRoot;

    Vector3 fixedPosition;
    Vector3 handStartPosition;

    bool referenceMode;

    public void EnterReferenceMode()
    {
        fixedPosition = this.transform.localPosition;
        handStartPosition = RightHandRoot.transform.position;
        referenceMode = true;
    }

    public void ExitReferenceMode()
    {
        referenceMode = false;
    }

    private void Update()
    {
        if (referenceMode)
        {
            var delta = RightHandRoot.transform.position.y - handStartPosition.y;
            delta = Mathf.Round(delta * 20) / 20;
            this.transform.localPosition = new Vector3(
                fixedPosition.x,
                fixedPosition.y + delta,
                fixedPosition.z
                );
        }
    }
}
