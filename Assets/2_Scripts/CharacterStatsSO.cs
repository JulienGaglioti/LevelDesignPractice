using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterStatsSO : ScriptableObject
{
    [Header("LAYERS")]
    [Tooltip("Set this to the layer your character is on")]
    public LayerMask CharacterLayer;
    [Tooltip("Set this to the layer of the ground")]
    public LayerMask GroundLayer;

    [Header("MOVEMENT")]
    [Tooltip("The top horizontal movement speed")]
    public float MaxSpeed = 14;

    [Tooltip("The character's capacity to gain horizontal speed")]
    public float Acceleration = 120;

    [Tooltip("A constant downward force applied while grounded"), Range(0f, -10f)]
    public float GroundingForce = -1.5f;
    [Tooltip("How fast the character turns to face movement direction"), Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    /* [Tooltip("How much you can control movement while in air (fraction of Max Speed)"), Range(0.0f, 1f)]
    public float InAirManeuverability = 0.6f; */
    /* [Tooltip("Max slope angle in degrees"), Range(0.0f, 90f)]
    public float SlopeLimit = 45f;
    [Tooltip("Force applied when on a slope"), Range(0.0f, 30f)]
    public float SlopeFallPower = 10f; */

    [Header("JUMP")]
    [Tooltip("The immediate velocity applied when jumping")]
    public float JumpPower = 36;

    [Tooltip("The maximum vertical movement speed")]
    public float MaxFallSpeed = 40;

    [Tooltip("The player's capacity to gain fall speed. a.k.a. In Air Gravity")]
    public float FallAcceleration = 110;

    [Tooltip("The gravity multiplier added when jump is released early")]
    public float JumpEndEarlyGravityModifier = 3;

    [Tooltip("The time before coyote jump becomes unusable. Coyote jump allows jump to execute even after leaving a ledge")]
    public float CoyoteTime = 0.15f;

    [Tooltip("The amount of time we buffer a jump. This allows jump input before actually hitting the ground")]
    public float JumpBuffer = 0.2f;

    [Header("DASH")]
    [Tooltip("How fast the character moves during dash")]
    public float DashSpeed = 30;
    [Tooltip("Dash duration in seconds")]
    public float DashDuration = 0.2f;
    [Tooltip("Duration of dash slowdown: a small window that starts after the dash ends OR when too close to dash target. During this time, the dash velocity is decreased until it reaches zero")]
    public float DashSlowdownDuration = 1f;
}
