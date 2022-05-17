using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitWithOpacity : MonoBehaviour
{
    public Slider Slider;
    public PrefsManager PrefsManager;
    public OVRPassthroughLayer OVRPassthroughLayer;
    void Start()
    {
        Slider.value = PrefsManager.GetOpacity();
        OVRPassthroughLayer.textureOpacity = Slider.value;
    }
}
