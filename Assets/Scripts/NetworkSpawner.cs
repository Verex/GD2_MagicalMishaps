using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSpawner : NetworkBehaviour
{

    [SerializeField] private float spawnDelay = 1.0f;
    [SerializeField] private int maxMobs = 10;
    [SerializeField] private Transform[] spawnLocations;
    [SerializeField] private GameObject[] mobPrefabs;

    private List<GameObject> spawned;

    [Server]
    private IEnumerator SpawnUpdate()
    {
        while (true)
        {
			// Remove empty game objects.
			spawned.RemoveAll(g => g == null);

            if (spawnLocations.Length > 0 && mobPrefabs.Length > 0 && spawned.Count < maxMobs)
            {
                // Select random spawn location and mob.
                Transform spawnTransform = spawnLocations[Random.Range(0, spawnLocations.Length - 1)];

				// Check for empty position.
				Collider2D collider = Physics2D.OverlapPoint(spawnTransform.position);

				// Check for empty space.
				if (collider == null)
				{
					GameObject prefab = mobPrefabs[Random.Range(0, mobPrefabs.Length - 1)];

					// Spawn mob at location.
					GameObject mob = Instantiate(prefab, spawnTransform.position, Quaternion.identity);
					NetworkServer.Spawn(mob);

					// Add mob to spawned list.
					spawned.Add(mob);

					yield return new WaitForSeconds(spawnDelay);
				}
            }

            yield return null;
        }
    }

    void Start()
    {
        if (isServer)
        {
            spawned = new List<GameObject>();
            StartCoroutine(SpawnUpdate());
        }
    }
}
