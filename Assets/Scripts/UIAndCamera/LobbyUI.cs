using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] Button createGameBtn, joinGameBtn;

    private void Awake()
    {
        createGameBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("CharacterSelectScene", LoadSceneMode.Single);
        });
        joinGameBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.StartClient();
        });
    }
}
