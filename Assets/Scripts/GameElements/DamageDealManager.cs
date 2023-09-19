using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DamageDealManager : NetworkBehaviour
{
    [SerializeField] GameObject rangedProjectilePrefab;
    public static DamageDealManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void DealtDamage(NetworkObjectReference attacker, NetworkObjectReference attacked)
    {
        HandleDamageingServerRpc(attacker, attacked);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleDamageingServerRpc(NetworkObjectReference attacker, NetworkObjectReference attacked)
    {
        attacker.TryGet(out NetworkObject attackerNetworkObject);
        attacked.TryGet(out NetworkObject attackedNetworkObject);
        if(attackerNetworkObject == null) { Debug.Log("DamageDealManager: Null attackerNetworkObject"); return; }
        if (attackedNetworkObject == null) { Debug.Log("DamageDealManager: Null attackedNetworkObject"); return; }

        GameObject attackerObj = attackerNetworkObject.gameObject;
        GameObject attackedObj = attackedNetworkObject.gameObject;
        if (attackerObj == null) { Debug.Log("DamageDealManager: Null attackerObj"); return; }
        if (attackedObj == null) { Debug.Log("DamageDealManager: Null attackedObj"); return; }

        CombatManagerBase attackerDMG = attackerObj.GetComponent<CombatManagerBase>();
        Health attackedHP = attackedObj.GetComponent<Health>();

        if(!CheckIfValidAttack(
            attackerObj.transform.position, 
            attackedObj.transform.position,
            attackerDMG.currentAttackRange))
        { Debug.Log("Not a Valid Attack"); return; }


        float damagedHP = attackedHP.GetCurrentHealth() - attackerDMG.damage;
        bool dead = damagedHP <= 0;

        if (attackedObj.tag == "Player")
        {
            Debug.Log("DamagedHP: " + damagedHP + " flag: " + dead);
        }

        attackedHP.decreaseHealthRequest = attackerDMG.damage;

        if(dead && attackedObj.tag == "Player")
        {
            Debug.Log("Triggered Event");
            PlayerCombatManager playerDMG = attackerObj.GetComponent<PlayerCombatManager>();
            playerDMG.FireEventOnKill();
        }

        if(attackerDMG.type == CombatManagerBase.CombatType.Ranged)
        {
            SpawnProjectileThroughTarget(attackedObj, attackedObj, attackerDMG.damage, attackerDMG.projectileSpawnPoint);
        }
    }

    private bool CheckIfValidAttack(Vector3 attackerPos, Vector3 attackedPos, float AttackRange)
    {
        if(Vector3.Distance(attackedPos, attackerPos) <= AttackRange) { return true; }
        else { return false; }
    }
    private void SpawnProjectileThroughTarget(GameObject parent, GameObject target, float damage, Transform projectileSpawnPoint)
    {
        GameObject projectile = Instantiate(rangedProjectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        if (projectile == null) { Debug.Log("Projectile Null"); }
        NetworkObject _projectileNetworkObj = projectile.GetComponent<NetworkObject>();
        if (_projectileNetworkObj == null) { Debug.Log("_projectileNetworkObj Null"); }
        ProjectileBehaviour projectileBehavior = projectile.GetComponent<ProjectileBehaviour>();
        if (_projectileNetworkObj == null) { Debug.Log("projectileBehavior Null"); }
        projectileBehavior.target = target;
        projectileBehavior.parent = parent;
        projectileBehavior.damage = damage;

        _projectileNetworkObj.Spawn(projectile);

        Debug.Log(gameObject.name + " shooting a projectile to " + target.name + " Type: Ranged");
    }
}
