using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public enum GameStates {
        Menu,
        NewMap,
        EditTile,
        InsertTile,
        EditMap
    }
    public Transform rayAnchor;

    private bool debouncing = false;
    private float lastHit = 0;
    private float debounce = 0.5f;

    private bool movingRoot = false;
    private Vector3 rigthHandStartingPos = Vector3.zero;

    public GameStates GameState;
    public MenuManager MenuManager;

    public TextMeshProUGUI Log;

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

    bool rotatingRootClockwise = false;
    bool rotatingRootCounterClockwise = false;
    bool movingMap = false;
    float delay = 1.0f;
    float startDelay = 0.0f;

    void Update()
    {
        var buttonOne = OVRInput.Get(OVRInput.Button.One);
        var buttonTwo = OVRInput.Get(OVRInput.Button.Two);

        Log.text = "GameState? " + GameState;
        /*
        if (GameState == GameStates.NewMap) {
            if(OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch) > 0.5f) {
                GameState = GameStates.InsertTile;
                MenuManager.InsertTileMode();
            }
        }
        */

        if(GameState == GameStates.InsertTile) {
            if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch) < 0.1f) {
                GameState = GameStates.NewMap;
                MenuManager.EditTileMode();
            }
        }

        if(GameState == GameStates.EditMap) {
            if (buttonOne && !buttonTwo  && !rotatingRootClockwise) {
                startDelay = Time.timeSinceLevelLoad;
                movingMap = false;
                rotatingRootClockwise = true;
                rotatingRootCounterClockwise = false;
                rigthHandStartingPos = rayAnchor.position;
            }
            if(buttonTwo && !buttonOne && !rotatingRootCounterClockwise) {
                startDelay = Time.timeSinceLevelLoad;
                movingMap = false;
                rotatingRootClockwise = false;
                rotatingRootCounterClockwise = true;
                rigthHandStartingPos = rayAnchor.position;
            }
            if(buttonOne && buttonTwo && !movingMap) {
                startDelay = Time.timeSinceLevelLoad;
                movingMap = true;
                rotatingRootClockwise = false;
                rotatingRootCounterClockwise = false;
            }

            if(!buttonOne && !buttonTwo) {
                movingMap = false;
                rotatingRootClockwise = false;
                rotatingRootCounterClockwise = false;
                rigthHandStartingPos = rayAnchor.position;
            }

            if (rotatingRootClockwise && (Time.timeSinceLevelLoad - startDelay) > delay) {
                var mapRoot = GameObject.FindGameObjectWithTag("MapRoot").GetComponent<MapManager>();
                mapRoot.transform.Rotate(Vector3.up * 1.0f);
            }

            if (rotatingRootCounterClockwise && (Time.timeSinceLevelLoad - startDelay) > delay) {
                var mapRoot = GameObject.FindGameObjectWithTag("MapRoot").GetComponent<MapManager>();
                mapRoot.transform.Rotate(Vector3.up * -1.0f);
            }

            if (movingMap && (Time.timeSinceLevelLoad - startDelay) > delay) {
                var delta = rayAnchor.position - rigthHandStartingPos;
                var mapRoot = GameObject.FindGameObjectWithTag("MapRoot").GetComponent<MapManager>();
                mapRoot.MoveWithDelta(delta);
            }
        }

        if (Time.realtimeSinceStartup - lastHit > debounce) {
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
