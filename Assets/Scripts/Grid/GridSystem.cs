using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class GridSystem : MonoBehaviour {

	[SerializeField] private int defaultCost = 100;
	[SerializeField] private TileCost[] tileCosts;

	private Tilemap tilemap;
	private Grid grid;

	// Use this for initialization
	void Start () {
		// Get main components.
		tilemap = GetComponent<Tilemap>();
		grid = GetComponentInParent<Grid>();

		Vector3Int p = grid.WorldToCell(new Vector3(10, 10, 0));
		TileBase tb = tilemap.GetTile(p);

		foreach(TileCost tc in tileCosts)
		{
			Debug.Log(tc.name);
		}
	}

	public static List<Vector3Int> FindPath(Vector3Int start, Vector3Int end)
	{
		// Clear our step list.

		// Check for start/end.
		if (start == end)
		{
			return new List<Vector3Int> { start };
		}


		return new List<Vector3Int>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

public class Step {

	public StepType Type;
	public Vector3Int Position;
	public IList<Vector3Int> Path;
}

public enum StepType {
	Current,
	Open,
	Close
}
