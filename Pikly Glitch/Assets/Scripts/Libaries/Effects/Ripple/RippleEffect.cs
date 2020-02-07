using UnityEngine;
using System.Collections;

public class RippleEffect : MonoBehaviour {
    public AnimationCurve waveform = new AnimationCurve(
        new Keyframe(0.00f, 0.50f, 0, 0),
        new Keyframe(0.05f, 1.00f, 0, 0),
        new Keyframe(0.15f, 0.10f, 0, 0),
        new Keyframe(0.25f, 0.80f, 0, 0),
        new Keyframe(0.35f, 0.30f, 0, 0),
        new Keyframe(0.45f, 0.60f, 0, 0),
        new Keyframe(0.55f, 0.40f, 0, 0),
        new Keyframe(0.65f, 0.55f, 0, 0),
        new Keyframe(0.75f, 0.46f, 0, 0),
        new Keyframe(0.85f, 0.52f, 0, 0),
        new Keyframe(0.99f, 0.50f, 0, 0)
    );

    [Range(0.01f, 1.0f)]
    public float refractionStrength = 0.5f;

    public Color reflectionColor = Color.gray;

    [Range(0.01f, 1.0f)]
    public float reflectionStrength = 0.7f;

    [Range(1.0f, 3.0f)]
    public float waveSpeed = 1.25f;

    [Range(0.0f, 2.0f)]
    public float dropInterval = 0.5f;
    
    #pragma warning disable CS0649
    [SerializeField]
    Shader shader;

    class Droplet {
        Vector2 position;
        float time;

        public Droplet(Vector2 position) {
            this.position = position;
            time = 1000;
        }

        public void Reset() {
            time = 0;
        }

        public void Update() {
            time += Time.deltaTime;
        }

        public Vector4 MakeShaderParameter(float aspect) {
            return new Vector4(position.x * aspect, position.y, time, 0);
        }
    }

    Texture2D gradTexture;
    Material material;
    float timer;

    void Awake() {
        gradTexture = new Texture2D(2048, 1, TextureFormat.Alpha8, false);
        gradTexture.wrapMode = TextureWrapMode.Clamp;
        gradTexture.filterMode = FilterMode.Bilinear;
        for (var i = 0; i < gradTexture.width; i++) {
            var x = 1.0f / gradTexture.width * i;
            var a = waveform.Evaluate(x);
            gradTexture.SetPixel(i, 0, new Color(a, a, a, a));
        }
        gradTexture.Apply();

        material = new Material(shader);
        material.hideFlags = HideFlags.DontSave;
        material.SetTexture("_GradTex", gradTexture);
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector2 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            Droplet d = new Droplet(pos);

            StartCoroutine(UpdateDrop(d));
        }
    }

    IEnumerator UpdateDrop(Droplet d) {
        float timer = 0;

        d.Reset();

        while(timer < dropInterval) {
            timer += Time.deltaTime;

            material.SetVector("_Drop1", d.MakeShaderParameter(Camera.main.aspect));
            material.SetVector("_Drop2", d.MakeShaderParameter(Camera.main.aspect));
            material.SetVector("_Drop3", d.MakeShaderParameter(Camera.main.aspect));

            material.SetColor("_Reflection", reflectionColor);
            material.SetVector("_Params1", new Vector4(Camera.main.aspect, 1, 1 / waveSpeed, 0));
            material.SetVector("_Params2", new Vector4(1, 1 / Camera.main.aspect, refractionStrength, reflectionStrength));
            
            d.Update();

            yield return new WaitForEndOfFrame();
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(source, destination, material);
    }
}