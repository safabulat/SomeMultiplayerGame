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
    

    private GameObject target = null; // Store the target as a GameObject


    public virtual void Attack(GameObject target, float damage)
    {
        this.target = target;
        this.damage = damage;

        if(target == null) { Debug.Log("NoTargetAtAll"); return; }

        switch (type)
        {
            case CombatType.Melee:
                MeleeAttackServerRpc();
                break;
            case CombatType.Ranged:
                Debug.Log(gameObject.name + " attacking to " + target.name + " Type: Ranged");
                RangedAttackServerRpc();
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void MeleeAttackServerRpc()
    {
        MeleeAttackClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RangedAttackServerRpc()
    {
        
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
