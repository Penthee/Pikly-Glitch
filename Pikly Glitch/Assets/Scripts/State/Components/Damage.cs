using UnityEngine;
using System.Collections;

namespace Pikl.States.Components {
    [System.Serializable]
    public class Damage {
        public enum Type { Normal, Stun, Burn, Chill, Explode, Heal };
        public enum Status { Burning, Chilled, Stunned, Ok };
        public Type type;
        public float baseDmg, knockbackForce, tickRepeatRate, effectDuration, cooldown;
        public int ticks;
        public double effectChance;
        public Transform origin;

        public Damage() : this(0) { }

        public Damage(Damage d) {
            this.baseDmg = d.baseDmg;
            this.type = d.type;
            this.tickRepeatRate = d.tickRepeatRate;
            this.effectDuration = d.effectDuration;
            this.ticks = d.ticks;
            this.effectChance = d.effectChance;
            this.knockbackForce = d.knockbackForce;
            this.cooldown = d.cooldown;
            this.origin = d.origin;
        }

        public Damage(float baseDmg) : this(baseDmg, Type.Normal, 0, 0, 0, 0, 0, 0, null) { }

        public Damage(float baseDmg, Type type, float tickRepeatRate, float effectDuration, int ticks, double effectChance, float knockbackForce, float cooldown, Transform origin) {
            this.baseDmg = baseDmg;
            this.type = type;
            this.tickRepeatRate = tickRepeatRate;
            this.effectDuration = effectDuration;
            this.ticks = ticks;
            this.effectChance = effectChance;
            this.knockbackForce = knockbackForce;
            this.cooldown = cooldown;
            this.origin = origin;
        }
    }
}