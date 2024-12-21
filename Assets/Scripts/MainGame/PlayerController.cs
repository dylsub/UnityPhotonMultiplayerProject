using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using Fusion;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : NetworkBehaviour, IBeforeUpdate
{
    public bool AcceptAnyInput => PlayerIsAlive && !GameManager.MatchIsOver;

    [SerializeField] private GameObject cam;
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float jumpForce = 1000f;
    [SerializeField] private TextMeshProUGUI playerNameText;

    [Header("Grounded Vars")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundDetectionObj;

    [Networked] public TickTimer RespawnTimer { get; private set; }

    [Networked(OnChanged = nameof(OnNicknameChanged))] private NetworkString<_8> playerName { get; set; }
    [Networked] private NetworkButtons buttonsPrev { get; set; }
    [Networked] public NetworkBool PlayerIsAlive { get; private set; }
    [Networked] private Vector2 serverNextSpawnPoint { get; set; }
    [Networked] private NetworkBool IsGrounded { get; set; }

    private float horizontal;
    private Rigidbody2D rigid;
    private PlayerWeaponController playerWeaponController;
    private PlayerVisualController playerVisualController;
    private PlayerHealthController playerHealthController;

    public enum PlayerInputButtons
    {
        None,
        Jump,
        Shoot
    }

    public override void Spawned()
    {
        rigid = GetComponent<Rigidbody2D>();
        playerWeaponController = GetComponent<PlayerWeaponController>();
        playerVisualController = GetComponent<PlayerVisualController>();
        playerHealthController = GetComponent<PlayerHealthController>();
        SetLocalObjects();
        PlayerIsAlive = true;
    }

    private void setPlayerNickname(NetworkString<_8> nickname)
    {
        playerNameText.text = nickname + " " + Object.InputAuthority.PlayerId;
    }

    private static void OnNicknameChanged(Changed<PlayerController> changed)
    {
        // Access newest nickname
        changed.LoadNew();
        var newNickname = changed.Behaviour.playerName;

        // Access previous nickname
        changed.LoadOld();
        var oldNickname = changed.Behaviour.playerName;

        changed.Behaviour.setPlayerNickname(newNickname);
    }

    public void KillPlayer()
    {
        if (Runner.IsServer)
        {
            serverNextSpawnPoint = GlobalManagers.Instance.playerSpawnerController.GetRandomSpawnPoint();
        }

        rigid.simulated = false;
        playerVisualController.TriggerDieAnimation();
        PlayerIsAlive = false;
        RespawnTimer = TickTimer.CreateFromSeconds(Runner, 3f);
    }

    private void SetLocalObjects()
    {

        // Check if this player is controlled by the local client
        if (Runner.LocalPlayer == Object.HasInputAuthority)
        {
            // Enable the camera and ensure its AudioListener is active
            cam.transform.SetParent(null);
            cam.SetActive(true);

            var nickname = GlobalManagers.Instance.networkRunnerController.LocalPlayerNickname;
            RpcSetNickname(nickname);
        }
        else
        {
            // Disable the camera for non-local players
            cam.SetActive(false);

            //Make sure that we are seeing proxies (other players) as snapshots not predicted
            GetComponent<NetworkRigidbody2D>().InterpolationDataSource = InterpolationDataSources.Snapshots;
        }
    }

    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RpcSetNickname(NetworkString<_8> nickname)
    {
        playerName = nickname;
    }

    // Happens before any network behaviour is done
    public void BeforeUpdate()
    {
        // We are the local machine
        if (Runner.LocalPlayer == Object.HasInputAuthority && AcceptAnyInput)
        {
            const string HORIZONTAL = "Horizontal";
            horizontal = Input.GetAxisRaw(HORIZONTAL);
        }
    }

    public override void FixedUpdateNetwork()
    {
        CheckRespawnTimer();

        // Will return false if:
        // The client does not have state authority or input authority
        // The requested type of input does not exist in the simulation
        if (Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out var input) && AcceptAnyInput)
        {
            rigid.velocity = new Vector2(input.HorizontalInput * moveSpeed, rigid.velocity.y);
            CheckJumpInput(input);
            buttonsPrev = input.NetworkButtons;
        }
        playerVisualController.UpdateScaleTransforms(rigid.velocity);
    }

    private void CheckRespawnTimer()
    {
        if (PlayerIsAlive) return;

        if (RespawnTimer.Expired(Runner))
        {
            RespawnTimer = TickTimer.None;
            RespawnPlayer();
        }
    }

    private void RespawnPlayer()
    {
        rigid.simulated = true;
        rigid.position = serverNextSpawnPoint;
        playerVisualController.TriggerRespawnAnimation();
        PlayerIsAlive = true;
        playerHealthController.ResetHealthAmountToMax();
    }

    // Renders after the FUN
    public override void Render()
    {
        playerVisualController.RendererVisuals(rigid.velocity, playerWeaponController.IsHoldingShootingKey);
    }

    private void CheckJumpInput(PlayerData input)
    {
        IsGrounded = (bool)Runner.GetPhysicsScene2D().OverlapBox(groundDetectionObj.transform.position, groundDetectionObj.transform.localScale, 0, groundLayer);

        if (IsGrounded)
        {
            var pressed = input.NetworkButtons.GetPressed(buttonsPrev);
            if (pressed.WasPressed(buttonsPrev, PlayerInputButtons.Jump))
            {
                rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
            }
        }
    }

    public PlayerData GetPlayedNetworkInput()
    {
        PlayerData data = new PlayerData();
        data.HorizontalInput = horizontal;
        data.NetworkButtons.Set(PlayerInputButtons.Jump, Input.GetKey(KeyCode.Space));
        data.NetworkButtons.Set(PlayerInputButtons.Shoot, Input.GetButton("Fire1"));
        data.GunPivotRotation = playerWeaponController.LocalQuaternionPivotRot;
        return data;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        GlobalManagers.Instance.objectPoolingManager.RemoveNetworkObjectFromDict(Object);
        Destroy(gameObject);
    }

}
