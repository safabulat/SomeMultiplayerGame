using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCombatManager : CombatManagerBase
{
    public float thisID; //Debug purposes delete later
    public Vector3 teamBasePos = Vector3.zero;
    public bool isPlayerDead = false;
    public float playerReSpawnTime;

    GameManager gm = null;

    public UnityEvent<float,bool> NotifyUISpawnTime;

    public override void OnNetworkSpawn()
    {

        base.OnNetworkSpawn();

        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        thisID = OwnerClientId;
        if (gm != null)
        {
            if (IsOwner) // && IsClient
            {
                if(gm.thisPlayerSelectedTeam == 0)
                {
                    teamBasePos = gm.teamBasePosBlue;
                    networkTeams.Value = 0;
                    teamBasePos = gm.teamBasePosBlue;
                    //ChangeTeamServerRpc(0);
                }
                else if (gm.thisPlayerSelectedTeam == 1)
                {
                    teamBasePos = gm.teamBasePosRed;
                    networkTeams.Value = 1;
                    teamBasePos = gm.teamBasePosRed;
                    //ChangeTeamServerRpc(1);
                }
            }
        }

    }

    private void Start()
    {
        switch(type)
        {
            case CombatType.Melee:
                currentAttackRange = meleeAttackRange; break;
            case CombatType.Ranged:
                currentAttackRange = rangedAttackRange; break;
        }
        if(IsOwner)
        {
            transform.position = teamBasePos;
        }

    }

    [ServerRpc(RequireOwnership = true)]
    private void ChangeTeamServerRpc(int team)
    {
        Debug.Log("ID: " + thisID);

        Vector3 offset = new Vector3(1, 0, 0);

        if (team == 0)
        {
            this.team = Teams.Blue;
        }
        else if (team == 1)
        {
            this.team = Teams.Red;
        }

        if (teamBasePos != Vector3.zero)
        {
            transform.position = teamBasePos;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateTeamsServerRpc()
    {
        UpdateTeamsClientRpc();
    }

    [ClientRpc]
    void UpdateTeamsClientRpc()
    {
        if(networkTeams.Value == 0) { team = Teams.Blue; }
        if(networkTeams.Value == 1) { team = Teams.Red; }
    }

    void Update()
    {
        if(!IsOwner) { return; }
        UpdateTeamsServerRpc();
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

    public void RespawnAtBase()
    {
        if (IsOwner)
        {
            transform.position = teamBasePos;
            NotifyUISpawnTime?.Invoke(playerReSpawnTime, isPlayerDead);
            playerReSpawnTime += 3;            
        }
    }
}
