using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Skeleton : NetworkBehaviour
{

    [SyncVar] private Vector3 targetPosition;

    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private bool isMoving;

    private Vector3 startPosition;
    private Vector3 endPosition;

	private int direction = 1;
    private IEnumerator SlowUpdate()
    {
        while (true)
        {
            if (!isMoving)
            {
				Vector2 moveDirection = new Vector2(direction, 0);

                RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, 1.0f);

                if (hit.collider == null)
                {

					// Change serverside transform.
					StartCoroutine(Move(moveDirection));
                }
                else
                {
                    Debug.Log("We hit something.");
					direction *= -1;
                }
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    [ClientRpc]
    private void RpcMoveEntity(Vector2 direction, Vector3 startPosition)
    {
		// Don't want host doing this.
		if (isServer) return;

        Debug.Log("Move cmd recieved.");

        if (!isMoving)
        {
			transform.position = startPosition;
            StartCoroutine(Move(direction));
        }
    }

    private IEnumerator Move(Vector2 direction)
    {
        isMoving = true;
        startPosition = transform.position;
        float t = 0.0f;

        // Set ending position to synced target position.
        endPosition = new Vector3(transform.position.x + direction.x,
                                    transform.position.y + direction.y,
                                    transform.position.z);

		if (isServer) {
			targetPosition = endPosition;

			// Send move rpc to clients.
			//RpcMoveEntity(direction, startPosition);
		}

        float factor = 1.0f;

        while (t < 1.0f)
        {
            t += Time.deltaTime * moveSpeed * factor;

            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            yield return null;
        }

        isMoving = false;

        yield break;
    }

    // Use this for initialization
    void Start()
    {
        if (isServer)
        {
            targetPosition = transform.position;

            StartCoroutine(SlowUpdate());
        }
    }

    // Update is called once per frame
    void Update()
    {
		if (isClient && !isServer) {
			if (transform.position != targetPosition) {
				
			}
		}
    }

    private void OnClientStart()
    {
        transform.position = targetPosition;
    }
}
