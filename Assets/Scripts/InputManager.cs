using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.XR.Oculus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using static TileManager;

public class InputManager : MonoBehaviour
{
    public Transform rayAnchor;

    public TileTypes SelectedTile;

    private bool debouncing = false;
    private float lastHit = 0;
    private float debounce = 0.25f;

    public MenuManager MenuManager;
    public MapManager MapManager;

    public TextMeshProUGUI Log;

    GameObject selectedTilePrefab;

    private InputDevice GetInputDevice() {
        InputDeviceCharacteristics characteristics = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(characteristics, devices);
        return devices.First();
    }

    private void StartDebounce()
    {
        debouncing = true;
        lastHit = Time.realtimeSinceStartup;
    }

    void Update()
    {
        var inputDevice = GetInputDevice();
        var buttonOne = OVRInput.Get(OVRInput.Button.One);
        var buttonTwo = OVRInput.Get(OVRInput.Button.Two);
        var indexTrigger = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch);

        if(indexTrigger < 0.1f)
        {
            MapManager.ExitMovementMode();
        }

        var handTrigger = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger, OVRInput.Controller.Touch);

        if(handTrigger < 0.1f)
        {
            MapManager.ExitReferenceMode();
        }

        bool indexTouching;
        bool thumbTouching;

        inputDevice.TryGetFeatureValue(OculusUsages.indexTouch, out indexTouching);
        inputDevice.TryGetFeatureValue(OculusUsages.thumbTouch, out thumbTouching);

        if (Time.realtimeSinceStartup - lastHit > debounce) {
            debouncing = false;
        }

        if(!indexTouching && !thumbTouching) {
            Destroy(selectedTilePrefab);
            SelectedTile = TileTypes.None;
        }

        if(debouncing) {
            return;
        }

        RaycastHit hit;
        var collides = Physics.Raycast(rayAnchor.position, rayAnchor.forward, out hit);
        if(!collides) {
            return;
        }
        var collidingPoint = hit.point;
        var collidingObject = hit.transform.gameObject;
        var selectionTile = collidingObject.GetComponent<SelectionTile>();
        if(selectionTile != null) {
            if (buttonOne) {
                SelectedTile = selectionTile.GetComponent<TileCollider>().TileManager.GetComponent<TileManager>().TileType;
                Destroy(selectedTilePrefab);
                var group = MenuManager.Tiles.FirstOrDefault(x => x.Tiles.Select(tile => tile.TileType).Contains(SelectedTile));
                if (group == null) {
                    return;
                }
                var prefab = group.Tiles.FirstOrDefault(x => x.TileType == SelectedTile).Prefab;
                selectedTilePrefab = Instantiate(prefab);
                selectedTilePrefab.transform.parent = MapManager.transform;
                selectedTilePrefab.transform.localEulerAngles = new Vector3(0, 0, 0);
                selectedTilePrefab.transform.localScale = Vector3.zero;
                var colliders = selectedTilePrefab.GetComponentsInChildren<BoxCollider>();
                foreach(var collider in colliders)
                {
                    collider.enabled = false;
                }
                
                StartDebounce();
                Log.text = SelectedTile.ToString(); 
                return;
            }
            if(buttonTwo && !debouncing) {
                selectionTile.GetComponent<TileCollider>().TileManager.transform.Rotate(Vector3.up * 90);
                StartDebounce();
                return;
            }
        }
        var referencePlane = collidingObject.GetComponent<ReferencePlaneCollider>();

        if(referencePlane != null)
        {
            if(SelectedTile == TileTypes.None)
            {
                if(indexTrigger > 0.1f)
                {
                    MapManager.EnterMovementMode();
                } else if(handTrigger > 0.1f)
                {
                    MapManager.EnterReferenceMode();
                }
            } else if(selectedTilePrefab != null) {
                Vector3 point = hit.point;
                selectedTilePrefab.transform.localScale = Vector3.one;
                selectedTilePrefab.transform.position = point;
                selectedTilePrefab.transform.localPosition = new Vector3(Mathf.Round(selectedTilePrefab.transform.localPosition.x * 20) / 20, selectedTilePrefab.transform.localPosition.y, Mathf.Round(selectedTilePrefab.transform.localPosition.z * 20) / 20);
                if(buttonTwo && !debouncing)
                {
                    selectedTilePrefab.transform.Rotate(Vector3.up * 90);
                    StartDebounce();
                    return;
                }
                if(buttonOne && !debouncing)
                {
                    var go = Instantiate(selectedTilePrefab);
                    go.transform.parent = MapManager.transform;
                    go.transform.position = selectedTilePrefab.transform.position;
                    go.transform.eulerAngles = selectedTilePrefab.transform.eulerAngles;
                }
            }
        }
        var tileInMap = collidingObject.GetComponent<TileCollider>();

        /*
        if (GameState == GameStates.InsertTile) {
            if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch) < 0.1f) {
                Destroy(selectedTilePrefab);
                GameState = GameStates.NewMap;
                MenuManager.EditTileMode();
            }
            
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

        
        prevOne = buttonOne;
        prevTwo = buttonTwo;
        */
    }
}
