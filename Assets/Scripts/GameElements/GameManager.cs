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
    
    private void Start()
    {
        InvokeRepeating(nameof(LimitedUpdate), 0f, .1f);
    }

    void LimitedUpdate()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (activePlayerCount == maxPlayerCount)
        {
            state = GameState.Started;
            
        }
        else
        {
            state = GameState.Ready;
        }
        if (state == GameState.Started)
        {
            CancelInvoke(nameof(LimitedUpdate));
            StartCoroutine(GameStartCountDown());
        }        
    }

    //private void OnApplicationFocus(bool focus)
    //{
    //    if(focus)
    //    {
    //        Cursor.lockState = CursorLockMode.Confined;
    //        Cursor.visible = true;
    //    }
    //    else
    //    {
    //        Cursor.lockState = CursorLockMode.None;
    //        Cursor.visible = true;
    //    }
    //}

    public void AddActivePlayer()
    {
        activePlayerCount++;
    }

    IEnumerator GameStartCountDown()
    {
        yield return new WaitForSeconds(3);
        GameStart?.Invoke();
    }

}
