using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerTeamSelection : MonoBehaviour
{
    [SerializeField] private Button blueTeam, redTeam;
    [SerializeField] private GameObject MultiplayerUI;
    private void Awake()
    {
        blueTeam.onClick.AddListener(() =>
        {
            Debug.Log("Team: Blue");
            GameManager.Instance.thisPlayerSelectedTeam = 0;
            Hide();
        });
        redTeam.onClick.AddListener(() =>
        {
            Debug.Log("Team: Red");
            GameManager.Instance.thisPlayerSelectedTeam = 1;
            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        MultiplayerUI.SetActive(true);
    }
}
