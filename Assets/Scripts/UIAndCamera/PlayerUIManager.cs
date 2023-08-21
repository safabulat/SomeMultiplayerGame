using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] GameObject OnDeadUI;
    [SerializeField] TMP_Text OnDeadUIText;

    PlayerCombatManager PlayerCombatManager = null;
    private bool isListening = false;

    private void Start()
    {
        
    }

    private void Update()
    {
        PlayerCombatManager = GameObject.FindAnyObjectByType<PlayerCombatManager>();
        if(PlayerCombatManager!= null && !isListening) { PlayerCombatManager.NotifyUISpawnTime.AddListener(PCM_Notify); isListening = true; }
    }

    void PCM_Notify(float time, bool isDead)
    {
        if (isDead)
        {
            OnDeadUI.SetActive(true);
        }
        else
        {
            OnDeadUI.SetActive(false);
        }

        if(isDead) { OnDeadUIText.text = "SpawningIn: " + time.ToString(); }
    }
}
