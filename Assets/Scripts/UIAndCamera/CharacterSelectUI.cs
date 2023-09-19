using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] Button readyBtn;

    private void Awake()
    {
        readyBtn.onClick.AddListener(() =>
        {

        });
    }
}
