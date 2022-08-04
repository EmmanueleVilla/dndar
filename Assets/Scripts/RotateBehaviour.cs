using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBehaviour : MonoBehaviour
{
    void Update()
    {
        this.transform.Rotate(new Vector3(0, 1, 0) * 50 * Time.deltaTime);
    }
}
