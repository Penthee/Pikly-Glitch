using System.Collections;
using System.Collections.Generic;
using LOS;
using UnityEngine;

namespace Pikl {
    public class Door : MonoBehaviour {

        public bool locked = false;
        public LayerMask openingLayers;
        public float closeTime;
        [HideInInspector]
        public bool isOpen, isClosed, setLastTime;
        public float lastInRangeTime;
        Collider2D c2d;
        Animator animator;

        void Start() {
            c2d = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();
        }

        void Update() {
            if (isOpen) {
                if (!c2d.IsTouchingLayers(openingLayers)) {
                    if (!setLastTime) {
                        lastInRangeTime = Time.time;
                        setLastTime = true;
                    }

                    if (lastInRangeTime + closeTime < Time.time) {
                        animator.Play("Close");
                        isOpen = false;
                    }
                }
            } else {
                if (c2d.IsTouchingLayers(openingLayers) && !locked) {
                    animator.Play("Open");
                    isOpen = true;
                    setLastTime = false;
                }
            }
        }

        void SetDirty() {
            LOSManager.instance._isDirty = true;
        }
    }
}