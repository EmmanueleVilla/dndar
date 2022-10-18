using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Creatures;
using TMPro;
using UnityEngine;
using static InputManager;
using static TileManager;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI Log;
    public UIManager UIManager;

    public GameObject MainMenu;
    public MapManager MapManager;
    public GameObject Settings;
    public GameObject Create;
    public GameObject Play;
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
            Destroy(child.gameObject);
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
        Play.SetActive(false);
        MainMenu.SetActive(false);
        Settings.SetActive(true);
        Create.SetActive(false);
        ReferencePlane.SetActive(false);
    }

    private void ResetMap()
    {
        MapManager.ResetRefPlane();
        UIManager.ResetUI();
        var children = MapManager.GetComponentsInChildren<TileManager>();
        foreach (var tile in children)
        {
            if (tile.gameObject.tag != "RefPlane")
            {
                Destroy(tile.gameObject);
            }
        }
    }

    private void InitPlay()
    {
        ResetMap();
        Play.SetActive(true);
        InputManager.GameState = GameStates.Play;
        MainMenu.SetActive(false);
        Settings.SetActive(false);
        Create.SetActive(false);
        ReferencePlane.SetActive(true);
        InputManager.Load();
    }

    public void GoToPlayAuto()
    {
        InitPlay();
        this.StartCoroutine(GameManager.StartGame(InputManager.GetMap(), true));
    }

    public void GoToPlayManual()
    {
        InitPlay();
        this.StartCoroutine(GameManager.StartGame(InputManager.GetMap(), false));
    }

    public void GoToMainMenu()
    {
        ResetMap();
        Play.SetActive(false);
        InputManager.GameState = GameStates.None;
        MainMenu.SetActive(true);
        Settings.SetActive(false);
        Create.SetActive(false);
        ReferencePlane.SetActive(false);
    }

    public void GoToCreate()
    {
        ResetMap();
        Play.SetActive(false);
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

    public void ShowWinner(Loyalties valueLoyalty)
    {
        Debug.Log("Winner is: " + valueLoyalty);
    }
}