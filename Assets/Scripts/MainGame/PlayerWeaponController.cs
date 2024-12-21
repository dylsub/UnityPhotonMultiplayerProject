using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerWeaponController : NetworkBehaviour, IBeforeUpdate
{
    public Quaternion LocalQuaternionPivotRot { get; private set; }

    [SerializeField] private NetworkPrefabRef bulletPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private Transform firePointPos;
    [SerializeField] private ParticleSystem muzzleEffect;
    [SerializeField] private float delayBetweenShots = 0.18f;
    [SerializeField] private Camera localCam;
    [SerializeField] private Transform pivotToRotate;

    [Networked(OnChanged = nameof(OnMuzzleEffectStateChanged))] private NetworkBool playMuzzleEffect { get; set; }

    [Networked, HideInInspector] public NetworkBool IsHoldingShootingKey { get; private set; }

    [Networked] private Quaternion currentPlayerPivotRotation { get; set; }

    [Networked] private NetworkButtons buttonsPrev { get; set; }
    [Networked] private TickTimer shootCooldown { get; set; }

    private PlayerController playerController;

    public override void Spawned()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void BeforeUpdate()
    {
        if (Runner.LocalPlayer == Object.HasInputAuthority && playerController.AcceptAnyInput)
        {
            var direction = localCam.ScreenToWorldPoint(Input.mousePosition) - transform.position;

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            LocalQuaternionPivotRot = Quaternion.AngleAxis(angle, Vector3.forward);

            // Would work for local player
            // pivotToRotate.transform.rotation = quaternion;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out var input))
        {
            if (playerController.AcceptAnyInput)
            {
                CheckShootingInput(input);
                currentPlayerPivotRotation = input.GunPivotRotation;

                buttonsPrev = input.NetworkButtons;
            }
            else
            {
                IsHoldingShootingKey = false;
                playMuzzleEffect = false;
                buttonsPrev = default;
            }
        }

        pivotToRotate.rotation = currentPlayerPivotRotation;
    }

    private void CheckShootingInput(PlayerData input)
    {
        var currentBtns = input.NetworkButtons.GetPressed(buttonsPrev);

        IsHoldingShootingKey = currentBtns.WasReleased(buttonsPrev, PlayerController.PlayerInputButtons.Shoot);

        if (currentBtns.WasReleased(buttonsPrev, PlayerController.PlayerInputButtons.Shoot) && shootCooldown.ExpiredOrNotRunning(Runner))
        {
            playMuzzleEffect = true;
            shootCooldown = TickTimer.CreateFromSeconds(Runner, delayBetweenShots);

            // Instantiate the bullet prefab
            Runner.Spawn(bulletPrefab, firePointPos.position, firePointPos.rotation, Object.InputAuthority);
        }
        else
        {
            playMuzzleEffect = false;
        }
    }

    private static void OnMuzzleEffectStateChanged(Changed<PlayerWeaponController> changed)
    {
        var currentState = changed.Behaviour.playMuzzleEffect;
        changed.LoadOld();
        var oldState = changed.Behaviour.playMuzzleEffect;

        if (oldState != currentState)
        {

            changed.Behaviour.PlayOrStopMuzzleEffect(currentState);
        }
    }

    private void PlayOrStopMuzzleEffect(bool play)
    {
        if (play)
        {
            muzzleEffect.Play();
        }
        else
        {
            muzzleEffect.Stop();
        }
    }
}
