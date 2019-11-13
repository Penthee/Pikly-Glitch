using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;

// You call it like this : WaveExploPostProcessing.Get().StartIt(myVector2Position);

public class WaveExploPostProcessing : MonoBehaviour {
    public Material mat;
    
    protected float _radius, _amplitude;

    public float radius {
        get { return _radius; }
        set {
            _radius = value;
            mat.SetFloat("_Radius", _radius);
        }
    }

    public float amplitude {
        get { return _amplitude; }
        set {
            _amplitude = value;
            mat.SetFloat("_Amplitude", _amplitude);
        }
    }

    public WaveExploPostProcessing() {
        mat = new Material(Shader.Find("Custom/WaveExplo"));
    }

    public void StartIt(Vector2 center, float startRadius, float endRadius, float time, float ampTime, float amplitude, Ease ampEase, float waveSize, Ease ease) {
        //mat.SetFloat("_CenterX", (center.x + Screen.width / 2) / Screen.width);
        //mat.SetFloat("_CenterY", (center.y + Screen.height / 2) / Screen.height);

        mat.SetFloat("_CenterX", center.x);
        mat.SetFloat("_CenterY", center.y);
        mat.SetFloat("_WaveSize", waveSize);

        radius = startRadius;
        this.amplitude = 0f;

        DOTween.To(() => radius, x => radius = x, endRadius, time)
               .SetEase(ease)
               .OnComplete(() => Destroy(this));

        DOTween.To(() => this.amplitude, x => this.amplitude = x, amplitude, ampTime).SetEase(ampEase);
               //.OnComplete(() => DOTween.To(() => this.amplitude, x => this.amplitude = x, 0, time / 2)).SetEase(ampEase);
    }

    static WaveExploPostProcessing _postProcessing;

    static public WaveExploPostProcessing Get() {
        WaveExploPostProcessing postProcessing = Camera.main.gameObject.AddComponent<WaveExploPostProcessing>();
        return postProcessing;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(src, dest, mat);
    }
}