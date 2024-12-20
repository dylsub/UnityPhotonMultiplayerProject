using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerController : NetworkBehaviour, IBeforeUpdate
{
    [SerializeField] private GameObject cam;
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float jumpForce = 1000f;
    [SerializeField] private TextMeshProUGUI playerNameText;

    [Networked(OnChanged = nameof(OnNicknameChanged))] private NetworkString<_8> playerName { get; set; }
    [Networked] private NetworkButtons buttonsPrev { get; set; }

    private float horizontal;
    private Rigidbody2D rigid;
    private PlayerWeaponController playerWeaponController;
    private PlayerVisualController playerVisualController;


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
        SetLocalObjects();
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

    private void SetLocalObjects()
    {

        // Check if this player is controlled by the local client
        if (Runner.LocalPlayer == Object.HasInputAuthority)
        {
            // Enable the camera and ensure its AudioListener is active
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
        if (Runner.LocalPlayer == Object.HasInputAuthority)
        {
            const string HORIZONTAL = "Horizontal";
            horizontal = Input.GetAxisRaw(HORIZONTAL);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Will return false if:
        // The client does not have state authority or input authority
        // The requested type of input does not exist in the simulation
        if (Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out var input))
        {
            rigid.velocity = new Vector2(input.HorizontalInput * moveSpeed, rigid.velocity.y);
            CheckJumpInput(input);
        }
        playerVisualController.UpdateScaleTransforms(rigid.velocity);
    }

    // Renders after the FUN
    public override void Render()
    {
        playerVisualController.RendererVisuals(rigid.velocity, playerWeaponController.IsHoldingShootingKey);
    }

    private void CheckJumpInput(PlayerData input)
    {
        var pressed = input.NetworkButtons.GetPressed(buttonsPrev);
        if (pressed.WasPressed(buttonsPrev, PlayerInputButtons.Jump))
        {
            rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
        }

        buttonsPrev = input.NetworkButtons;

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

}
