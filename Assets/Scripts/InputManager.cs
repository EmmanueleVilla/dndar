using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static TileManager;

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

    public TileTypes SelectedTile;

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
    float delay = 0.5f;
    float startDelay = 0.0f;
    GameObject selectedTilePrefab;

    bool prevOne = false;
    bool prevTwo = false;

    void Update()
    {
        var buttonOne = OVRInput.Get(OVRInput.Button.One);
        var buttonTwo = OVRInput.Get(OVRInput.Button.Two);

        if (GameState == GameStates.EditMap) {
            if(OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch) > 0.5f) {
                GameState = GameStates.InsertTile;
                MenuManager.InsertTileMode();
            }
        }

        if(GameState == GameStates.InsertTile) {
            if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch) < 0.1f) {
                Destroy(selectedTilePrefab);
                GameState = GameStates.NewMap;
                MenuManager.EditTileMode();
            }
            RaycastHit hit;
            if (buttonOne) {
                if (Physics.Raycast(rayAnchor.position, rayAnchor.forward, out hit)) {
                    var collider = hit.transform.gameObject.GetComponent<TileCollider>();
                    if (collider != null) {
                        SelectedTile = collider.TileManager.GetComponent<TileManager>().TileType;
                        Destroy(selectedTilePrefab);
                        var group = MenuManager.Tiles.FirstOrDefault(x => x.Tiles.Select(tile => tile.TileType).Contains(SelectedTile));
                        if (group == null) {
                            return;
                        }
                        var prefab = group.Tiles.FirstOrDefault(x => x.TileType == SelectedTile).Prefab;
                        selectedTilePrefab = Instantiate(prefab);
                        selectedTilePrefab.transform.parent = GameObject.FindGameObjectWithTag("MapRoot").transform;
                    }
                    var referencePlane = this.transform.gameObject.GetComponent<ReferencePlaneCollider>();
                    if(referencePlane != null && !debouncing) {
                        var go = Instantiate(selectedTilePrefab);
                        Vector3 point = hit.point;
                        go.transform.position = new Vector3(Mathf.Round(point.x * 20) / 20, Mathf.Round(point.y * 20) / 20, Mathf.Round(point.z * 20) / 20);
                    }
                }
            }
            
            if (Physics.Raycast(rayAnchor.position, rayAnchor.forward, out hit)) {
                var collider = hit.transform.gameObject.GetComponent<ReferencePlaneCollider>();
                if (collider != null) {
                    Vector3 point = hit.point;
                    selectedTilePrefab.transform.localEulerAngles = new Vector3(-90, 0, 0);
                    selectedTilePrefab.transform.position = new Vector3(Mathf.Round(point.x * 20) / 20, Mathf.Round(point.y * 20) / 20, Mathf.Round(point.z * 20) / 20);
                }
            }

            if (buttonTwo && !debouncing) {
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
        } else {
            movingMap = false;
            rotatingRootClockwise = false;
            rotatingRootCounterClockwise = false;
            rigthHandStartingPos = rayAnchor.position;
        }

        if (Time.realtimeSinceStartup - lastHit > debounce) {
            debouncing = false;
        }
        prevOne = buttonOne;
        prevTwo = buttonTwo;
    }
}
