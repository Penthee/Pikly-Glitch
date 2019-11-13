using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Pikl.UI
{
    public class Modal : Menu
    {
        public bool result;
        public Text message;

        public string Message
        {
            get { return message.text; }
            set { message.text = value; }
        }
    }
}