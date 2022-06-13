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
    private float debounce = 0.5f;

    public MenuManager MenuManager;
    public MapManager MapManager;

    public TextMeshProUGUI Log;

    GameObject selectedTilePrefab;

    private InputDevice GetInputDevice() {
        InputDeviceCharacteristics characteristics = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(characteristics, devices);
        return devices.FirstOrDefault();
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



        inputDevice.TryGetFeatureValue(OculusUsages.indexTouch, out bool indexTouching);
        inputDevice.TryGetFeatureValue(OculusUsages.thumbTouch, out bool thumbTouching);
        inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripTouching);

        if (Time.realtimeSinceStartup - lastHit > debounce) {
            debouncing = false;
        }


        if (!indexTouching && !thumbTouching && !gripTouching) {
            Destroy(selectedTilePrefab);
            SelectedTile = TileTypes.None;
        }

        if(indexTrigger > 0.9f && handTrigger > 0.9f && gripTouching)
        {
            Save();
            Log.text = "SAVED";
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
                selectedTilePrefab.transform.parent = hit.transform.parent;
                var snap = selectedTilePrefab.GetComponent<TileManager>().Snap;
                selectedTilePrefab.transform.localPosition = new Vector3(Mathf.Round(selectedTilePrefab.transform.localPosition.x * snap) / snap, hit.transform.localPosition.y + 0.01f, Mathf.Round(selectedTilePrefab.transform.localPosition.z * snap) / snap);
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

        //TODO: edit already placed tile
    }


    void Save()
    {
        var list = MapManager.GetComponentsInChildren<TileManager>().Select(x => new SavedTile() { Type = x.TileType, LocalPosition = x.transform.localPosition, LocalEulerAngles = x.transform.localEulerAngles });
        PlayerPrefs.SetString("saved_map", JsonUtility.ToJson(list));
    }

    public class SavedTile
    {
        public TileTypes Type;
        public Vector3 LocalPosition;
        public Vector3 LocalEulerAngles;
    }
}
