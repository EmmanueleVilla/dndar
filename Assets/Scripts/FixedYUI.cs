using UnityEngine;

[ExecuteInEditMode]
public class FixedYUI : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        var me = transform;
        me.position = player.position + player.forward * 1.5f;
        me.localEulerAngles = player.localEulerAngles;
    }
}