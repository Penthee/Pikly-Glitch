using UnityEngine;
using DG.Tweening;

public class WaveExplo : MonoBehaviour {
    public float startRadius = 0, endRadius = 0.6f, time = 0.95f, ampTime = 0.4f, amplitude = 0.05f, waveSize = 0.2f;
    public Ease ease = Ease.InOutExpo, ampEase = Ease.Linear;

	void Update () {
        if (Input.GetMouseButtonDown(0)) {
            Vector2 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            WaveExploPostProcessing.Get().StartIt(pos, startRadius, endRadius, time, ampTime, amplitude, ampEase, waveSize, ease);
        }
    }


}
