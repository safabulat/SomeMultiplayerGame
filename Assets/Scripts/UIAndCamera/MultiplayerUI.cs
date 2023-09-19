using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerUI : MonoBehaviour
{
    [SerializeField] private Button startHost, startClient;
    [SerializeField] private GameObject BackgroundIMG, GameStartWaitObj;

    private void Awake()
    {
        startHost.onClick.AddListener(() =>
        {
            Debug.Log("Host");
            GameManager.Instance.StartHost();
            Hide();
        });
        startClient.onClick.AddListener(() =>
        {
            Debug.Log("Client");
            GameManager.Instance.StartClient();
            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        BackgroundIMG.SetActive(false);
        GameStartWaitObj.SetActive(true);
    }
}
