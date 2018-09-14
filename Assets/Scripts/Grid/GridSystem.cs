using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class GridSystem : MonoBehaviour
{

    [SerializeField] private int defaultCost = 100;
    [SerializeField] private TileCost[] tileCosts;

	[SerializeField] private Vector2Int gridSize;

    private List<Step> StepList = new List<Step>(0);

    private List<Vector2Int> MovementOptions = new List<Vector2Int> { new Vector2Int(0, 1) };

    private Tilemap tilemap;
    private Grid grid;

    // Use this for initialization
    void Start()
    {
        // Get main components.
        tilemap = GetComponent<Tilemap>();
        grid = GetComponentInParent<Grid>();

        Vector3Int p = grid.WorldToCell(new Vector3(10, 10, 0));
        TileBase tb = tilemap.GetTile(p);

        foreach (TileCost tc in tileCosts)
        {
            Debug.Log(tc.name);
        }
    }

	private void MessageCurrent(Vector2Int position, IList<Vector2Int> path)
	{
		StepList.Add(new Step(StepType.Current, position, path));
	}

	private void MessageClose(Vector2Int position)
	{
		StepList.Add(new Step(StepType.Close, position, new List<Vector2Int>(0)));
	}

	private List<Vector2Int> PartiallyReconstructPath (Vector2Int start, Vector2Int end, Vector2Int[] cameFrom)
	{
		List<Vector2Int> path = new List<Vector2Int> { end };

		return path;
	}

    private List<Vector2Int> ReconstructPath(Vector2Int start, Vector2Int end, Vector2Int[] cameFrom)
    {
        List<Vector2Int> path = new List<Vector2Int> { end };
        Vector2Int current = end;
        do
        {
            Vector2Int prev = cameFrom[GetIndexUnchecked(current.x, current.y)];
            current = prev;
            path.Add(current);
        } while (current != start);

        return path;
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, int iterationLimit = 200)
    {
        // Clear step list.
        StepList.Clear();

        // Check for start/end.
        if (start == end)
        {
            return new List<Vector2Int> { start };
        }

        MinHeapNode head = new MinHeapNode(start, ManhattanDistance(start, end));
		MinHeap open = new MinHeap();

		// Push head node to heap.
		open.Push(head);

		// Define total cost.
		float[] totalCost = new float[gridSize.x * gridSize.y];
		Vector2Int[] cameFrom = new Vector2Int[gridSize.x * gridSize.y];

		while(open.HasNext() && iterationLimit > 0)
		{
			// Get current position.
			Vector2Int current = open.Pop().Position;
			MessageCurrent(current, PartiallyReconstructPath(start, end, cameFrom));

			if (current == end)
			{
                return ReconstructPath(start, end, cameFrom);
			}

            Step(open, cameFrom, totalCost, current, end);

            MessageClose(current);

            --iterationLimit;
		}

        return new List<Vector2Int>();
    }

    private void Step(MinHeap open, Vector2Int[] cameFrom, float[] totalCost, Vector2Int current, Vector2Int end)
    {
        float initialCost = totalCost[GetIndexUnchecked(current.x, current.y)];

        foreach (Vector2Int option in MovementOptions)
    }

    private List<Vector2Int> GetMovementOptions(Vector2Int position)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        foreach (Vector2Int move in MovementOptions)
        {
            Vector2Int target = position + move;

            if (target.x >= 0 && target.y < gridSize.x && target.y >= 0 && target.y < gridSize.y)
            {
                moves.Add(move);
            }
        }

        return moves;
    }

    private int GetIndexUnchecked(int x, int y)
    {
        return gridSize.x * y + x;
    }

    private static float ManhattanDistance(Vector2Int p0, Vector2Int p1)
    {
        var dx = Mathf.Abs(p0.x - p1.x);
        var dy = Mathf.Abs(p0.y - p1.y);
        return dx + dy;
    }
}

public class Step
{

    public Step(StepType type, Vector2Int position, IList<Vector2Int> path)
    {
        this.Type = type;
        this.Position = position;
        this.Path = path;
    }

    public StepType Type;
    public Vector2Int Position;
    public IList<Vector2Int> Path;
}

public enum StepType
{
    Current,
    Open,
    Close
}
