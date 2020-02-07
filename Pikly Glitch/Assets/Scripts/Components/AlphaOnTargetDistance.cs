using System;
using Pikl.Extensions;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

namespace Pikl.Components {
    public class AlphaOnTargetDistance : MonoBehaviour {
        [Range(2, 10)] public float zeroDistance, oneDistance;

        static Transform _target;
        Transform _t;
        Image _spriteRenderer;
        float DistanceToTarget => Vector2.Distance(_t.position, _target.position);
        void Awake() {
            _t = transform;
            _spriteRenderer = GetComponent<Image>();
        }
        void Start() {
            if (_target == null)
                _target = Player.Player.I.t;
        }
        void Update() {
            _spriteRenderer.color = _spriteRenderer.color.Alpha(Alpha);
        }
        float Alpha {
            get {
                float alpha = 0;
                float dist = DistanceToTarget;

                alpha = (dist - zeroDistance) / (oneDistance - zeroDistance);

                return Mathf.Clamp01(alpha);
            }
        }
        void OnDrawGizmos() {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, zeroDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, oneDistance);
        }
    }
}
