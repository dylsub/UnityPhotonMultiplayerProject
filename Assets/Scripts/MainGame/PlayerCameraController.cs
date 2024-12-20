using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulseSource;

    public void ShakeCamera(Vector3 shakeAmount)
    {
        impulseSource.GenerateImpulse(shakeAmount);
    }
}
