using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartWaitUI : MonoBehaviour
{
    [SerializeField] GameObject WaitingForOtherPlayersBackground, GameStartCountDown;
    private void Update()
    {
        if (GameManager.Instance.IsWaitingToStart())
        {
            WaitingForOtherPlayersBackground.SetActive(true);
        }
        if (GameManager.Instance.IsCountdownToStartActive())
        {
            WaitingForOtherPlayersBackground.SetActive(false);
            GameStartCountDown.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
