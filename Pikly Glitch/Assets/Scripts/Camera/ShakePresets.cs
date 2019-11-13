using UnityEngine;
using System.Collections.Generic;

namespace Pikl.Utils.Shaker {
    public static class ShakePresets {

        public static List<Shaker.ShakeInstance> All {
            get {
                return new List<Shaker.ShakeInstance>() {
                    new Shaker.ShakeInstance(Thud, null),
                    new Shaker.ShakeInstance(Shot, null),
                    new Shaker.ShakeInstance(Bump, null),
                    new Shaker.ShakeInstance(Explosion, null),
                    new Shaker.ShakeInstance(Death, null),
                    new Shaker.ShakeInstance(Earthquake, null),
                    new Shaker.ShakeInstance(BadTrip, null),
                    new Shaker.ShakeInstance(HandheldCamera, null),
                    new Shaker.ShakeInstance(Vibration, null),
                    new Shaker.ShakeInstance(RoughDriving, null),
                };
            }
        }
        
        public static List<Shaker.CameraShakeInstance> AllAsCameraShake {
            get {
                return new List<Shaker.CameraShakeInstance>() {
                    new Shaker.CameraShakeInstance(Thud),
                    new Shaker.CameraShakeInstance(Shot),
                    new Shaker.CameraShakeInstance(Bump),
                    new Shaker.CameraShakeInstance(Explosion),
                    new Shaker.CameraShakeInstance(Death),
                    new Shaker.CameraShakeInstance(Earthquake),
                    new Shaker.CameraShakeInstance(BadTrip),
                    new Shaker.CameraShakeInstance(HandheldCamera),
                    new Shaker.CameraShakeInstance(Vibration),
                    new Shaker.CameraShakeInstance(RoughDriving),
                };
            }
        }

        /// <summary>[Sustained] A bizarre shake that'll make any game impossible to play.</summary>
        public static Shake BadTrip {
            get {
                Shake c = new Shake(5f, 0.15f, 5f, 10f, Vector2.one * 0.7f, 70, 0.75f);
                c.loop = true;
                c.name = "Bad Trip";
                return c;
            }
        }

        /// <summary>[One-Shot] A high magnitude, short, yet smooth shake.</summary>
        public static Shake Bump {
            get {
                Shake c = new Shake(2.5f, 4, 0.1f, 0.75f, new Vector2(0.15f, 0.15f), 1, 0.1f);
                c.name = "Bump";
                return c;
            }
        }

        /// <summary>[One-Shot] An intense and rough shake that ends smoothly.</summary>
        public static Shake Death {
            get {
                Shake c = new Shake(3f, 4, 0, 1f, new Vector2(0.2f, 0.2f), 1, 0.25f);
                c.name = "Death";
                return c;
            }
        }

        /// <summary>[Sustained] A continuous, rough shake.</summary>
        public static Shake Earthquake {
            get {
                Shake c = new Shake(0.6f, 3.5f, 2f, 10f, new Vector2(0.25f, 0.25f), 4, 0.1f);
                c.loop = true;
                c.name = "Earthquake";
                return c;
            }
        }
        
        /// <summary>[One-Shot] A high magnitude, short, smooth rotation thud.</summary>
        public static Shake Evade {
            get {
                Shake c = new Shake(1.75f, 4.20f, 0.15f, 0.25f, new Vector2(0f, 0f), 3, 0.2f);
                c.name = "Thud";
                return c;
            }
        }

        /// <summary>[One-Shot] An intense and rough shake.</summary>
        public static Shake Explosion {
            get {
                Shake c = new Shake(5f, 10, 0, 1.5f, new Vector2(0.25f, 0.25f), 1, 0.25f);
                c.name = "Explosion";
                return c;
            }
        }
        
        /// <summary>[Sustained] A subtle, slow shake with low rotation.</summary>
        public static Shake HandheldCamera {
            get {
                Shake c = new Shake(1f, 0.25f, 5f, 10f, Vector2.one * 0.02f, 0.5f, 0f);
                c.loop = true;
                c.name = "Handheld Camera";
                return c;
            }
        }

        /// <summary>[One-Shot] A high magnitude, very short smoothish shake.</summary>
        public static Shake HeavyShot {
            get {
                Shake c = new Shake(3f, 4.20f, 0.15f, 0.25f, new Vector2(0.25f, 0.25f), 3, 0.1f);
                c.name = "Heavy Shot";
                return c;
            }
        }

        /// <summary>[Sustained] A slightly rough, medium magnitude shake.</summary>
        public static Shake RoughDriving {
            get {
                Shake c = new Shake(1, 2f, 1f, 1f, Vector2.zero, 1, 0);
                c.name = "Rough Driving";
                c.loop = true;
                return c;
            }
        }

        /// <summary>[Sustained] A very rough continous shake.</summary>
        public static Shake Rumble {
            get {
                Shake c = new Shake(1, 6.5f, 1f, 0.5f, Vector2.one * 0.5f, 1.5f, 0.1f);
                c.name = "Rumble";
                c.loop = true;
                return c;
            }
        }

        /// <summary>[One-Shot] A shake that has a long fade in time.</summary>
        public static Shake ShockCharge {
            get {
                Shake c = new Shake(2.5f, 5f, 1.5f, 0.75f, new Vector2(0.6f, 0.6f), 5, 0f);
                c.name = "Shot";
                return c;
            }
        }

        /// <summary>[One-Shot] A medium magnitude, very short smoothish shake.</summary>
        public static Shake Shot {
            get {
                Shake c = new Shake(0.75f, 4.20f, 0.15f, 0.25f, new Vector2(0.25f, 0.25f), 1, 0.05f);
                c.name = "Shot";
                return c;
            }
        }

        /// <summary>[One-Shot] A medium magnitude, short, smoothish zoom thud.</summary>
        public static Shake Thud {
            get {
                Shake c = new Shake(1.75f, 4.20f, 0f, 0.25f, new Vector2(0f, 0f), 0, 0.2f);
                c.name = "Thud";
                return c;
            }
        }

        /// <summary>[Sustained] A very rough, yet low magnitude shake.</summary>
        public static Shake Vibration {
            get {
                Shake c = new Shake(0.4f, 20f, 2f, 2f, new Vector2(0, 0.15f), 4, 0);
                c.loop = true;
                c.name = "Vibration";
                return c;
            }
        }
    }
}