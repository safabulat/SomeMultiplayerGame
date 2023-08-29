using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Chat : NetworkBehaviour
{
    [SerializeField] TMP_Text textPanel;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        textPanel.text = string.Empty;
        textPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        if (Input.GetKeyDown(KeyCode.T))
        {
            TypeTextServerRpc();
        }
    }

    [ServerRpc(RequireOwnership =false)]
    private void TypeTextServerRpc()
    {
        textPanel.gameObject.SetActive(true);
        SendTextClientRpc();
    }

    [ClientRpc]
    private void SendTextClientRpc()
    {
        textPanel.text = "3123441234";
        StartCoroutine(ClearTextTimer(5));
    }

    private IEnumerator ClearTextTimer(float time)
    {
        yield return new WaitForSeconds(time);
        textPanel.text = string.Empty;
        textPanel.gameObject.SetActive(false);
    }
}
