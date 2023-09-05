using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TowerCombatManager : CombatManagerBase
{
    private GameObject currentTarget;
    [SerializeField] float towerRange;

    private void Start()
    {
        currentAttackRange = towerRange;
        rangedAttackRange = towerRange;

        // Start attacking immediately and continue attacking at intervals
        StartCoroutine(AttackLoop());
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            if (currentTarget == null || !IsValidTarget(currentTarget.GetComponent<CombatManagerBase>()) || IsOutOfRange(currentTarget))
            {
                // Release the current target if it's invalid or out of range
                currentTarget = null;
            }

            if (currentTarget == null)
            {
                // Find a new target if the current target is null
                currentTarget = FindNearestValidTarget();
            }

            if (currentTarget != null)
            {
                Attack(currentTarget, damage);
            }

            yield return new WaitForSeconds(attackCoolDownTime);
        }
    }

    private GameObject FindNearestValidTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, currentAttackRange);

        float closestDistance = Mathf.Infinity;
        GameObject nearestTarget = null;

        foreach (var collider in colliders)
        {
            CombatManagerBase combatManager = collider.GetComponent<CombatManagerBase>();
            if (combatManager != null && combatManager != this && IsValidTarget(combatManager))
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestTarget = collider.gameObject;
                }
            }
        }
        if (nearestTarget != null)
        {
            Debug.Log("NearestTargetName: " + nearestTarget.name);
        }
        return nearestTarget;
    }

    private bool IsOutOfRange(GameObject target)
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);
        return distance > towerRange;
    }

    private bool IsValidTarget(CombatManagerBase target)
    {   
        if (target.team != team && target.isTargetable)
        {
            return true;
        }
        return false;
    }
}
