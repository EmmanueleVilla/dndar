using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DndCore.Map;

public abstract class ObjectQuantization : MonoBehaviour
{
    public abstract List<CellInfo> ToMap();
}
