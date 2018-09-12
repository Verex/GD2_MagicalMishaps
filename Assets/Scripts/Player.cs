using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings(sendInterval = 0.1f)]
public class Player : NetworkCharacter {

	[SerializeField] private float inputDelay = 0.01f;
	[SerializeField] private float inputThreshold = 0.2f;

	private Vector2 moveDirection;
	private Vector2 lastDirection;
	private bool shouldMove = false;
	private bool lastMove;

	[Command]
	private void CmdMoveState(bool moving, Vector2 direction)
	{
		shouldMove = moving;
		
		moveDirection = direction;
	}

	[Client]
	private IEnumerator UpdateInput()
	{
		while (true) 
		{
			float x = Input.GetAxis("Horizontal"),
					y = Input.GetAxis("Vertical");

			Vector2 direction = Vector2.zero;

			bool move = false;

			// Check if we are trying to move.
			if (Mathf.Abs(x) > inputThreshold)
			{
				direction = new Vector2((Mathf.Abs(x) / x) * Mathf.Ceil(Mathf.Abs(x)), 0);
				move = true;
			}
			else if (Mathf.Abs(y) > inputThreshold) 
			{
				// Move vertically.
				direction = new Vector2(0, (Mathf.Abs(y) / y) * Mathf.Ceil(Mathf.Abs(y)));
				move = true;
			}

			if (direction != lastDirection || move != lastMove)
			{
				CmdMoveState(move, direction);
				lastDirection = direction;
				lastMove = move;
			}

			yield return new WaitForSeconds(inputDelay);
		}
	}
	
	// Server side update.
	protected override IEnumerator UpdateCharacter()
	{
		if (!isMoving && shouldMove)
		{
			if (MoveCharacter(moveDirection))
			{
				if (!shouldMove)
				{
					StopMovement();
				}
			}
			else
			{
				StopMovement();
			}
		}

		yield return null;
	}

	protected override void OnMoveFinish()
	{
		if (!shouldMove)
		{
			StopMovement();
		}
	}

	new private void Start()
	{
		base.Start();

		if (isServer)
		{
			moveDirection = Vector2.zero;
		}


		// Start input coroutine if local player.
		if (isLocalPlayer) 
		{
			StartCoroutine(UpdateInput());
		}
	}

}
