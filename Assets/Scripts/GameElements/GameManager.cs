using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    //public event Action GameStart, GamePaused;
    public int thisPlayerSelectedTeam = -1;
    public int activePlayerCount = 0;
    public int maxPlayerCount = 2;

    public Vector3 teamBasePosRed, teamBasePosBlue;

    public event EventHandler OnStateChanged;
    //add gameTimer and send event to UI

    public enum GameState
    {
        WaitingToStart,
        CountdownToStart,
        GameStarted,
        GameOver,
    }
    private NetworkVariable<GameState> state = new NetworkVariable<GameState>(GameState.WaitingToStart);
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(5f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += GameState_OnValueChanged;
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        switch (state.Value)
        {
            case GameState.WaitingToStart:
                if(NetworkManager.Singleton.ConnectedClients.Count == maxPlayerCount)
                {
                    state.Value = GameState.CountdownToStart;
                }
                break;
            case GameState.CountdownToStart:
                countdownToStartTimer.Value -= Time.deltaTime;
                if (countdownToStartTimer.Value < 0f)
                {
                    state.Value = GameState.GameStarted;
                }
                break;
            case GameState.GameStarted:
                gamePlayingTimer.Value += Time.deltaTime;
                break;
            case GameState.GameOver:
                break;
        }
    }

    private void GameState_OnValueChanged(GameState previousValue, GameState newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddActivePlayer()
    {
        activePlayerCount++;
    }

    public bool IsGameStarted()
    {
        return state.Value == GameState.GameStarted;
    }

    public bool IsCountdownToStartActive()
    {
        return state.Value == GameState.CountdownToStart;
    }

    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer.Value;
    }

    public bool IsGameOver()
    {
        return state.Value == GameState.GameOver;
    }

    public bool IsWaitingToStart()
    {
        return state.Value == GameState.WaitingToStart;
    }
}
