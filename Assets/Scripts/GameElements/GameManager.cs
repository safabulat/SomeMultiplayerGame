using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action GameStart, GamePaused;
    public int thisPlayerSelectedTeam = -1;
    public int activePlayerCount = 0;
    public int maxPlayerCount = 2;

    public Vector3 teamBasePosRed, teamBasePosBlue;

    public enum GameState
    {
        Ready,
        Started,
        Paused
    }
    public GameState state = GameState.Ready;
    
    private void Update()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if(activePlayerCount == maxPlayerCount)
        {
            state = GameState.Started;
            GameStart?.Invoke();
        }
        else
        {
            state = GameState.Ready;
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if(focus)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void AddActivePlayer()
    {
        activePlayerCount++;
    }

}
