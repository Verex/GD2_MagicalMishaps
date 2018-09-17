using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Skeleton : NetworkNPC
{
    [SerializeField] private float attackDelay = 1.0f;
    [SerializeField] private GameObject meleeProjectilePrefab;
    private Player target;

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

    private void CheckTargetPosition(float maxDist)
    {
        // Cancel if we're moving.
        if (target.isMoving) return;

        // Check if player still in target position.
        Vector3Int targetPosition = WorldToCell(target.transform.position);
        Vector2Int dest = new Vector2Int(targetPosition.x, targetPosition.y);

        // Check how far target has moved from path.
        if (Vector2.Distance(pathDestination, dest) >= maxDist)
        {
            // Clear old path.
            ClearPath();

            // Compute new path.
            FindPath(dest);
        }
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.2f);

        float angle = Mathf.Atan2(-facing.y, -facing.x) * Mathf.Rad2Deg;

        GameObject m = Instantiate(meleeProjectilePrefab,
                                new Vector3(transform.position.x + facing.x, transform.position.y + facing.y, -2),
                                Quaternion.AngleAxis(angle, Vector3.forward));
        m.GetComponent<projectile_melee>().owner = this;
        NetworkServer.Spawn(m);

        yield return new WaitForSeconds(attackDelay);
    }

    private IEnumerator FindPlayer()
    {
        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance <= 5.0f)
        {
            // Raycast and check for player.
            RaycastHit2D hit = Physics2D.Raycast(transform.position, facing, 1.0f, LayerMask.GetMask("Characters"));

            // Check for hit.
            if (hit.collider != null)
            {
                // Check if we hit player.
                if (hit.collider.tag == "Player")
                {
                    // Clear path.
                    ClearPath();

                    // Do attack.
                    yield return Attack();

                    yield break;
                }
            }

            CheckTargetPosition(0.5f);
        }
        else
        {
            // Clear target;
            target = null;
        }

        yield return null;
    }

    [Server]
    protected override IEnumerator OnPathObstruction()
    {
        yield return base.OnPathObstruction();

        yield return FindPlayer();

        yield return new WaitForSeconds(0.2f);
    }

    protected override IEnumerator OnPathMove()
    {
        // When we move along the path.
        CheckTargetPosition(2.0f);

        yield return null;
    }

    protected override IEnumerator OnHasNoPath()
    {
        if (target == null)
        {
            Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(5f, 5f), LayerMask.GetMask("Characters"));

            // Check if players nearby.
            foreach (Collider2D c in colliders)
            {
                if (c.tag == "Player")
                {
                    // Get player component.
                    target = c.gameObject.GetComponent<Player>();

                    if (hasGridSystem)
                    {
                        // Get cell of target.
                        Vector3Int targetPosition = WorldToCell(target.transform.position);
                        Debug.Log(targetPosition);

                        // Find path to player.
                        FindPath(new Vector2Int(targetPosition.x, targetPosition.y));

                        yield break;
                    }
                }
            }

            int move = Random.Range(0, 5);

            if (move == 0)
            {
                // Choose random move direction.
                moveDirection = RandomDirection();

                MoveCharacter(moveDirection);

                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                StopMovement();

                yield return new WaitForSeconds(1.2f);
            }
        }
        else
        {
            yield return FindPlayer();
        }
    }

    [Server]
    protected override void OnTakeDamage(NetworkCharacter attacker, float damage)
    {
        if (attacker == null) return;

        // Change targets.
        if (attacker.tag == "Player")
        {
            target = (Player)attacker;
        }
    }

    public override void OnKill(NetworkCharacter character)
    {
        if (character == target)
        {
            target = null;
        }
    }

    protected override IEnumerator OnDie()
    {
        NetworkServer.Destroy(this.gameObject);

        yield return new WaitForSeconds(1.0f);
        yield break;
    }

    new private void Start()
    {
        base.Start();
    }
}
