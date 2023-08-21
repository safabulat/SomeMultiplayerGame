using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerTrialController : NetworkBehaviour
{
    [SerializeField] TMP_Text m_Text;

    [SerializeField] GameObject bulletPrefab;
    private GameObject target = null; // Store the target as a GameObject

    private NetworkVariable<Vector3> targetVariable = new NetworkVariable<Vector3>(Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    public int zurrna = -1;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (IsOwner)
        {
            if(gm != null)
            {
                if(gm.thisPlayerSelectedTeam== 0)
                {
                    transform.position = new Vector3(1, 0, 1);
                    zurrna = gm.thisPlayerSelectedTeam;
                }
                else if(gm.thisPlayerSelectedTeam == 1)
                {
                    transform.position = new Vector3(5, 0, -2);
                    zurrna = gm.thisPlayerSelectedTeam;
                }
                Debug.Log(OwnerClientId + " is: " + zurrna + " and spawned at: " + transform.position);
            }
            targetVariable.Value = new Vector3( Random.Range(-5,5), Random.Range(-5, 5), Random.Range(-5, 5));
        }
    }

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if(!IsOwner) { return; }
        PlayerMovementController();
        PlayerAttackController();
        PlayerActionController();
    }

    void PlayerMovementController()
    {
        Vector3 inputs = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) { inputs.z += .3f; }
        if (Input.GetKey(KeyCode.A)) { inputs.x -= .3f; }
        if (Input.GetKey(KeyCode.S)) { inputs.z -= .3f; }
        if (Input.GetKey(KeyCode.D)) { inputs.x += .3f; }

        transform.position += inputs;
    }

    void PlayerAttackController()
    {

    }

    void PlayerActionController()
    {       
        if (Input.GetKey(KeyCode.R))
        {
            targetVariable.Value = new Vector3(Random.Range(-5, 5), Random.Range(-5, 5), Random.Range(-5, 5));
        }
        UpdateOnServerRpc();
    }

    [ServerRpc(RequireOwnership = true)]
    void UpdateOnServerRpc()
    {
        UpdateOnClientRpc();
    }

    [ClientRpc]
    void UpdateOnClientRpc()
    {
        m_Text.text = targetVariable.Value.ToString();
    }

}
