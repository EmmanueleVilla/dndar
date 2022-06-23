using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DndCore.Map;

public class SewerPipeQuantization : TileQuantization
{
    public override List<CellInfo> ToMap()
    {
        var cells = new List<CellInfo>();
        var rotation = transform.localEulerAngles.y;
        rotation = Mathf.Round(rotation / 90) * 90;
        if (rotation == 0)
        {
            cells.Add(new CellInfo('D', 0, null, 0, 0));
            cells.Add(new CellInfo('D', 0, null, 1, 0));
            cells.Add(new CellInfo(' ', 0, null, 0, 1));
            cells.Add(new CellInfo(' ', 0, null, 1, 1));
        }
        if (rotation == 90)
        {
            cells.Add(new CellInfo('D', 0, null, 0, 0));
            cells.Add(new CellInfo(' ', 0, null, 1, 0));
            cells.Add(new CellInfo('D', 0, null, 0, 1));
            cells.Add(new CellInfo(' ', 0, null, 1, 1));
        }
        if (rotation == 180)
        {
            cells.Add(new CellInfo(' ', 0, null, 0, 0));
            cells.Add(new CellInfo(' ', 0, null, 1, 0));
            cells.Add(new CellInfo('D', 0, null, 0, 1));
            cells.Add(new CellInfo('D', 0, null, 1, 1));
        }
        if (rotation == 270)
        {
            cells.Add(new CellInfo(' ', 0, null, 0, 0));
            cells.Add(new CellInfo('D', 0, null, 1, 0));
            cells.Add(new CellInfo(' ', 0, null, 0, 1));
            cells.Add(new CellInfo('D', 0, null, 1, 1));
        }
        return cells;
    }
}
