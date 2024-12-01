using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerController : NetworkBehaviour, IBeforeUpdate
{
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float jumpForce = 1000f;

    [Networked] private NetworkButtons buttonsPrev { get; set; }
    private float horizontal;
    private Rigidbody2D rigid;


    public enum PlayerInputButtons
    {
        None,
        Jump
    }

    public override void Spawned()
    {
        rigid = GetComponent<Rigidbody2D>();
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
        return data;
    }

}
