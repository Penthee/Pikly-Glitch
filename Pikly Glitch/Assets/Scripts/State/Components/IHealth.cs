using UnityEngine;
using System.Collections;

namespace Pikl.States.Components {
    public interface IHealth {

        Damage.Status Status { get; set; }
        bool isDead { get; set; }
        bool Invulnerable { get; set; }
        float HP { get; set; }
        float MaxHp { get; set; }

        void TakeDamage(float dmg);

        void TakeDamage(Damage dmg);

        IEnumerator StartEffect(Damage dmg);

        Damage.Status TypeToStatus(Damage.Type type);

        void StopEffect();

        IEnumerator DamageTick(Damage dmg);

        void Heal(float amount);

        void Die();
    }
}