using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ZoomBlur : VolumeComponent, IPostProcessComponent
{
    [Range(0f, 100f)]
    public FloatParameter focusPower = new FloatParameter(0f);

    [Range(0, 10)]
    public IntParameter focusDetail = new IntParameter(5);
    
    public Vector2Parameter focusScreenPosition = new Vector2Parameter(Vector2.zero);
    public IntParameter referenceResolutionX = new IntParameter(1334);
    
    public bool IsActive()
    {
        return focusPower.value > 0f;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
