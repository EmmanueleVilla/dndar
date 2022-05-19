using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public Transform rayAnchor;

    private bool debouncing = false;
    private float lastHit = 0;
    private float debounce = 0.5f;

    void Update()
    {
        if(Time.realtimeSinceStartup - lastHit > debounce) {
            debouncing = false;
        }
        if (OVRInput.Get(OVRInput.Button.One) && !debouncing) {
            RaycastHit hit;

            if (Physics.Raycast(rayAnchor.position, rayAnchor.forward, out hit)) {
                var collider = hit.transform.gameObject.GetComponent<TileCollider>();
                if (collider != null) {
                    collider.Rotate();
                }
            }
            debouncing = true;
            lastHit = Time.realtimeSinceStartup;
        }
    }
}
