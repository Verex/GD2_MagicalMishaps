using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public abstract class NetworkCharacter : NetworkBehaviour
{
	[SerializeField] private float updateDelay = 0.01f;
    [SerializeField] protected float moveSpeed = 1.0f;

    public bool isMoving;

	protected SpriteRenderer spriteRenderer;
	protected Animator animator;

    [ClientRpc]
    protected void RpcMoveStart(Vector2 direction, Vector3 startPosition, Vector3 endPosition)
    {
		// Update sprite flip.
		spriteRenderer.flipX = (direction.x != 0 && direction.x == -1 ? true : false);

		// Set our position to where we should be.
		transform.position = startPosition;

		StartCoroutine(Move(endPosition));
    }

    [ClientRpc]
    protected void RpcMoveFinish(Vector3 newPosition)
    {

    }

	[TargetRpc]
	protected void TargetUpdatePosition(NetworkConnection target, Vector3 position) 
	{
		transform.position = position;
	}

	[Client]
    protected IEnumerator Move(Vector3 endPosition)
    {
		// Set moving state.
        isMoving = true;

        Vector3 startPosition = transform.position;

		float t = 0.0f;

        while (t < 1.0f)
        {
            t += Time.deltaTime * moveSpeed;

            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            yield return null;
        }

		// Set moving state.
        isMoving = false;

        yield break;
    }
	
	[Command]
	protected void CmdGetPosition() {
		TargetUpdatePosition(connectionToClient, transform.position);
	}

	[Server]
	protected bool GetCanMove(Vector2 direction) 
	{
		// Cast ray for collision detection.
		RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1.0f);

		return hit.collider == null;
	}

	[Server]
	protected void StopMovement()
	{
		// Set animation parameters.
		animator.SetBool("isMoving", false);
	}

	[Server]
	protected bool MoveCharacter(Vector2 direction) 
	{
		// Check if we can move.
		if (GetCanMove(direction)) 
		{
			// Get our target position.
			Vector3 targetPosition = new Vector3(transform.position.x + direction.x,
												transform.position.y + direction.y,
												transform.position.z);

			// Make networked movement.
			StartCoroutine(NetworkMove(direction, targetPosition));

			return true;
		}
		else
		{
			// Set animation parameters.
			animator.SetBool("isMoving", false);

			return false;
		}
	}

	[Server]
    private IEnumerator NetworkUpdate()
    {
        while (true)
        {
			// Call character update.
			yield return UpdateCharacter();

			// Wait set time.
            yield return new WaitForSeconds(updateDelay);
        }
    }

	[Server]
	protected abstract IEnumerator UpdateCharacter();

	[Server] protected virtual void OnMoveStart() { }
	[Server] protected virtual void OnMoveFinish() { }

	[Server]
	protected IEnumerator NetworkMove(Vector2 direction, Vector3 targetPosition) 
	{
		// Set moving state.
		isMoving = true;

		// Set animation parameters.
		animator.SetBool("isMoving", true);

		if (direction.x != 0) 
		{
			animator.SetFloat("direction", 0.5f);
		} 
		else if (direction.y != 0) 
		{
			switch ((int)direction.y) 
			{
				case -1:
					animator.SetFloat("direction", 0.0f);
					break;
				case 1:
					animator.SetFloat("direction", 1.0f);
					break;
			}
		}

		// Callback for move start.
		OnMoveStart();

		// Start character movement on all clients.
		RpcMoveStart(direction, transform.position, targetPosition);

		// Wait for move.
		yield return new WaitForSeconds(1.0f / moveSpeed);

		// Update movement state.
		isMoving = false;

		// Update new position.
		transform.position = targetPosition;

		// Callback for move finish.
		OnMoveFinish();

		// End character movement for all clients.
		RpcMoveFinish(transform.position);

		yield break;
	}

    // Use this for initialization
    protected void Start()
    {
		// Get components.
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();

        if (isServer)
        {
            StartCoroutine(NetworkUpdate());
        }
    }

    protected void OnClientStart()
    {
        CmdGetPosition();
    }
}
