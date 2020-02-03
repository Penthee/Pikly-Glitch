using Pikl;
using Pikl.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pikl.Data {
    [CreateAssetMenu(fileName = "New Text", menuName = "Data/Text/Level Text")]
    public class LevelInfo : Interactable {
        [TextArea(3, 10)]
        public string text;
        public string sceneToOpen;
        [FormerlySerializedAs("scrollSpeed")] [Range(5, 20)]
        public float scrollTime = 5;
    }
}