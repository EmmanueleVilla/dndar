using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DndCore.Map;

public class DifficultQuantization : TileQuantization
{
    public override List<CellInfo> ToMap()
    {
        var cells = new List<CellInfo>();
        cells.Add(new CellInfo('D', 0, null, 0, 0));
        cells.Add(new CellInfo('D', 0, null, 0, 1));
        cells.Add(new CellInfo('D', 0, null, 1, 0));
        cells.Add(new CellInfo('D', 0, null, 1, 1));
        return cells;
    }
}
