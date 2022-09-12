using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UVOffset : VolumeComponent, IPostProcessComponent
{
    [Range(0f, 0.1f)]
    public ClampedFloatParameter offsetStrength = new ClampedFloatParameter(0f, 0.0f, 0.1f);

    public Vector2Parameter offsetDirection = new Vector2Parameter(Vector2.one);
    public bool IsActive()
    {
        return offsetStrength.value > 0f;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
