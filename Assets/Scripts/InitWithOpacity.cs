using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitWithOpacity : MonoBehaviour
{
    public Slider Slider;
    public OVRPassthroughLayer OVRPassthroughLayer;
    void Start()
    {
        Slider.value = OVRPassthroughLayer.textureOpacity;
    }
}
