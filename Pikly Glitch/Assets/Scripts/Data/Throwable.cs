using Pikl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pikl.Data {
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Throwable", menuName = "Data/Interactable/New Throwable")]
    public class Throwable : Item {

        public GameObject liveObject;

        internal void Init(string name, Sprite sprite, string description, ItemType type, int quantity, int maxStack,
                           GameObject liveObject) {
            Init(name, sprite, description, type, quantity, maxStack);
            this.liveObject = liveObject;
        }

        public override void Use() {
            Vector2 pos = Player.Player.I.t.position + (Player.Player.I.input.MouseDir.normalized);
            var angle = Mathf.Atan2(Player.Player.I.input.MouseDir.normalized.y, Player.Player.I.input.MouseDir.normalized.x) * Mathf.Rad2Deg;

            Instantiate(liveObject, pos, Quaternion.AngleAxis(angle, Vector3.forward));


            base.Use();
        }

        public void Throw() {

        }

        public static Throwable CreateInstance(Throwable throwable) {
            var data = CreateInstance<Throwable>();
            data.Init(throwable.name, throwable.sprite, throwable.description, throwable.type, throwable.quantity, throwable.maxStack,
                      throwable.liveObject);
            return data;
        }
    }
}