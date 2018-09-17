using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class projectile_melee : NetworkBehaviour
{
    [SerializeField] private float damage = 2.0f;
    [SerializeField] private float delay = 1.0f;

    private BoxCollider2D boxCollider;

    public NetworkCharacter owner;

    private IEnumerator ProjectileUpdate()
    {
        yield return new WaitForSeconds(delay);

        Collider2D[] colliders = Physics2D.OverlapPointAll(transform.position);

        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("Characters"))
            {
                if (collider.gameObject == this.gameObject) continue;

                NetworkCharacter nc = collider.gameObject.GetComponent<NetworkCharacter>();

                if (nc.TakeDamage(owner, damage) <= 0)
                {
                    if (owner != null && nc != null)
                    {
                        owner.OnKill(nc);
                    }
                }
            }
        }

        // Destroy.
        NetworkManager.Destroy(this.gameObject);

        yield break;
    }

    void Start()
    {
        if (isServer)
        {
            // Get box collider component.
            boxCollider = GetComponent<BoxCollider2D>();

            StartCoroutine(ProjectileUpdate());
        }
    }

    void Update()
    {

    }
}
