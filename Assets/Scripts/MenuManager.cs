using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InputManager;
using static TileManager;
using static UnityEngine.UI.Dropdown;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI Log;

    public GameObject MainMenu;
    public MapManager MapManager;
    public GameObject Settings;
    public GameObject Create;
    public GameObject ReferencePlane;

    public GameObject TilesRoot;

    public OVRPassthroughLayer OVRPassthroughLayer;
    public PrefsManager PrefsManager;
    public TMP_Dropdown TilesDropdown;

    public InputManager InputManager;
    public GameManager GameManager;

    [Serializable]
    public class TileInfo
    {
        public TileTypes TileType;
        public GameObject Prefab;
    }

    [Serializable]
    public class TileGroup
    {
        public string Label;
        public TileInfo[] Tiles;
    }

    public List<TileGroup> Tiles;

    public void Start()
    {
        GoToMainMenu();
        TilesDropdown.options = Tiles.Select(tile => new TMP_Dropdown.OptionData(tile.Label)).ToList();
        TilesDropdown.value = 0;
        TilesDropdown.Select();
        TilesDropdown.RefreshShownValue();
        OnTileSetSelected();
    }

    public void OnTileSetSelected()
    {
        var selectedTiles = Tiles[TilesDropdown.value];
        int startX = -65;
        int endX = 65;
        int startY = 6;
        int x = startX;
        int y = startY;
        foreach (Transform child in TilesRoot.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (var tile in selectedTiles.Tiles)
        {
            var go = Instantiate(tile.Prefab);
            var colliders = go.GetComponentsInChildren<TileCollider>();
            foreach (var collider in colliders)
            {
                collider.gameObject.AddComponent<SelectionTile>();
                collider.enabled = false;
            }
            go.transform.parent = TilesRoot.transform;
            go.transform.localPosition = new Vector3(x, y, -15);
            go.transform.localScale = new Vector3(250, 250, 250);
            go.transform.localEulerAngles = new Vector3(0, 0, 0);
            Utils.SetLayerRecursively(go, LayerMask.NameToLayer("Tiles"));
            x += 20;
            if (x > endX)
            {
                x = startX;
                y -= 20;
            }
        }
    }
    public void GoToSettings()
    {
        ResetMap();
        MainMenu.SetActive(false);
        Settings.SetActive(true);
        Create.SetActive(false);
        ReferencePlane.SetActive(false);
    }

    private void ResetMap()
    {
        MapManager.ResetRefPlane();
        var children = MapManager.GetComponentsInChildren<TileManager>();
        foreach (var tile in children)
        {
            if (tile.gameObject.tag != "RefPlane")
            {
                Destroy(tile.gameObject);
            }
        }
    }

    public void GoToPlay()
    {
        ResetMap();
        InputManager.GameState = GameStates.Play;
        MainMenu.SetActive(false);
        Settings.SetActive(false);
        Create.SetActive(true);
        ReferencePlane.SetActive(true);
        InputManager.Load();
        GameManager.StartGame(InputManager.GetMap());
    }

    public void GoToMainMenu()
    {
        ResetMap();
        InputManager.GameState = GameStates.None;
        MainMenu.SetActive(true);
        Settings.SetActive(false);
        Create.SetActive(false);
        ReferencePlane.SetActive(false);
    }

    public void GoToCreate()
    {
        ResetMap();
        InputManager.GameState = GameStates.Create;
        MainMenu.SetActive(false);
        Settings.SetActive(false);
        Create.SetActive(true);
        ReferencePlane.SetActive(true);
        InputManager.Load();
    }

    public void SetOpacity(float value)
    {
        OVRPassthroughLayer.textureOpacity = value;
        PrefsManager.SetOpacity(value);
    }

    public void SetEdgeRendering(bool value)
    {
        OVRPassthroughLayer.edgeRenderingEnabled = value;
        PrefsManager.SetEdgesRendering(value);
    }

}
