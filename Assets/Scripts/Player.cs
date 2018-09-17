using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;

[NetworkSettings(sendInterval = 0.008f)]
public class Player : NetworkCharacter
{

    [SerializeField] private float inputDelay = 0.01f;
    [SerializeField] private float inputMoveThreshold = 0.2f;
    [SerializeField] private float inputDirThreshold = 0.1f;
    [SerializeField] private float attackProjectileSpeed = 2.0f;
    [SerializeField] private float attackDelay = 2.0f;
    [SerializeField] private float meleeDelay = 1.0f;
    [SerializeField] private float movementDelay = 0.4f;
    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private GameObject UIPrefab;
    [SerializeField] private GameObject attackProjectilePrefab;
    [SerializeField] private GameObject meleeProjectilePrefab;

    [SyncVar] public int score = 0;

    private GameObject camera;
    private Vector2 moveDirection;
    private Vector2 lastDirection;
    private bool shouldMove = false;
    private bool lastMove;
    private bool lastAttack;
    private float lastAttackTime = 0.0f;
    private float lastMeleeTime = 0.0f;
    private float lastMoveTime = 0.0f;
    private PlayerInput playerInput;

    [Command]
    private void CmdMoveState(bool moving, Vector2 direction)
    {
        if (moving != shouldMove)
        {
            shouldMove = moving;

            if (shouldMove == true)
            {
                lastMoveTime = Time.time;
            }
        }

        // Check if we're trying to change move direction.
        if (moveDirection != direction && direction != Vector2.zero)
        {
            moveDirection = direction;

            // Update our animation if needed.
            if (!isMoving)
            {
                facing = direction;
                UpdateDirection();
            }
        }

        moveDirection = direction;
    }

    [Command]
    private void CmdAttack()
    {
        if (lastAttackTime + attackDelay <= Time.time)
        {
            lastAttackTime = Time.time;

            GameObject p = Instantiate(attackProjectilePrefab, transform.position + new Vector3(facing.x, facing.y, transform.position.z), Quaternion.identity);
            p.GetComponent<Rigidbody2D>().AddForce(facing * attackProjectileSpeed, ForceMode2D.Impulse);
            p.GetComponent<projectile_fireball>().owner = this;
            NetworkServer.Spawn(p);
        }
    }

    [Command]
    private void CmdMelee()
    {
        if (lastMeleeTime + meleeDelay <= Time.time && !isMoving)
        {
            lastMeleeTime = Time.time;

            float angle = Mathf.Atan2(-facing.y, -facing.x) * Mathf.Rad2Deg;

            GameObject m = Instantiate(meleeProjectilePrefab,
                                    new Vector3(transform.position.x + facing.x, transform.position.y + facing.y, -2),
                                    Quaternion.AngleAxis(angle, Vector3.forward));
            m.GetComponent<projectile_melee>().owner = this;
            NetworkServer.Spawn(m);
        }
    }

    protected override IEnumerator ClientUpdate()
    {

        yield return null;
    }

    [Client]
    private IEnumerator UpdateInput()
    {
        while (true)
        {
            if (playerInput.Actions != null)
            {
                // Capture input.
                Vector2 inputDirection = (Vector2)playerInput.Actions.Move,
                        absInputDirection = new Vector2(Mathf.Abs(inputDirection.x), Mathf.Abs(inputDirection.y)),
                        direction = Vector2.zero;
                // Set default state of move.
                bool move = false;

                // Check if we are trying to move.
                if (absInputDirection.x > inputDirThreshold)
                {
                    direction = new Vector2((absInputDirection.x / inputDirection.x) * Mathf.Ceil(absInputDirection.x), 0);

                    if (absInputDirection.x > inputMoveThreshold)
                    {
                        move = true;
                    }
                }
                else if (absInputDirection.y > inputDirThreshold)
                {
                    // Move vertically.
                    direction = new Vector2(0, (absInputDirection.y / inputDirection.y) * Mathf.Ceil(absInputDirection.y));

                    if (absInputDirection.y > inputMoveThreshold)
                    {
                        move = true;
                    }
                }

                if (direction != lastDirection || move != lastMove)
                {
                    CmdMoveState(move, direction);
                    lastDirection = direction;
                    lastMove = move;
                }
            }

            yield return new WaitForSeconds(inputDelay);
        }
    }

    // Server side update.
    protected override IEnumerator ServerUpdate()
    {
        if (!isMoving && shouldMove && lastMoveTime + movementDelay < Time.time && health > 0)
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
            // Spawn local camera.
            camera = Instantiate(cameraPrefab);

            camera.transform.parent = transform;
            camera.transform.localPosition = new Vector3(0, 0, -10);

            // Create player's UI.
            GameObject ui = Instantiate(UIPrefab);

            ui.GetComponent<UIControllerGame>().playerTarget = this;

            // Add player input component.
            playerInput = gameObject.AddComponent(typeof(PlayerInput)) as PlayerInput;

            // Start input coroutine.
            StartCoroutine(UpdateInput());
        }
    }

    public override void OnKill(NetworkCharacter character)
    {
        if (character is Mage)
        {
            score += 2;
        }
        else if (character is Player)
        {
            score += 3;
        }
        else
        {
            score++;
        }
    }

    protected override IEnumerator OnDie()
    {
        // Reset position.
        transform.position = new Vector3(-2.5f, -2.5f, 0);

        // Reset health.
        health = maxHealth;

        // Reset score.
        score = 0;

        yield return new WaitForSeconds(1.8f);

        RpcUpdatePosition(transform.position);

        yield break;
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            // Check if we're trying to attack.
            if (playerInput.Actions.Attack.WasReleased)
            {
                CmdAttack();
            }

            if (playerInput.Actions.Melee.WasReleased)
            {
                CmdMelee();
            }
        }
    }

}
