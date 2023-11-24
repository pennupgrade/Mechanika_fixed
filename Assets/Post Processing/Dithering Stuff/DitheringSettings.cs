using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(DitheringRenderer), PostProcessEvent.AfterStack, "Custom/Dithering", true)]
public sealed class DitheringSettings : PostProcessEffectSettings
{
    [Range(0f, 1f)]
    public FloatParameter Interpolation = new() { value = .4f };
    public TextureParameter DitherTexture = new() { value = null };

    public ColorParameter UnderDitherColor = new() { value = Color.black };
    public Vector2Parameter TimeVariation = new() { value = Vector2.zero };

    public FloatParameter Scale = new() { value = 50f };

    public Vector2Parameter Speed = new() { value = Vector2.zero };

    public FloatParameter Offset = new() { value = 0f };
}

public sealed class DitheringRenderer : PostProcessEffectRenderer<DitheringSettings>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Dithering"));
        sheet.properties.SetFloat("_Interpolation", settings.Interpolation.value);
        sheet.properties.SetTexture("_DitherTexture", settings.DitherTexture.value);
        sheet.properties.SetVector("_UnderDitherColor", settings.UnderDitherColor.value);
        sheet.properties.SetVector("_TimeVariation", settings.TimeVariation.value);
        sheet.properties.SetFloat("_Scale", settings.Scale.value);
        sheet.properties.SetVector("_Speed", settings.Speed.value);
        sheet.properties.SetFloat("_Offset", settings.Offset.value);//TransitionVFXController.Ins.DitheringInterpolation); later make this access static
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}