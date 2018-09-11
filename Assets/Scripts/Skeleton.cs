using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Skeleton : NetworkBehaviour {

	private IEnumerator SlowUpdate() {
		while (true) {
			RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, 1.0f);

			if (hit.collider != null) {
				Debug.Log("We hit something.");
				yield return new WaitForSeconds(200.0f);
			}

			transform.position += new Vector3(1, 0, 0);

			yield return new WaitForSeconds(2.0f);
		}
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
