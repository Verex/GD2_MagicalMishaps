using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Skeleton : NetworkCharacter
{
	private Vector2 moveDirection;

	protected override IEnumerator UpdateCharacter()
	{
		// Check if character is currently moving.
		if (!isMoving)
		{
			// Attempt to move character in direction.
			if (MoveCharacter(moveDirection)) 
			{
				int c = Random.Range(0, 10);

				if (c == 0) 
				{
					moveDirection = RandomDirection();
				}
				else if (c == 1)
				{
					// Wait until done moving.
					yield return new WaitUntil(() => !isMoving);

					// Stop movement.
					StopMovement();

					yield return new WaitForSeconds(1.0f);
				}
			}
			else 
			{
				// Choose new direction.
				moveDirection = RandomDirection();

				// Wait for a little.
				yield return new WaitForSeconds(1.0f);
			}
		}

		yield return null;
	}

	private Vector2 RandomDirection()
	{
		// Choose random axis.
		int axis = Random.Range(0, 2);

		// Define direction.
		Vector2 direction = Vector2.zero;

		// Choose direction along axis.
		direction[axis] = (Random.Range(0, 2) == 1 ? -1 : 1);

		return direction;
	}

	new private void Start()
	{
		base.Start();

		// Choose initial movement direction.
		moveDirection = RandomDirection();
	}
}
