using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float lifeTimeAmount = 0.8f;

    [Networked] private NetworkBool didHitSomething { get; set; }
    [Networked] private TickTimer lifeTimeTimer { get; set; }
    private Collider2D coll;

    public override void Spawned()
    {
        coll = GetComponent<Collider2D>();
        lifeTimeTimer = TickTimer.CreateFromSeconds(Runner, lifeTimeAmount);
    }

    public override void FixedUpdateNetwork()
    {
        CheckIfHitGround();

        if (lifeTimeTimer.ExpiredOrNotRunning(Runner) == false && !didHitSomething)
        {
            transform.Translate(transform.right * moveSpeed * Runner.DeltaTime, Space.World);
        }

        if (lifeTimeTimer.Expired(Runner) || didHitSomething)
        {
            Runner.Despawn(Object);
        }
    }

    private void CheckIfHitGround()
    {
        var groundCollider = Runner.GetPhysicsScene2D().OverlapBox(transform.position, coll.bounds.size, 0, groundLayerMask);

        if (groundCollider != default)
        {
            didHitSomething = true;
        }
    }


}
