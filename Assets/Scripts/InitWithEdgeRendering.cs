using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitWithEdgeRendering : MonoBehaviour {
    public Toggle Toggle;
    public PrefsManager PrefsManager;
    public OVRPassthroughLayer OVRPassthroughLayer;
    void Start() {
        Toggle.isOn = PrefsManager.GetEdgesRendering();
        OVRPassthroughLayer.edgeRenderingEnabled = Toggle.isOn;
    }
}
