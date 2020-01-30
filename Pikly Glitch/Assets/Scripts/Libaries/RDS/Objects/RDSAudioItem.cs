using UnityEngine;
using System.Collections;

namespace Pikl.Utils.RDS {
    public class RDSAudioItem : RDSObject {
        public AudioClip rdsClip;

        public RDSAudioItem(AudioClip clip, string rdsName) {
            rdsClip = clip;
            this.rdsName = rdsName;
        }

        public RDSAudioItem(AudioClip clip, string name, float probability) {
            rdsClip = clip;
            rdsName = name;
            rdsProbability = probability;
        }

        public override string ToString() {
            return string.Format("{0} \t Prob:{1},UAE:{2}{3}{4}", rdsName, rdsProbability, (rdsUnique ? "1" : "0"), (rdsAlways ? "1" : "0"), (rdsEnabled ? "1" : "0"));
        }
    }
}