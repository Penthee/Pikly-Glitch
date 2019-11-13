using Pikl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pikl.Data {
    [CreateAssetMenu(fileName = "New Material", menuName = "Data/Interactable/New Material")]
    public class Material : Item {

        public override void Interact() {
            base.Interact();
        }

        public override void PickUp() {
            base.PickUp();
        }

        public override void Use() {
            base.Use();
        }

        public override void Drop() {
        }

        public static Material CreateInstance(Material material) {
            var data = CreateInstance<Material>();
            data.Init(material.name, material.sprite, material.description, material.type, material.quantity, material.maxStack);
            return data;
        }
    }
}