using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CombatManagerBase : NetworkBehaviour
{
    public enum CombatType
    {
        None,
        Ranged,
        Melee
    }
    public CombatType type;

    public enum Teams
    {
        Blue,
        Red,
        Natural
    }
    public Teams team;

    [SerializeField] GameObject rangedProjectilePrefab, meleeHolder, meleeWeapon;
    public Transform projectileSpawnPoint;

    
    public float damage;
    public bool isTargetable;
    public float currentAttackRange, meleeAttackRange, rangedAttackRange;
    public float attackCoolDownTime = 1;
    protected bool isAttacking = false;

    public NetworkVariable<int> networkTeams = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private GameObject target = null; // Store the target as a GameObject


    public virtual void Attack(GameObject target, float damage)
    {


        if(target == null) { Debug.Log("NoTargetAtAll"); return; }

        switch (type)
        {
            case CombatType.Melee:
                this.target = target;
                this.damage = damage;
                MeleeAttackServerRpc();
                break;
            case CombatType.Ranged:
                Debug.Log(gameObject.name + " attackingXXX to " + target.name + " Type: Ranged");
                RangedAttackServerRpc(target, damage);
                break;
            default:
                Debug.Log("Unknown attack type: " + type);
                break;
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void RangedAttackServerRpc(NetworkObjectReference networkObjectReference, float damage)
    {
        networkObjectReference.TryGet(out NetworkObject targetNetworkObject);
        if(targetNetworkObject == null) { Debug.Log("No targetNetworkObject"); return; }
        GameObject target = targetNetworkObject.gameObject;
        if (target == null) { Debug.Log("No Target"); return; } else { Debug.Log("Target: " + target.name); }   
        GameObject projectile = Instantiate(rangedProjectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        if(projectile == null) { Debug.Log("Projectile Null"); }
        NetworkObject _projectileNetworkObj = projectile.GetComponent<NetworkObject>();
        if (_projectileNetworkObj == null) { Debug.Log("_projectileNetworkObj Null"); }
        ProjectileBehaviour projectileBehavior = projectile.GetComponent<ProjectileBehaviour>();
        if (_projectileNetworkObj == null) { Debug.Log("projectileBehavior Null"); }
        projectileBehavior.target = target;
        projectileBehavior.parent = gameObject;
        projectileBehavior.damage = damage;

        _projectileNetworkObj.Spawn(projectile);

        Debug.Log(gameObject.name + " shooting a projectile to " + target.name + " Type: Ranged");
    }

    [ServerRpc(RequireOwnership = false)]
    private void MeleeAttackServerRpc()
    {
        MeleeAttackClientRpc();
    }

    [ClientRpc]
    private void MeleeAttackClientRpc()
    {
        if (target == null) { return; }
        var targetHM = target.GetComponent<Health>();
        if (targetHM == null)
        {
            target = null;
            Debug.Log("No Melee Target?");
            return;
        }
        targetHM.GetTakenDamageServerRpc(damage);
    }
}
