using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Fusion;
using TMPro;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public event Action OnGameIsOver;

    public static bool MatchIsOver
    { get; private set; }
    [SerializeField] private Camera cam;

    [Networked] private TickTimer matchTimer { get; set; }
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float matchTimerAmount = 120;

    private void Awake()
    {
        if (GlobalManagers.Instance != null)
        {
            GlobalManagers.Instance.gameManager = this;
        }
    }

    public override void Spawned()
    {
        // Reset this var
        MatchIsOver = false;

        cam.gameObject.SetActive(false);

        matchTimer = TickTimer.CreateFromSeconds(Runner, matchTimerAmount);
    }

    public override void FixedUpdateNetwork()
    {
        if (matchTimer.Expired(Runner) == false && matchTimer.RemainingTime(Runner).HasValue)
        {
            var timeSpan = TimeSpan.FromSeconds(matchTimer.RemainingTime(Runner).Value);
            var output = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            timerText.text = output;
        }
        else if (matchTimer.Expired(Runner))
        {
            MatchIsOver = true;
            matchTimer = TickTimer.None;
            OnGameIsOver?.Invoke();
            Debug.Log("Match timer has ended");
        }
    }
}
