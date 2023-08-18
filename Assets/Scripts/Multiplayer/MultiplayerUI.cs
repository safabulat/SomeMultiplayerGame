using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerUI : MonoBehaviour
{
    [SerializeField] private Button startHost, startClient;
    [SerializeField] private GameObject PlayerUI, BackgroundIMG;

    private void Awake()
    {
        startHost.onClick.AddListener(() =>
        {
            Debug.Log("Host");
            NetworkManager.Singleton.StartHost();
            Hide();
        });
        startClient.onClick.AddListener(() =>
        {
            Debug.Log("Client");
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        BackgroundIMG.SetActive(false);
        PlayerUI.SetActive(true);
    }
}
