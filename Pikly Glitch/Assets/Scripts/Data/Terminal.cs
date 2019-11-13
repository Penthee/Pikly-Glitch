using Pikl;
using Pikl.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pikl.Data {
    [CreateAssetMenu(fileName = "New Item", menuName = "Data/Interactable/New Terminal")]
    public class Terminal : Interactable {
        [TextArea(3, 10)]
        public string text;
    }
}