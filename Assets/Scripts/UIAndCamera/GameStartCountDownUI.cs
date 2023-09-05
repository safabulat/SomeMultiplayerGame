using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStartCountDownUI : MonoBehaviour
{
    [SerializeField] TMP_Text countDownText;
    [SerializeField] GameObject stopper, stopper2;
    void Update()
    {
        if (GameManager.Instance.IsCountdownToStartActive())
        {
            countDownText.text = GameManager.Instance.GetCountdownToStartTimer().ToString("F0");
            if(GameManager.Instance.GetCountdownToStartTimer() <= 0)
            {
                Hide();
            }
        }
        if(GameManager.Instance.IsGameStarted())
        {
            Hide();
        }
    }

    void Hide()
    {
        gameObject.SetActive(false);
        stopper.SetActive(false);
        stopper2.SetActive(false);
        Destroy(stopper);
        Destroy(stopper2);
        Destroy(gameObject);
    }
}
