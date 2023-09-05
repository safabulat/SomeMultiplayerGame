using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerUIManager : NetworkBehaviour
{
    [SerializeField] GameObject OwnerDeadUI, OwnerKADM, DeadBackGround;
    [SerializeField] TMP_Text OnDeadUIText, KDAText, MinionText;

    PlayerCombatManager PlayerCombatManager = null;

    private float deadTimer = 0f;
    private int _kill = 0, _dead = 0, _assist = 0, _minion = 0;
    private void Start()
    {
        PlayerCombatManager = GetComponentInParent<PlayerCombatManager>();
        if (PlayerCombatManager != null)
        {
            PlayerCombatManager.NotifyUISpawnTime.AddListener(PCM_NotifyDeadTimer);
            PlayerCombatManager.NotifyUIKills.AddListener(PCM_NotifyKills);
            PlayerCombatManager.NotifyUIDeads.AddListener(PCM_NotifyDeads);
            PlayerCombatManager.NotifyUIAssists.AddListener(PCM_NotifyAssists);
            PlayerCombatManager.NotifyUIMinion.AddListener(PCM_NotifyMinions);

            Debug.Log("UI listenning");
        }
        OwnerDeadUI.SetActive(false);
        OwnerKADM.SetActive(false);
        UpdateKDAText();
        UpdateMinionText();
    }

    private void Update()
    {
        if (!IsOwner)
        {
            OwnerDeadUI.SetActive(false);
            OwnerKADM.SetActive(false);
            return;
        }
        else
        {
            OwnerKADM.SetActive(true);
        }

        if(deadTimer > 0f)
        {
            deadTimer -= Time.deltaTime;
            OnDeadUIText.text = "SpawningIn: " + deadTimer.ToString("F2");
        }
        else
        {
            OwnerDeadUI.SetActive(false);
            DeadBackGround.SetActive(false);
        }
        
    }

    void UpdateMinionText()
    {
        MinionText.text = "M: " + _minion.ToString();
    }
    void UpdateKDAText()
    {
        
        KDAText.text = _kill.ToString() + " / " + _dead.ToString() + " / " + _assist.ToString();
    }

    void PCM_NotifyDeadTimer(float time, bool isDead)
    {
        if (!IsOwner) { Debug.Log("UI not owner: PCM_NotifyDeadTimer"); return; }
        if (isDead)
        {
            Debug.Log("Invoked: PCM_NotifyDeadTimer");
            OwnerDeadUI.SetActive(true);
            DeadBackGround.SetActive(true);
            OnDeadUIText.text = "SpawningIn: " + time.ToString("F2");
            deadTimer = time;
        }
    }

    void PCM_NotifyKills(int kills)
    {
        if (!IsOwner) { Debug.Log("UI not owner: PCM_NotifyKills"); return; }
        Debug.Log("Invoked: PCM_NotifyKills");
        _kill = kills;
        UpdateKDAText();
    }

    void PCM_NotifyDeads(int deads)
    {
        if (!IsOwner) { Debug.Log("UI not owner: PCM_NotifyDeads"); return; }
        Debug.Log("Invoked: PCM_NotifyDeads");
        _dead = deads;
        UpdateKDAText();
    }

    void PCM_NotifyAssists(int assist)
    {
        if (!IsOwner) { Debug.Log("UI not owner: PCM_NotifyAssists"); return; }
        Debug.Log("Invoked: PCM_NotifyAssists");
        _assist = assist;
        UpdateKDAText();
    }

    void PCM_NotifyMinions(int minions)
    {
        if (!IsOwner) { Debug.Log("UI not owner: PCM_NotifyMinions"); return; }
        Debug.Log("Invoked: PCM_NotifyMinions");
        _minion = minions;
        UpdateMinionText();
    }
}
