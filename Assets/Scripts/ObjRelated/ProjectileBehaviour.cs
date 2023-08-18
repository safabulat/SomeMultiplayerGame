using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileBehaviour : NetworkBehaviour
{
    public float projectileSpeed;
    public GameObject target;
    public float damage;

    private ulong ownerClientId; // Store the owner's client ID

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Set the owner client ID
        ownerClientId = NetworkObject.OwnerClientId;
    }

    private void Update()
    {
        if (!IsServer)
            return;

        if (target == null)
        {
            DestroyProjectile();
            return;
        }

        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
        transform.position += directionToTarget * projectileSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
            return;

        var targetHM = other.gameObject.GetComponent<Health>();

        if (targetHM != null)
        {
            targetHM.GetTakenDamageServerRpc(damage);
        }

        // Check if the owner is trying to destroy the projectile
        if (NetworkObject.OwnerClientId == ownerClientId)
        {
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        NetworkObject.Destroy(gameObject);
    }
}