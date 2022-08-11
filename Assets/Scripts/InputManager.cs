using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.XR.Oculus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using static TileManager;
using System;
using System.Text;
using Logic.Core.Map.Impl;
using DndCore.Map;
using Logic.Core.Creatures;
using Logic.Core.Creatures.Bestiary;
using Newtonsoft.Json;

public class InputManager : MonoBehaviour
{

    public enum GameStates
    {
        None,
        Create,
        Play
    }

    public GameStates GameState;
    public GameManager GameManager;

    public Transform rayAnchor;

    public TileTypes SelectedTile;

    private bool debouncing = false;
    private float lastHit = 0;
    private float debounce = 0.5f;

    public MenuManager MenuManager;
    public MapManager MapManager;
    public UIManager UIManager;

    public TextMeshProUGUI Log;

    GameObject selectedTilePrefab;

    private InputDevice GetInputDevice()
    {
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

    void Start()
    {
        DndCore.DI.DndModule.RegisterRules(false, null, null, false);
    }

    public void Load()
    {
        var cache = PlayerPrefs.GetString("saved_map", "");
        var objects = cache.Split("\n");
        foreach (var tile in objects)
        {
            try
            {
                var values = tile.Split("#");
                var type = (TileTypes)(Enum.Parse(typeof(TileTypes), values[0]));
                var pos = new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                var rot = new Vector3(float.Parse(values[4]), float.Parse(values[5]), float.Parse(values[6]));
                var group = MenuManager.Tiles.FirstOrDefault(x => x.Tiles.Select(tile => tile.TileType).Contains(type));
                if (group == null)
                {
                    continue;
                }
                var prefab = group.Tiles.FirstOrDefault(x => x.TileType == type).Prefab;
                var go = Instantiate(prefab);
                go.transform.parent = MapManager.transform;
                go.transform.localPosition = pos;
                go.transform.localEulerAngles = rot;
                Utils.SetLayerRecursively(go, LayerMask.NameToLayer("Map"));
            }
            catch (Exception e)
            {
            }
        }
    }

    void Update()
    {
        if (GameState == GameStates.None)
        {
            return;
        }

        var inputDevice = GetInputDevice();
        var buttonOne = OVRInput.Get(OVRInput.Button.One);
        var buttonTwo = OVRInput.Get(OVRInput.Button.Two);
        var indexTrigger = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch);

        if (indexTrigger < 0.1f)
        {
            MapManager.ExitMovementMode();
        }

        var handTrigger = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger, OVRInput.Controller.Touch);

        if (handTrigger < 0.1f)
        {
            MapManager.ExitReferenceMode();
        }

