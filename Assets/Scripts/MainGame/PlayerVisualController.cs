using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform pivotGunTr;
    [SerializeField] private Transform canvasTr;

    private readonly int isMovingHash = Animator.StringToHash("isWalking");
    private readonly int isShootingHash = Animator.StringToHash("isShooting");
    private bool isFacingRight = true;
    private bool init = false;

    private Vector3 originalPlayerScale;
    private Vector3 originalGunPivotScale;
    private Vector3 originalCanvasScale;

    private void Start()
    {
        originalPlayerScale = this.transform.localScale;
        originalGunPivotScale = pivotGunTr.transform.localScale;
        originalCanvasScale = canvasTr.transform.localScale;

        const int SHOOTING_LAYER_INDEX = 1;
        animator.SetLayerWeight(SHOOTING_LAYER_INDEX, 1);

        init = true;
    }

    public void TriggerDieAnimation()
    {
        const string TRIGGER = "Die";
        animator.SetTrigger(TRIGGER);
    }

    public void TriggerRespawnAnimation()
    {
        const string TRIGGER = "Respawn";
        animator.SetTrigger(TRIGGER);
    }

    // Called after simulations
    public void RendererVisuals(Vector2 velocity, bool isShooting)
    {
        if (!init) return;

        var isMoving = velocity.x > 0.1f || velocity.x < -0.1f;
        animator.SetBool(isMovingHash, isMoving);
        animator.SetBool(isShootingHash, isShooting);
    }

    // Called before simulations
    public void UpdateScaleTransforms(Vector2 velocity)
    {
        if (!init) return;

        if (velocity.x > 0.1f)
        {
            isFacingRight = true;
        }
        else if (velocity.x < -0.1f)
        {
            isFacingRight = false;
        }

        SetObjectLocalScaleBasedOnDir(gameObject, originalPlayerScale);
        SetObjectLocalScaleBasedOnDir(pivotGunTr.gameObject, originalGunPivotScale);
        SetObjectLocalScaleBasedOnDir(canvasTr.gameObject, originalCanvasScale);
    }

    private void SetObjectLocalScaleBasedOnDir(GameObject obj, Vector3 originalScale)
    {
        var zValue = originalScale.z;
        var yValue = originalScale.y;
        var xValue = isFacingRight ? originalScale.x : -originalScale.x;
        obj.transform.localScale = new Vector3(xValue, yValue, zValue);
    }
}
