using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class MinionCombatManager : CombatManagerBase
{
    NavMeshAgent minionAgent;
    public Vector3 targetBase, targetMinion, targetPlayer;

    [SerializeField] float searchRange = 10f; // Adjust this value as needed

    private void Start()
    {
        minionAgent = GetComponent<NavMeshAgent>();

        switch (type)  // Use the enum directly
        {
            case CombatType.Ranged:
                currentAttackRange = rangedAttackRange;
                break;
            case CombatType.Melee:
                currentAttackRange = meleeAttackRange;
                break;
        }
    }

    private void Update()
    {
        FindAndAttackTarget();
    }

    private void FindAndAttackTarget()
    {
        // Find the nearest target within the search range
        Collider[] colliders = Physics.OverlapSphere(transform.position, searchRange);

        CombatManagerBase nearestTarget = null;
        NetworkObjectReference? networkObjectReference = null; // Use a nullable NetworkObjectReference
        foreach (var collider in colliders)
        {
            CombatManagerBase combatManager = collider.GetComponent<CombatManagerBase>();
            if (combatManager == null) { continue; }
            if (combatManager == this) { continue; }
            if (!IsValidTarget(combatManager)) { continue; }
            if (nearestTarget != null)
            {
                if (!IsThereCloserTarget(nearestTarget.transform.position, combatManager.transform.position)) { return; }
            }
            nearestTarget = combatManager;

            // Check if the target has a NetworkObject component and it is spawned
            NetworkObject targetNetworkObject = combatManager.gameObject.GetComponent<NetworkObject>();
            if (targetNetworkObject != null && targetNetworkObject.IsSpawned)
            {
                networkObjectReference = targetNetworkObject; // Assign a valid NetworkObjectReference
            }
        }

        if (nearestTarget != null)
        {
            if (networkObjectReference.HasValue) // Check if it has a value (not null)
            {
                if (IsInAttackRange(nearestTarget.transform.position))
                {
                    Attack(networkObjectReference.Value, damage); // Access the value using .Value
                }
                else
                {
                    minionAgent.SetDestination(nearestTarget.transform.position);
                }
            }
            else
            {
                // Handle the case when the target's NetworkObject is not spawned yet or is not available
            }
        }
        else
        {
            // Go directly to the predefined enemy base position
            minionAgent.SetDestination(targetBase);
        }
    }

    public override void Attack(NetworkObjectReference target, float _damage)
    {
        if (!isAttacking)
        {
            isAttacking = true;

            base.Attack(target, _damage);
            StartCoroutine(AttackCoolDown(attackCoolDownTime));
        }
    }

    private bool IsValidTarget(CombatManagerBase target)
    {
        if (target.team == Teams.Natural || target.team != team)
        {
            if(target.isTargetable)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private bool IsInAttackRange(Vector3 targetPos)
    {
        if (Vector3.Distance(transform.position, targetPos) <= currentAttackRange)
        {
            return true;
        }
        return false;
    }

    private bool IsThereCloserTarget(Vector3 currTargetPos, Vector3 otherTargetPos)
    {
        if (Vector3.Distance(transform.position, otherTargetPos) <= Vector3.Distance(transform.position, currTargetPos))
        {
            return true;
        }
        return false;
    }

    IEnumerator AttackCoolDown(float attackSpeed)
    {
        yield return new WaitForSeconds(attackSpeed);
        isAttacking = false;
    }
}
