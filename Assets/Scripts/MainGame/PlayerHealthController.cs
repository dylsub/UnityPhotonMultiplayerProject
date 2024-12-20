using System.Collections;
using System.Collections.Generic;
using System.Resources;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthController : NetworkBehaviour
{
    [SerializeField] private PlayerCameraController playerCameraController;
    [SerializeField] private Image fillAmountImg;
    [SerializeField] private TextMeshProUGUI healthAmountText;
    [SerializeField] private Animator bloodScreenHitAnimator;

    [Networked(OnChanged = nameof(HealthAmountChanged))] private int currentHealthAmount { get; set; }

    private const int MAX_HEALTH_AMOUNT = 100;

    public override void Spawned()
    {
        currentHealthAmount = MAX_HEALTH_AMOUNT;
    }

    private static void HealthAmountChanged(Changed<PlayerHealthController> changed)
    {
        var currentHealth = changed.Behaviour.currentHealthAmount;

        changed.LoadOld();
        var oldHealth = changed.Behaviour.currentHealthAmount;

        // Only if the current health is not the same as the prev one
        if (currentHealth != oldHealth)
        {
            changed.Behaviour.UpdateVisuals(currentHealth);

            // We did not respawn or just spawned
            if (currentHealth != MAX_HEALTH_AMOUNT)
            {
                changed.Behaviour.PlayerGotHit(currentHealth);
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void Rpc_ReducePlayerHealth(int damage)
    {
        currentHealthAmount -= damage;
    }

    private void UpdateVisuals(int healthAmount)
    {
        var num = (float)healthAmount / MAX_HEALTH_AMOUNT;
        fillAmountImg.fillAmount = num;
        healthAmountText.text = $"{healthAmount}/{MAX_HEALTH_AMOUNT}";
    }

    private void PlayerGotHit(int healthAmount)
    {
        var isLocalPlayer = Runner.LocalPlayer == Object.HasInputAuthority;
        if (isLocalPlayer)
        {
            // play animation, screen shake
            const string BLOOD_HIT_CLIP_NAME = "BloodScreenHit";
            bloodScreenHitAnimator.Play(BLOOD_HIT_CLIP_NAME);
            Debug.Log("Local player got hit");

            var shakeAmount = new Vector3(0.1f, -0.3f, 0f);
            playerCameraController.ShakeCamera(shakeAmount);
        }

        if (healthAmount <= 0)
        {
            // Todo kill the player
            Debug.Log("Player is dead");
        }
    }


}
