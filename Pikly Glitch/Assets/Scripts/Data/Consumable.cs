using Pikl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pikl.Data {
    public enum ConsumableEffect { Heal, Armour, Speed, Resistance, Berserk };

    [System.Serializable]
    [CreateAssetMenu(fileName = "New Consumable", menuName = "Data/Interactable/New Consumable")]
    public class Consumable : Item {

        public ConsumableEffect effect;
        public int strength;

        internal void Init(string name, Sprite sprite, string description, ItemType type, int quantity, int maxStack, 
                           ConsumableEffect effect, int strength) {
            Init(name, sprite, description, type, quantity, maxStack);
            this.effect = effect;
            this.strength = strength;
        }

        public override void Use() {
            switch (effect) {
                case ConsumableEffect.Heal:
                    if (name == "Canned Meat") {
                        if (Player.Player.I.health.HP < Player.Player.I.health.MaxHp + 50) {
                            Player.Player.I.health.Overheal(strength);
                            Player.Player.I.inventory.Add(Player.Player.I.inventory.emptyCan);
                        } else
                            return;
                    } else {
                        if (Player.Player.I.health.HP < Player.Player.I.health.MaxHp)
                            Player.Player.I.health.Heal(strength);
                        else
                            return;
                    }
                    break;
                case ConsumableEffect.Armour:
                    if (Player.Player.I.health.Armour < Player.Player.I.health.maxArmour)
                        Player.Player.I.health.AddArmour(strength);
                    break;
                case ConsumableEffect.Berserk:
                    //Player.Player.I.powerup.Berserk();
                    break;
                case ConsumableEffect.Resistance:
                    //Player.Player.I.powerup.Resistance();
                    break;
                case ConsumableEffect.Speed:
                    //Player.Player.I.powerup.Speed();
                    break;
            }

            base.Use();
        }

        public static Consumable CreateInstance(Consumable consumable) {
            var data = CreateInstance<Consumable>();
            data.Init(consumable.name, consumable.sprite, consumable.description, consumable.type, consumable.quantity, consumable.maxStack,
                      consumable.effect, consumable.strength);
            return data;
        }
    }
}