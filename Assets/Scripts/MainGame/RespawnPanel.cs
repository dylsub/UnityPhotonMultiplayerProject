using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public class RespawnPanel : SimulationBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TextMeshProUGUI respawnAmountText;
    [SerializeField] private GameObject childObj;

    public override void FixedUpdateNetwork()
    {
        if (Runner.LocalPlayer == playerController.Object.InputAuthority)
        {
            var timerIsRunning = playerController.RespawnTimer.IsRunning;
            childObj.SetActive(timerIsRunning);

            if (timerIsRunning && playerController.RespawnTimer.RemainingTime(Runner).HasValue)
            {
                var time = playerController.RespawnTimer.RemainingTime(Runner).Value;
                var roundInt = Mathf.RoundToInt(time);

                respawnAmountText.text = roundInt.ToString();
            }
        }
    }
}
