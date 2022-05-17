using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject Settings;
    public OVRPassthroughLayer OVRPassthroughLayer;

    public void Start() {
        GoToMainMenu();
    }

    public void GoToSettings() {
        MainMenu.SetActive(false);
        Settings.SetActive(true);
    }

    public void GoToMainMenu() {
        MainMenu.SetActive(true);
        Settings.SetActive(false);
    }

    public void SetOpacity(float value) {
        OVRPassthroughLayer.textureOpacity = value;
    }

}
