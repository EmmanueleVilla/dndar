using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DndCore.Map;

public class StairsHighQuantization : TileQuantization
{
    public override List<CellInfo> ToMap()
    {
        var cells = new List<CellInfo>();
        var rotation = transform.localEulerAngles.y;
        rotation = Mathf.Round(rotation / 90) * 90;
        if (rotation == 0)
        {
            cells.Add(new CellInfo('G', 3, null, 0, 0));
            cells.Add(new CellInfo('G', 3, null, 1, 0));
            cells.Add(new CellInfo('G', 4, null, 0, 1));
            cells.Add(new CellInfo('G', 4, null, 1, 1));
        }
        if (rotation == 90)
        {
            cells.Add(new CellInfo('G', 4, null, 1, 0));
            cells.Add(new CellInfo('G', 4, null, 1, 1));
            cells.Add(new CellInfo('G', 3, null, 0, 1));
            cells.Add(new CellInfo('G', 3, null, 0, 0));
        }
        if (rotation == 180)
        {
            cells.Add(new CellInfo('G', 4, null, 0, 0));
            cells.Add(new CellInfo('G', 4, null, 1, 0));
            cells.Add(new CellInfo('G', 3, null, 0, 1));
            cells.Add(new CellInfo('G', 3, null, 1, 1));
        }
        if (rotation == 270)
        {
            cells.Add(new CellInfo('G', 3, null, 1, 0));
            cells.Add(new CellInfo('G', 3, null, 1, 1));
            cells.Add(new CellInfo('G', 4, null, 0, 1));
            cells.Add(new CellInfo('G', 4, null, 0, 0));
        }
        return cells;
    }
}
