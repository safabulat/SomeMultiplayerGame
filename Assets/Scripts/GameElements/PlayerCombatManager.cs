using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerCombatManager : CombatManagerBase
{
    public NetworkVariable<Vector3> teamBasePos = new NetworkVariable<Vector3>(Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public Vector3 TeamBasePos = Vector3.zero;
    public float thisID; //Debug purposes delete later
    public override void OnNetworkSpawn()
    {

        base.OnNetworkSpawn();

        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        TeamBasePos = Vector3.zero;
        Vector3 offset = new Vector3(1, 0, 0);

        thisID = OwnerClientId;
        if (gm != null)
        {
            if (IsOwner)
            {
                if(gm.thisPlayerSelectedTeam == 0)
                {
                    ChangeTeamServerRpc(0);
                }
                else if (gm.thisPlayerSelectedTeam == 1)
                {
                    ChangeTeamServerRpc(1);
                }
                if (teamBasePos.Value != Vector3.zero)
                {
                    transform.position = teamBasePos.Value;
                }
            }
        }

    }

    [ServerRpc(RequireOwnership = true)]
    private void ChangeTeamServerRpc(int team)
    {
        TeamBasePos = Vector3.zero;
        Vector3 offset = new Vector3(1, 0, 0);
        if (team == 0)
        {
            this.team = Teams.Blue;
            teamBasePos.Value = GameObject.Find("TeamBaseBlue").transform.position + offset;
        }
        else if (team == 1)
        {
            this.team = Teams.Red;
            teamBasePos.Value = GameObject.Find("TeamBaseRed").transform.position - offset;
        }
        if (teamBasePos.Value != Vector3.zero)
        {
            transform.position = teamBasePos.Value;
        }
    }

    void Update()
    {
        if(!IsOwner) { return; }
        if (Input.GetMouseButton(0))
        {
            MouseTarget();
        }
    }

    private void MouseTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            CombatManagerBase targetCombatManager = null; // Declare targetCombatManager of type CombatManagerBase

            if (hit.transform.tag == "Player")
            {
                targetCombatManager = hit.transform.GetComponent<PlayerCombatManager>();
            }
            else if (hit.transform.tag == "Minion")
            {
                targetCombatManager = hit.transform.GetComponent<MinionCombatManager>();
            }
            else if (hit.transform.tag == "Tower" || hit.transform.tag == "Base")
            {
                targetCombatManager = hit.transform.GetComponent<TowerCombatManager>();
            }
            else
            {
                Debug.Log("Non Valid Target: " + hit.transform.name + " : " + hit.transform.tag);
            }

            // Check if the hit object has a CombatManager script and is targetable
            if (targetCombatManager != null && targetCombatManager.isTargetable)
            {
                // Check if the target is not from the same team
                if (targetCombatManager.team != this.team)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, hit.transform.position);
                    if (distanceToTarget <= currentAttackRange)
                    {
                        Debug.Log(OwnerClientId +  " attacking to: " + hit.transform.GetComponent<NetworkObject>().OwnerClientId + " _ " + hit.transform.name);
                        Attack(hit.transform.gameObject, damage);
                    }
                }
            }
        }
    }

    public override void Attack(GameObject target, float _damage)
    {
        if(!isAttacking)
        {
            isAttacking = true;

            base.Attack(target, _damage);
            StartCoroutine(AttackCoolDown(attackCoolDownTime));
        }
    }

    IEnumerator AttackCoolDown(float attackSpeed)
    {
        yield return new WaitForSeconds(attackSpeed);
        isAttacking = false;
    }

}
