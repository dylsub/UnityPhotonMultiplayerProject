using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private Button returnToLobbyBtn;
    [SerializeField] private GameObject childObj;

    private void Start()
    {
        GlobalManagers.Instance.gameManager.OnGameIsOver += OnMatchIsOver;
        returnToLobbyBtn.onClick.AddListener(() => GlobalManagers.Instance.networkRunnerController.ShutDownRunner());
    }

    private void OnMatchIsOver()
    {
        childObj.SetActive(true);
    }

    private void OnDestroy()
    {
        GlobalManagers.Instance.gameManager.OnGameIsOver -= OnMatchIsOver;
    }
}
