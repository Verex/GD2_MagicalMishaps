using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[NetworkSettings(sendInterval = 0.01f)]
public abstract class NetworkCharacter : NetworkBehaviour
{
	[SerializeField] private float serverUpdateDelay = 0.01f;
	[SerializeField] private float clientUpdateDelay = 0.01f;
    [SerializeField] protected float moveSpeed = 1.0f;
	[SerializeField] protected float maxHealth = 3f;

    public bool isMoving;

	[SyncVar] public Vector2 facing;
	[SyncVar] public float health;

	protected SpriteRenderer spriteRenderer;
	protected Animator animator;

    [ClientRpc]
    protected void RpcMoveStart(Vector3 startPosition, Vector3 endPosition)
    {
		// Set our position to where we should be.
		transform.position = startPosition;

		// Start movement.
		StartCoroutine(Move(endPosition));
    }

    [ClientRpc]
    protected void RpcMoveFinish(Vector3 newPosition)
    {

    }

	[ClientRpc]
	protected void RpcChangeDirection(Vector2 direction)
	{
		// Update sprite flip.
		spriteRenderer.flipX = (direction.x != 0 && direction.x == -1 ? true : false);
	}

	[TargetRpc]
	protected void TargetUpdatePosition(NetworkConnection target, Vector3 position) 
	{
		transform.position = position;
	}

	[Client]
	protected abstract IEnumerator ClientUpdate();

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

	[Client]
	protected IEnumerator LocalUpdate()
	{
		while (true)
		{
			// Update sprite flip.
			spriteRenderer.flipX = (facing.x != 0 && facing.x == -1 ? true : false);

			// Call client update function.
			yield return ClientUpdate();

			yield return new WaitForSeconds(clientUpdateDelay);
		}
	}
	
	[Command]
	protected void CmdGetPosition() {
		try
		{
			TargetUpdatePosition(connectionToClient, transform.position);
		}
		catch (System.NullReferenceException)
		{

		}
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
		// Update facing direction.
		facing = direction;
		UpdateDirection();

		// Check if we can move.
		if (GetCanMove(direction)) 
		{
			// Get our target position.
			Vector3 targetPosition = new Vector3(Mathf.Floor(transform.position.x + direction.x) + 0.5f,
												Mathf.Ceil(transform.position.y + direction.y) - 0.5f,
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
	public void TakeDamage(float amt)
	{
		// Subtract damage.
		health -= amt;

		// Damage callback.
		OnTakeDamage(amt);
	}

	[Server]
    private IEnumerator NetworkUpdate()
    {
        while (true)
        {
			// Check for death.
			if (health <= 0.0f)
			{
				OnDie();
			}

			// Call character update.
			yield return ServerUpdate();

			// Wait set time.
            yield return new WaitForSeconds(serverUpdateDelay);
        }
    }

	[Server] protected abstract IEnumerator ServerUpdate();
	[Server] protected virtual void OnMoveStart() { }
	[Server] protected virtual void OnMoveFinish() { }
	[Server] protected virtual void OnTakeDamage(float amt) { }
	[Server] protected virtual void OnDie() { }

	[Server]
	protected IEnumerator NetworkMove(Vector2 direction, Vector3 targetPosition) 
	{
		// Set moving state.
		isMoving = true;

		// Set animation parameters.
		animator.SetBool("isMoving", true);

		// Callback for move start.
		OnMoveStart();

		// Start character movement on all clients.
		RpcMoveStart(transform.position, targetPosition);

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

	[Server]
	protected void UpdateDirection()
	{
		if (facing.x != 0) 
		{
			animator.SetFloat("direction", 0.5f);
		} 
		else if (facing.y != 0) 
		{
			switch ((int)facing.y) 
			{
				case -1:
					animator.SetFloat("direction", 0.0f);
					break;
				case 1:
					animator.SetFloat("direction", 1.0f);
					break;
			}
		}
	}

    protected void Start()
    {
		// Get components.
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();

        if (isServer)
        {
			// Update health.
			health = maxHealth;

			// Fix player position.
			transform.position = new Vector3(Mathf.Floor(transform.position.x) + 0.5f, Mathf.Floor(transform.position.y) + 0.5f, transform.position.z);

			// Update initial facing direction.
			facing = new Vector2(0, -1);

			// Start server update coroutine.
            StartCoroutine(NetworkUpdate());
        }
		
		if (isClient)
		{
			// Get initial position.
			CmdGetPosition();

			// Start client update coroutine.
			StartCoroutine(LocalUpdate());
		}
    }
}
