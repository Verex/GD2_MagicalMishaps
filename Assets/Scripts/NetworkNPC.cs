using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class NetworkNPC : NetworkCharacter
{
    protected Vector2 moveDirection;
    private GridSystem gridSystem;
    protected Queue<Vector2Int> movePath;

    protected bool hasGridSystem
    {
        get { return gridSystem != null; }
    }

    protected bool hasPath
    {
        get { return movePath != null; }
    }

    protected Vector2Int currentCell
    {
        get
        {
            if (!hasGridSystem) return Vector2Int.zero;

            Vector3Int currentPos = gridSystem.grid.WorldToCell(transform.position);

            return new Vector2Int(currentPos.x, currentPos.y);
        }
    }

    protected override IEnumerator ServerUpdate()
    {
        // Check if character is currently moving.
        if (!isMoving)
        {
            if (hasPath)
            {
                // Wait until done moving.
                yield return new WaitUntil(() => !isMoving);

                if (GetNextPathMove())
                {
                    MoveCharacter(moveDirection);
                }
                else
                {
                    Debug.Log("No more path.");

                    // Stop movement.
                    StopMovement();

                    // Wait.
                    yield return new WaitForSeconds(1.0f);

                    // Clear path.
                    movePath = null;
                }
            }
        }

        yield return null;
    }

    protected bool GetNextPathMove()
    {
        if (hasPath)
        {
            if (movePath.Count > 0)
            {
                // Update our move direction and remove point.
                moveDirection = (currentCell - movePath.Dequeue()) * new Vector2Int(-1, -1);

                // Update facing direction.
                facing = moveDirection;

                return true;
            }
        }

        return false;
    }

    protected void FindPath(Vector2Int destination, int searchDepth = 5000)
    {
        // Find path in tilemap.
        List<Vector2Int> path = gridSystem.FindPath(currentCell, destination, searchDepth);

        // Check if path found.
        if (path != null)
        {
			// Convert path to queue (and clear old).
            movePath = new Queue<Vector2Int>(path);

			// Remove current position from path.
			movePath.Dequeue();
        }
    }

    new protected void Start()
    {
        base.Start();

        // Set initial move direction.
        moveDirection = new Vector2(0, -1);

        // Get the grid system for the NPC (for pathfinding)
        gridSystem = (GridSystem)GameObject.FindObjectOfType(typeof(GridSystem));
    }
}
