using UnityEngine;

public class MinHeapNode {
	public MinHeapNode(Vector2Int position, float expectedCost)
	{
		this.Position = position;
		this.ExpectedCost = expectedCost;            
	}

	public Vector2Int Position;
	public float ExpectedCost;       
	public MinHeapNode Next;
}
