using UnityEngine;
using System.Collections;

namespace Pikl.Profile {
    public struct Res {
        public XmlAttribute<int> width, height;
        public XmlAttribute<bool> fullscreen;

        public Res(XmlAttribute<int> width, XmlAttribute<int> height, XmlAttribute<bool> fullscreen) {
            this.width = width;
            this.height = height;
            this.fullscreen = fullscreen;
        }
    }
}