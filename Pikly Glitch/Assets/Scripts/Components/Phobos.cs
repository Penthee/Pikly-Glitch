using System;
using UnityEngine;
using UnityEngine.Animations;

namespace Pikl.Components {
    public class Phobos : MonoBehaviour {
        public float spinSpeed = 10;
        public Axis axis = Axis.Z;

        Transform _t;

        void Awake() {
            _t = transform;
        }
        
        void Update() {
            switch (axis) {
                case Axis.X: _t.Rotate(Vector3.right, spinSpeed * Time.deltaTime, Space.World); break;
                case Axis.Y: _t.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World); break;
                case Axis.Z: _t.Rotate(Vector3.forward, spinSpeed * Time.deltaTime, Space.World); break;
            }
        }
    }
}
