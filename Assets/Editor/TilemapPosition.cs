using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(GridSystem))]
class LabelHandle : Editor
{
    void OnSceneGUI()
    {
        GridSystem gs = (GridSystem)target;

        if (gs == null) return;

        Vector3 mousePosition = Event.current.mousePosition;
        mousePosition.y = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePosition.y;
        mousePosition = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(mousePosition);
        //mousePosition.y = -mousePosition.y;

        Grid grid = gs.GetComponentInParent<Grid>();
        Tilemap tm = gs.GetComponent<Tilemap>();
        Vector3Int cellPos = grid.WorldToCell(mousePosition);

        string cellTileName = "Empty";

        TileBase tb = tm.GetTile(cellPos);

        if (tb != null)
        {
            cellTileName = tb.name;
        }

        Handles.BeginGUI();

        Handles.color = Color.blue;

        GUI.Label(new Rect(10, 10, 200, 200),
                    "X: " + cellPos.x + " Y: " + cellPos.y + "\n"
                    + "Tile: " + cellTileName);

        Handles.EndGUI();
    }
}