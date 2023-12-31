using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Health : NetworkBehaviour
{
    public float startHealth;

    public NetworkVariable<float> player_maxHealth = new NetworkVariable<float>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
        );
    public NetworkVariable<float> player_currentHealth = new NetworkVariable<float>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
        );
    public HealthBar healthBar;

    //Damage Animation
    public float blinkIntensity;
    public float blinkDuration;
    private float blinkTimer;
    SkinnedMeshRenderer skinnedMesh;
    MeshRenderer meshRenderer;

    public bool isGameStarted = false;
    public float decreaseHealthRequest = 0f;
    public bool isDecreaseHealthCalled = false;

    public bool isPlayerDead = false;

    private void Start()
    {
        if (gameObject.tag != "Player")
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        if (IsOwner)
        {
            GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        }
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (!GameManager.Instance.IsGameStarted())
        {
            Debug.Log("Game aint started, no dmg");
            decreaseHealthRequest = 0f;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsOwner)
        {
            if (gameObject.tag == "Player")
            {
                skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
            }
            SetHealthEtc();
        }
    }

    private void FixedUpdate()
    {
        if(!IsOwner)
        {
            return;
        }

        if (!isGameStarted)
        {
            if (GameManager.Instance.IsGameStarted())
            {
                isGameStarted= true;
                SetHealthEtc();
            }
            else
            {
                return;
            }
        }

        if (decreaseHealthRequest > 0f && !isDecreaseHealthCalled)
        {
            Debug.Log("Should call decreaseHp");
            decreaseHealth(decreaseHealthRequest);
        }

        SynchHealthBarServerRpc();

        if (player_currentHealth.Value <= 0 && gameObject.tag == "Minion")
        {
            DestroyNetworkObject(NetworkObject);
        }
        if (player_currentHealth.Value <= 0 && gameObject.tag == "Tower")
        {
            Debug.Log("TowerDead, Should call destroyer");
            DestroyNetworkObject(NetworkObject);
        }
        if (player_currentHealth.Value <= 0 && gameObject.tag == "Base")
        {
            Debug.Log("BaseDead, Should call destroyer");
            Time.timeScale = 0f;
        }
        if (player_currentHealth.Value <= 0 && gameObject.tag == "Player" && !isPlayerDead)
        {
            Debug.Log("Player Dead, Should disable");
            isPlayerDead = true;
            StartCoroutine(PlayerReSpawnTimer());
        }
        if(gameObject.tag == "Player" && skinnedMesh != null)
        {
            SyncDamageAnimationServerRpc(true);
        }
        if(meshRenderer!= null)
        {
            SyncDamageAnimationServerRpc(false);
        }
    }

    private void SetHealthEtc()
    {
        player_maxHealth.Value = startHealth;
        player_currentHealth.Value = player_maxHealth.Value;
        healthBar.SetMaxHealth(player_maxHealth.Value);
        healthBar.SetHealth(player_currentHealth.Value);
    }


    [ServerRpc(RequireOwnership = false)]
    private void SynchHealthBarServerRpc()
    {
        SynchHealthBarClientRpc();
    }

    [ClientRpc]
    private void SynchHealthBarClientRpc()
    {
        healthBar.SetMaxHealth(player_maxHealth.Value);
        healthBar.SetHealth(player_currentHealth.Value);
    }

    [ServerRpc(RequireOwnership =false)]
    private void SyncDamageAnimationServerRpc(bool whichAnim)
    {
        SyncDamageAnimationClientRpc(whichAnim);
    }

    [ClientRpc]
    private void SyncDamageAnimationClientRpc(bool whichAnim)
    {
        if (whichAnim)
        {
            DamagedAnimationSkinnedMeshRenderer();
        }
        else
        {
            DamagedAnimationMeshRenderer();
        }
    }


    private void DamagedAnimationSkinnedMeshRenderer()
    {
        if(skinnedMesh != null)
        {
            blinkTimer -= Time.deltaTime;
            float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
            float intersity = (lerp * blinkIntensity) + 1f;
            skinnedMesh.material.color = Color.white * intersity;
        }
    }

    private void DamagedAnimationMeshRenderer()
    {
        if(meshRenderer != null)
        {
            blinkTimer -= Time.deltaTime;
            float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
            float intersity = (lerp * blinkIntensity) + 1f;
            meshRenderer.material.color = Color.white * intersity;
        }
        else
        {
            Debug.Log("Mesh Renderer NULL on: " + gameObject.name);
        }
    }

    public void increaseHealth(float increaseBythis)
    {
        player_currentHealth.Value += increaseBythis;
        healthBar.SetHealth(player_currentHealth.Value);
    }

    public void decreaseHealth(float decreaseBythis)
    {
        if(!IsOwner) { Debug.Log(gameObject.name + " is not Owner"); return; }

        Debug.Log("Dmg Dealth from hp.sc");
        isDecreaseHealthCalled = true;

        player_currentHealth.Value -= decreaseBythis;
        healthBar.SetHealth(player_currentHealth.Value);

        blinkTimer = blinkDuration;

        decreaseHealthRequest = 0f;
        isDecreaseHealthCalled = false;
    }

    public void IncreaseMaxHealth(float increaseBythis)
    {
        player_maxHealth.Value += increaseBythis;
        healthBar.SetMaxHealth(player_maxHealth.Value);
        healthBar.SetHealth(player_currentHealth.Value);
    }

    public void SetMaxHealth(float setToThis)
    {
        player_maxHealth.Value = setToThis;
        healthBar.SetMaxHealth(player_maxHealth.Value);
        if (player_currentHealth.Value > player_maxHealth.Value)
        {
            player_currentHealth.Value = player_maxHealth.Value;
        }
        healthBar.SetHealth(player_currentHealth.Value);
    }

    public float GetMaxHealth()
    {
        return player_maxHealth.Value;
    }

    public float GetCurrentHealth()
    {
        return player_currentHealth.Value;
    }

    private void DestroyNetworkObject(NetworkObject networkObject)
    {
        DestroyObjectServerRpc(networkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyObjectServerRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        GameObject gameObjectToDestroy = networkObject.gameObject;
        Destroy(gameObjectToDestroy);
        Destroy(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void GetTakenDamageServerRpc(float dmg)
    {
        //Debug.Log("CalledSERV");
        BroadcastClientRpc(dmg);
    }

    [ClientRpc]
    public void BroadcastClientRpc(float dmg)
    {
        //Debug.Log("CalledCLI");
        decreaseHealthRequest = dmg;
    }

    IEnumerator PlayerReSpawnTimer()
    {
        PlayerCombatManager pcm = gameObject.GetComponent<PlayerCombatManager>();
        ThirdPersonController tpc = gameObject.GetComponent<ThirdPersonController>();

        pcm.isPlayerDead = true;
        pcm.DeadCounter++;
        tpc.enabled = false;

        yield return new WaitForSeconds(.1f);

        skinnedMesh.material.color = Color.white * 0;

        pcm.RespawnAtBase();
        yield return new WaitForSeconds(pcm.playerReSpawnTime);
        pcm.playerReSpawnTime += 3;
        SetHealthEtc();
        
        skinnedMesh.material.color = Color.white * 1;

        tpc.enabled = true;
        pcm.isPlayerDead = false;
        isPlayerDead = false;
    }
}
