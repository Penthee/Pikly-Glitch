using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pikl.Data {
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Interactable", menuName = "Data/Interactable/New Interactable")]
    public class Interactable : ScriptableObject {

        public new string name;
        public Sprite sprite;

        public virtual void Interact() {

        }

        internal void Init(string name, Sprite sprite) {
            this.name = name;
            this.sprite = sprite;
        }

        public static Interactable CreateInstance(Interactable interactable) {
            var data = CreateInstance<Interactable>();
            data.Init(interactable.name, interactable.sprite);
            return data;
        }
    }
}