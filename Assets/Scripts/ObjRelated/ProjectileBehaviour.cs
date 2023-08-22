using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileBehaviour : NetworkBehaviour
{
    public float projectileSpeed;
    public GameObject target, parent;
    public float damage;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    private void Update()
    {
        //if (!IsServer)
        //    return;

        if (target == null)
        {
            //DestroyProjectile();
            return;
        }
        var targetHM = target.gameObject.GetComponent<Health>();

        if (targetHM == null)
        {
            return;
        }
        if(targetHM.isPlayerDead) { DestroyProjectile(); }

        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
        transform.position += directionToTarget * projectileSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (!IsServer)
        //    return;

        var targetHM = other.gameObject.GetComponent<Health>();
        var parentCombatManager = parent.GetComponent<PlayerCombatManager>();
        float targetHp = 0f;
        int targetType = -1;

        if(other.tag == "Minion") { targetType = 3; }
        else if(other.tag == "Player") { targetType = 1; }

        if (targetHM != null)
        {
            targetHp = targetHM.GetCurrentHealth();            
            targetHM.GetTakenDamageServerRpc(damage);
            targetHp -= damage;
            Debug.Log("Hp Left: " + targetHp + " Type: " + targetType + " tag: " + other.tag);
        }

        if(parentCombatManager != null && targetHM != null)
        {
            parentCombatManager.targetsHPLeft = targetHp;
            parentCombatManager.targetType = targetType;
        }
        else { Debug.Log("Null parent etc"); }

        // Check if the owner is trying to destroy the projectile
        if (IsServer && IsOwner)
        {
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        NetworkObject.Destroy(gameObject);
    }
}