        inputDevice.TryGetFeatureValue(OculusUsages.indexTouch, out bool indexTouching);
        inputDevice.TryGetFeatureValue(OculusUsages.thumbTouch, out bool thumbTouching);
        inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripTouching);

        if (Time.realtimeSinceStartup - lastHit > debounce)
        {
            debouncing = false;
        }

        if (debouncing)
        {
            return;
        }

        RaycastHit hit;
        var mask = LayerMask.GetMask("Default", "Map", "Tiles", "Plane");
        if (SelectedTile != TileTypes.None)
        {
            mask = LayerMask.GetMask("Default", "Tiles", "Plane");
        }

        if (GameState == GameStates.Play)
        {
            mask = LayerMask.GetMask("Default", "Plane", "InputTile");
        }

        var collides = Physics.Raycast(rayAnchor.position, rayAnchor.forward, out hit, 50.0f, mask);
        if (!collides)
        {
            return;
        }

        var collidingPoint = hit.point;
        var collidingObject = hit.transform.gameObject;

        var referencePlane = collidingObject.GetComponent<ReferencePlaneCollider>();
        var spriteManager = collidingObject.GetComponent<SpriteManager>();

        if (GameState == GameStates.Create)
        {

            if (!indexTouching && !thumbTouching && !gripTouching)
            {
                Destroy(selectedTilePrefab);
                SelectedTile = TileTypes.None;
            }

            if (SelectedTile == TileTypes.None && indexTrigger > 0.9f && handTrigger > 0.9f && gripTouching)
            {
                Save();
            }

            var selectionTile = collidingObject.GetComponent<SelectionTile>();
            if (selectionTile != null)
            {
                if (buttonOne)
                {
                    StartDebounce();
                    SelectedTile = selectionTile.GetComponent<TileCollider>().TileManager.GetComponent<TileManager>().TileType;
                    Destroy(selectedTilePrefab);
                    var group = MenuManager.Tiles.FirstOrDefault(x => x.Tiles.Select(tile => tile.TileType).Contains(SelectedTile));
                    if (group == null)
                    {
                        return;
                    }
                    var prefab = group.Tiles.FirstOrDefault(x => x.TileType == SelectedTile).Prefab;
                    selectedTilePrefab = Instantiate(prefab);
                    selectedTilePrefab.transform.parent = MapManager.transform;
                    selectedTilePrefab.transform.localEulerAngles = new Vector3(0, 0, 0);
                    selectedTilePrefab.transform.localScale = Vector3.zero;
                    var colliders = selectedTilePrefab.GetComponentsInChildren<BoxCollider>();
                    foreach (var collider in colliders)
                    {
                        collider.enabled = false;
                    }

                    return;
                }
                if (buttonTwo && !debouncing)
                {
                    StartDebounce();
                    selectionTile.GetComponent<TileCollider>().TileManager.transform.Rotate(Vector3.up * 90);
                    return;
                }
            }

            if (referencePlane != null)
            {
                if (SelectedTile == TileTypes.None)
                {
                    if (indexTrigger > 0.1f)
                    {
                        MapManager.EnterMovementMode();
                    }
                    else if (handTrigger > 0.1f)
                    {
                        MapManager.EnterReferenceMode();
                    }
                }
                else if (selectedTilePrefab != null)
                {
                    Vector3 point = hit.point;
                    selectedTilePrefab.transform.localScale = Vector3.one;
                    selectedTilePrefab.transform.position = point;
                    selectedTilePrefab.transform.parent = hit.transform.parent;
                    var snap = selectedTilePrefab.GetComponent<TileManager>().Snap;
                    var delta = 0.0f;
                    if (SelectedTile.ToString().StartsWith("Character") || SelectedTile.ToString().StartsWith("Monster"))
                    {
                        delta = 0.0125f;
                    }
                    selectedTilePrefab.transform.localPosition = new Vector3(
                        (Mathf.Round(selectedTilePrefab.transform.localPosition.x * snap) / snap) + delta,
                        hit.transform.localPosition.y + 0.01f,
                        (Mathf.Round(selectedTilePrefab.transform.localPosition.z * snap) / snap) - delta
                        );
                    if (buttonTwo && !debouncing)
                    {
                        StartDebounce();
                        selectedTilePrefab.transform.Rotate(Vector3.up * 90);
                        return;
                    }
                    if (buttonOne && !debouncing)
                    {
                        StartDebounce();
                        var group = MenuManager.Tiles.FirstOrDefault(x => x.Tiles.Select(tile => tile.TileType).Contains(SelectedTile));
                        if (group == null)
                        {
                            return;
                        }
                        var prefab = group.Tiles.FirstOrDefault(x => x.TileType == SelectedTile).Prefab;
                        var go = Instantiate(prefab);
                        go.transform.parent = MapManager.transform;
                        go.transform.position = selectedTilePrefab.transform.position;
                        go.transform.eulerAngles = selectedTilePrefab.transform.eulerAngles;
                        Utils.SetLayerRecursively(go, LayerMask.NameToLayer("Map"));
                    }
                }
            }
            var tileInMap = collidingObject.GetComponent<TileCollider>();
            if (tileInMap != null)
            {
                if (SelectedTile == TileTypes.None)
                {
                    if (buttonOne && !debouncing)
                    {
                        StartDebounce();
                        Destroy(tileInMap.GetComponent<TileCollider>().TileManager.gameObject);
                    }
                    if (buttonTwo && !debouncing)
                    {
                        StartDebounce();
                        tileInMap.GetComponent<TileCollider>().TileManager.gameObject.transform.Rotate(Vector3.up * 90);
                    }
                }
            }
        }
        if (GameState == GameStates.Play)
        {
            if (spriteManager != null)
            {
                if (buttonOne)
                {
                    GameManager.OnCellClicked(spriteManager.X, spriteManager.Y);
                }
            }
            else if (referencePlane != null)
            {
                if (indexTrigger > 0.1f)
                {
                    MapManager.EnterMovementMode();
                }
            }
        }
    }

    public IMap GetMap()
    {
        var map = Save();
        return map;
    }

    IMap Save()
    {
        var children = MapManager.GetComponentsInChildren<TileManager>();
        var saveFile = new StringBuilder();
        ArrayDndMap map = new ArrayDndMap(42, 42, new CellInfo(' ', 0));
        var minY = 0f;

        foreach (var tile in children)
        {
            saveFile.Append(
            tile.TileType
            + "#" +
            tile.transform.localPosition.x
            + "#" +
            tile.transform.localPosition.y
            + "#" +
            tile.transform.localPosition.z
            + "#" +
            tile.transform.localEulerAngles.x
            + "#" +
            tile.transform.localEulerAngles.y
            + "#" +
            tile.transform.localEulerAngles.z
            + "\n");
            minY = MathF.Min(minY, tile.transform.localPosition.y);
        }

        foreach (var tile in children)
        {
            var quantization = tile.GetComponent<TileQuantization>();
            if (quantization != null)
            {
                var cells = quantization.ToMap();
                int shiftedX = -1 * ((int)(tile.transform.localPosition.x * 100) - 50);
                int shiftedZ = -1 * ((int)(tile.transform.localPosition.z * 100) - 50);
                int shiftedY = (int)(Math.Round((tile.transform.localPosition.y - minY) * 100 / 5.0) * 5);
                int x = (int)(shiftedX / 2.5);
                int z = (int)(shiftedZ / 2.5);
                byte y = (byte)(shiftedY);
                for (int i = 0; i < cells.Count; i++)
                {
                    int xx = cells[i].X + x;
                    int zz = cells[i].Y + z;
                    byte yy = (byte)((int)cells[i].Height + (int)y);
                    map.SetCell(xx, zz, new CellInfo(
                        cells[i].Terrain,
                        yy,
                        null,
                        xx,
                        zz
                    ));
                }
            }
        }

        foreach (var tile in children)
        {
            var quantization = tile.GetComponent<ObjectQuantization>();
            if (quantization != null)
            {
                var cells = quantization.ToMap();
                int shiftedX = -1 * ((int)(tile.transform.localPosition.x * 100) - 50);
                int shiftedZ = -1 * ((int)(tile.transform.localPosition.z * 100) - 50);
                int shiftedY = (int)(Math.Round((tile.transform.localPosition.y - minY) * 100 / 5.0) * 5);
                int x = (int)(shiftedX / 2.5);
                int z = (int)(shiftedZ / 2.5);
                byte y = (byte)(shiftedY);
                for (int i = 0; i < cells.Count; i++)
                {
                    int xx = cells[i].X + x;
                    int zz = cells[i].Y + z;
                    byte yy = (byte)((int)cells[i].Height + (int)y);
                    map.SetCell(xx, zz, new CellInfo(
                        cells[i].Terrain,
                        yy,
                        null,
                        xx,
                        zz
                    ));
                }
            }
        }

        Log.text = "";

        foreach (var tile in children)
        {
            var quantization = tile.GetComponent<CharacterQuantization>();
            if (quantization != null)
            {
                var info = quantization.ToMap();
                int shiftedX = -1 * ((int)(tile.transform.localPosition.x * 100) - 50);
                int shiftedZ = -1 * ((int)(tile.transform.localPosition.z * 100) - 50);
                int shiftedY = (int)(Math.Round((tile.transform.localPosition.y - minY) * 100 / 5.0) * 5);
                int x = (int)(shiftedX / 2.5) + 1;
                int z = (int)(shiftedZ / 2.5) + 1;
                if (!map.AddCreature(info.Creature, x, z))
                {
                    //Log.text += "\nFailed to add creature";
                }
            }
        }
        //Log.text += "\n<mspace=0.75em>Begin map\n";

        for (int j = 41; j >= 0; j--)
        {
            //Log.text += j.ToString("D2");

            for (int i = 0; i < 42; i++)
            {
                String c = map.GetCellInfo(i, j).Terrain + "";
                if (c == " ")
                {
                    c = "---";
                }
                else
                {
                    c += map.GetCellInfo(i, j).Height.ToString("D2");
                }
                ICreature creature = map.GetCellInfo(i, j).Creature;
                if (creature != null)
                {
                    c = "$" + map.GetCellInfo(i, j).Height.ToString("D2");
                }
                //Log.text += c;
            }
            //Log.text += "\n";
        }
        PlayerPrefs.SetString("saved_map", saveFile.ToString());
        return map;
    }
}
