using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class projectile_fireball : NetworkBehaviour
{
    [SerializeField] private float damage = 1.0f;
    
    public NetworkCharacter owner;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (isServer)
        {
            if (col.collider.gameObject.layer == LayerMask.NameToLayer("Characters"))
            {
                NetworkCharacter nc = col.collider.gameObject.GetComponent<NetworkCharacter>();

                if (nc.TakeDamage(damage) <= 0)
                {
                    if (owner != null && nc != null)
                    {
                        owner.OnKill(nc);
                    }
                }
            }

            NetworkServer.Destroy(this.gameObject);
        }
    }
}
