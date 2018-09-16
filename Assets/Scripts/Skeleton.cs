using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Skeleton : NetworkNPC
{

    protected override IEnumerator ClientUpdate()
    {

        yield return null;
    }

    protected override IEnumerator ServerUpdate()
    {
		// Our base npc server-side update.
        yield return base.ServerUpdate();

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

    protected override void OnDie()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    new private void Start()
    {
        base.Start();

		if (hasGridSystem)
		{
			FindPath(new Vector2Int(6, -3));
		}
    }
}
