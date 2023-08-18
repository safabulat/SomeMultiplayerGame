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

        foreach (var collider in colliders)
        {
            CombatManagerBase combatManager = collider.GetComponent<CombatManagerBase>();
            if (combatManager == null) { continue; }
            if (combatManager == this) { continue; }
            if (!IsValidTarget(combatManager)) { continue; }
            if (!IsThereCloserTarget(nearestTarget.transform.position, combatManager.transform.position)) { return; }
            nearestTarget = combatManager;
        }

        if (nearestTarget != null)
        {
            if (IsInAttackRange(nearestTarget.transform.position))
            {
                Attack(nearestTarget.gameObject, damage);
            }
            else
            {
                minionAgent.SetDestination(nearestTarget.transform.position);
            }
        }
        else
        {
            // Go directly to the predefined enemy base position
            minionAgent.SetDestination(targetBase);
        }
    }

    public override void Attack(GameObject target, float _damage)
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
