using Pikl;
using Pikl.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pikl.Data {
    [CreateAssetMenu(fileName = "New Text", menuName = "Data/Text/Level Text")]
    public class LevelInfo : Interactable {
        [TextArea(3, 10)]
        public string text;
        public string sceneToOpen;
        [Range(5, 20)]
        public float scrollSpeed = 5;
    }
}