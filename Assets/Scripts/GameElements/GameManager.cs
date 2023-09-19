using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
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

    [SerializeField] private Transform playerPrefab;

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

    private Dictionary<ulong, bool> playerReadyDictionary;

    private void Awake()
    {
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += GameState_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach(ulong client in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(client, true);
        }
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

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (state.Value == GameState.WaitingToStart)
        {
            connectionApprovalResponse.Approved = true;
            connectionApprovalResponse.CreatePlayerObject = true;
        }
        else
        {
            connectionApprovalResponse.Approved = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                // This player is NOT ready
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            state.Value = GameState.CountdownToStart;
        }
    }
}
