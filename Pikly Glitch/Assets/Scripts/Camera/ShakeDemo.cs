using UnityEngine;
//using Pikl.UI;
using System.Collections.Generic;

namespace Pikl.Utils.Shaker {
    public class ShakeDemo : MonoBehaviour {

        delegate float Slider(float val, string prefix, float min, float max, int pad);

        Shaker.CameraShakeInstance shake;
        List<Shaker.CameraShakeInstance> presets;
        Vector2 presetScrollPos;

        Vector3 posInf = new Vector3(0.25f, 0.25f, 0f);
        float magn = 1, rough = 1, fadeIn = 0.1f, fadeOut = 2f, rotInf = 1, scaleInf = 0.1f;
        int instanceCount = 1;
        bool showList = true;
        Slider slider;

        public bool show;

        void Start() {
            presets = new List<Shaker.CameraShakeInstance>(ShakePresets.AllAsCameraShake);
        }

        void OnGUI() {
            if (show) {
                slider = delegate (float val, string prefix, float min, float max, int pad) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(prefix, GUILayout.MaxWidth(pad));
                    val = GUILayout.HorizontalSlider(val, min, max, GUILayout.MinWidth((pad + 50) - 230));
                    GUILayout.Label(val.ToString("F2"), GUILayout.MaxWidth(50));
                    GUILayout.EndHorizontal();
                    return val;
                };

                DrawMakeAShake();
                DrawActiveShakes();
            }
        }

        void DrawMakeAShake() {
            GUI.Box(new Rect(10, 10, 250, 650), "-- Make A Shake --");
            GUILayout.BeginArea(new Rect(20, 40, 230, 610));

            GUILayout.Label("-- Position Infleunce --");
            posInf.x = slider(posInf.x, "X", 0, 4, 25);
            posInf.y = slider(posInf.y, "Y", 0, 4, 25);

            GUILayout.Label("-- Rotation Infleunce --");
            rotInf = slider(rotInf, "", 0, 4, 0);

            GUILayout.Label("-- Scale Infleunce --");
            scaleInf = slider(scaleInf, "", 0, 4, 0);

            GUILayout.Label("-- Magnitude --");
            magn = slider(magn, "", 0, 5, 0);
            GUILayout.Label("-- Roughness --");
            rough = slider(rough, "", 0.01f, 10, 0);

            GUILayout.Label("-- Fade Time --");
            fadeIn = slider(fadeIn, "  In:", 0, 10, 25);
            fadeOut = slider(fadeOut, "Out:", 0, 10, 25);

            if (shake == null && GUILayout.Button("Create Shake Instance")) {
                shake = new Shaker.CameraShakeInstance(new Shake(magn, rough, fadeIn, fadeOut, posInf, rotInf, scaleInf));
                Shaker.I.StartCameraShake(shake);
                shake.s.name = "Make-A-Shake Instance # " + instanceCount++;
            }

            if (shake != null) {
                if (GUILayout.Button("Delete Shake Instance")) {
                    Shaker.I.StopShake(shake.s);
                    shake = null;
                } else {
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Fade Out"))
                        Shaker.I.StopShake(shake.s);

                    if (GUILayout.Button("Fade In"))
                        shake = Shaker.I.StartCameraShake(shake);

                    shake.s.loop = GUILayout.Toggle(shake.s.loop, "Loop");

                    shake.s.magnitude = magn;
                    shake.s.roughness = rough;
                    shake.s.fadeInTime = fadeIn;
                    shake.s.fadeOutTime = fadeOut;
                    shake.s.positionInfluence = posInf;
                    shake.s.rotationInfluence = rotInf;
                    shake.s.scaleInfluence = scaleInf;

                    GUILayout.EndHorizontal();
                }
            }

            if (GUILayout.Button("Shake Once")) {
                Shake s = new Shake(magn, rough, fadeIn, fadeOut, posInf, rotInf, scaleInf);
                Shaker.I.ShakeCameraOnce(s);
                s.name = "Make-A-Shake Instance # " + instanceCount++;
            }

            GUILayout.Label("-- Preset Shake Instances --");
            presetScrollPos = GUILayout.BeginScrollView(presetScrollPos);
            for (int i = 0; i < presets.Count; i ++) {
                string btnTxt = (!Shaker.I.ActiveShakes.Contains(presets[i]) | !presets[i].s.loop ? "Add " : "Remove ") + presets[i].s.name;
                if (GUILayout.Button(btnTxt)) {
                    if (!Shaker.I.ActiveShakes.Contains(presets[i]) || !presets[i].s.loop) {
                        Debug.Log("starting " + presets[i].s.name);
                        if (presets[i].s.loop)
                            Shaker.I.StartCameraShake(presets[i]);
                        else
                            Shaker.I.ShakeCameraOnce(new Shake(presets[i].s));
                    } else {
                        Debug.Log("stopping " + presets[i].s.name);
                        Shaker.I.StopShake(presets[i]);
                    }
                }
            }
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        void DrawActiveShakes() {
            float height = 100f + ((!showList) ? 0 : Shaker.I.ActiveShakes.Count * 120f);

            GUI.Box(new Rect(Screen.width - 310, 10, 300, height), "-- Shake Instance List --");
            GUILayout.BeginArea(new Rect(Screen.width - 300, 40, 280, height));

            GUILayout.Label("All shake instances are saved and queued as long as they are active.");

            showList = GUILayout.Toggle(showList, "Show List");

            if (showList) {
                foreach (Shaker.ShakeInstance si in Shaker.I.ActiveShakes) {
                    string state = si.s.CurrentState.ToString();
                    GUILayout.Label("- - " + si.s.name + " - -");
                    GUILayout.Label(string.Concat("Magnitude: ", si.s.magnitude.ToString("F2"), " Roughness: ", si.s.roughness.ToString("F2")));
                    GUILayout.Label("Pos Infl: " + si.s.positionInfluence + " | Rot Infl: " + si.s.rotationInfluence + " | Scale Infl: " + si.s.scaleInfluence);
                    GUILayout.Label("State: " + state + " | Loop: " + si.s.loop + " | "  + si.s.FadeMagnitude);
                    GUILayout.Label("- - - - - - - - - - - - - - - - - - - - - - -");
                }
            }
            GUILayout.EndArea();
        }
    }
}