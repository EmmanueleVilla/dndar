using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TileManager;
using static UnityEngine.UI.Dropdown;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI Log;

    public GameObject MainMenu;
    public GameObject Settings;
    public GameObject Create;
    public GameObject ReferencePlane;

    public GameObject TilesRoot;

    public OVRPassthroughLayer OVRPassthroughLayer;
    public PrefsManager PrefsManager;
    public TMP_Dropdown TilesDropdown;


    [Serializable]
    public class TileInfo {
        public TileTypes TileType;
        public GameObject Prefab;
    }

    [Serializable]
    public class TileGroup {
        public string Label;
        public TileInfo[] Tiles;
    }

    public List<TileGroup> Tiles;

    public void Start() {
        GoToMainMenu();
        TilesDropdown.options = Tiles.Select(tile => new TMP_Dropdown.OptionData(tile.Label)).ToList();
        TilesDropdown.value = 0;
        TilesDropdown.Select();
        TilesDropdown.RefreshShownValue();
        OnTileSetSelected();
    }

    public void OnTileSetSelected() {
        var selectedTiles = Tiles[TilesDropdown.value];
        int startX = -65;
        int endX = 65;
        int startY = 6;
        int x = startX;
        int y = startY;
        foreach (Transform child in TilesRoot.transform) {
            GameObject.Destroy(child.gameObject);
        }
        foreach (var tile in selectedTiles.Tiles) {
            var go = Instantiate(tile.Prefab);
            go.transform.parent = TilesRoot.transform;
            go.transform.localPosition = new Vector3(x, y, -15);
            go.transform.localScale = new Vector3(250, 250, 250);
            x += 20;
            if(x > endX) {
                x = startX;
                y -= 20;
            }
        }
    }

    public void GoToSettings() {
        MainMenu.SetActive(false);
        Settings.SetActive(true);
        Create.SetActive(false);
        ReferencePlane.SetActive(false);
    }

    public void GoToMainMenu() {
        MainMenu.SetActive(true);
        Settings.SetActive(false);
        Create.SetActive(false);
        ReferencePlane.SetActive(false);
    }

    public void GoToCreate() {
        MainMenu.SetActive(false);
        Settings.SetActive(false);
        Create.SetActive(true);
        ReferencePlane.SetActive(true);
    }

    public void SetOpacity(float value) {
        OVRPassthroughLayer.textureOpacity = value;
        PrefsManager.SetOpacity(value);
    }

    public void SetEdgeRendering(bool value) {
        OVRPassthroughLayer.edgeRenderingEnabled = value;
        PrefsManager.SetEdgesRendering(value);
    }

}
