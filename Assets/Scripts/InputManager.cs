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

    private bool movingRoot = false;
    private Vector3 rigthHandStartingPos = Vector3.zero;

    void HandleButtonOnePressed() {
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

    void Update()
    {
        if(Time.realtimeSinceStartup - lastHit > debounce) {
            debouncing = false;
        }

        var primaryHandTrigger = OVRInput.Get(OVRInput.Button.Two);

        if(!primaryHandTrigger && movingRoot) {
            var mapRoot = GameObject.FindGameObjectWithTag("MapRoot").GetComponent<MapManager>();
            mapRoot.UpdatePosition(mapRoot.transform.position);
            movingRoot = false;
        }

        if (OVRInput.Get(OVRInput.Button.One) && !debouncing) {
            HandleButtonOnePressed();
        } else if (primaryHandTrigger) {
            RaycastHit hit;
            if (movingRoot) {
                var delta = rayAnchor.position - rigthHandStartingPos;
                var mapRoot = GameObject.FindGameObjectWithTag("MapRoot").GetComponent<MapManager>();
                mapRoot.MoveWithDelta(delta);
            }
            else if(Physics.Raycast(rayAnchor.position, rayAnchor.forward, out hit)) {
                var collider = hit.transform.gameObject.GetComponent<TileCollider>();
                if (collider != null && !movingRoot && primaryHandTrigger) {
                    movingRoot = true;
                    rigthHandStartingPos = rayAnchor.position;
                }
            }
        }
    }
}
