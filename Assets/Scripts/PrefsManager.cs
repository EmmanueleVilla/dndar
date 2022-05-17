using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefsManager : MonoBehaviour
{
    private string OPACITY_KEY = "OVRPassthroughLayer.textureOpacity";
    private string EDGES_RENDERING_KEY = "OVRPassthroughLayer.edgeRenderingEnabled";

    public OVRPassthroughLayer OVRPassthroughLayer;

    public float GetOpacity() {
        return PlayerPrefs.GetFloat(OPACITY_KEY, 1.0f);
    }

    public void SetOpacity(float value) {
        PlayerPrefs.SetFloat(OPACITY_KEY, value);
    }

    public bool GetEdgesRendering() {
        return PlayerPrefs.GetInt(EDGES_RENDERING_KEY, 1) == 1;
    }

    public void SetEdgesRendering(bool value) {
        PlayerPrefs.SetInt(EDGES_RENDERING_KEY, value ? 1 : 0);
    }
}
