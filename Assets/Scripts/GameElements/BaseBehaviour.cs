using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class BaseBehaviour : NetworkBehaviour
{
    [SerializeField] GameObject minion;
    [SerializeField] Transform oppositeBase;
    
    public int team;

    public Vector3 spawnLocationOffset;

    public float spawnTime;
    public float waveTime;
    public int waveMinionCount;
    public float spawnInterval;

    private bool isSpawnerActive = false;

    GameManager gm = null;

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGameStarted())
        {
            if (!IsServer || !IsOwner) { return; }

            InvokeSpawnerServerRpc();
            isSpawnerActive = true;
            Debug.Log("SpawnerCalled");
        }
    }

    private void MinionWaveSpawn()
    {
        StartCoroutine(SpawnMinionWave());
    }

    private IEnumerator SpawnMinionWave()
    {
        for (int i = 0; i < waveMinionCount; i++)
        {
            MinionSpawnerServerRpc();
            yield return new WaitForSeconds(spawnInterval);
        }        
    }

    [ServerRpc]
    private void MinionSpawnerServerRpc()
    {
        Debug.Log("Minion Wave Spawned");
        GameObject _spawnedMinion = Instantiate(minion, transform.position + spawnLocationOffset, Quaternion.identity, gameObject.transform); //, 



        NetworkObject _sMNetworkObj = _spawnedMinion.GetComponent<NetworkObject>();
        MinionCombatManager minionCM = _spawnedMinion.GetComponent<MinionCombatManager>();
        minionCM.targetBase = oppositeBase.transform.position;
        _sMNetworkObj.Spawn(true);
    }


    [ServerRpc]
    private void InvokeSpawnerServerRpc()
    {
        InvokeRepeating(nameof(MinionWaveSpawn), spawnTime, waveTime);
    }
}
