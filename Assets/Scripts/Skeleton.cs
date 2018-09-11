using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Skeleton : NetworkBehaviour {

	[SerializeField] private float moveSpeed = 1.0f;
	[SerializeField] private bool isMoving;

	private Vector3 startPosition;
	private Vector3 endPosition;

	private IEnumerator SlowUpdate() {
		while (true) {

			/*
				RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, 1.0f);

				if (hit.collider != null) {
					Debug.Log("We hit something.");
					yield return new WaitForSeconds(200.0f);
				}
			 */

			//transform.position += new Vector3(1, 0, 0);

			if (!isMoving) {
				StartCoroutine(Move());
			}
			yield return new WaitForSeconds(1.0f);
		}
	}

	private IEnumerator Move() {
		isMoving = true;
		startPosition = transform.position;
		float t = 0.0f;

		endPosition = new Vector3(startPosition.x + 1, startPosition.y, startPosition.z);

		float factor = 1.0f;

		while (t < 1.0f) {
			t += Time.deltaTime * moveSpeed * factor;
			transform.position = Vector3.Lerp(startPosition, endPosition, t);
			yield return null;
		}

		isMoving = false;

		yield break;
	}

	// Use this for initialization
	void Start () {
		if (isServer) {
			StartCoroutine(SlowUpdate());
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
