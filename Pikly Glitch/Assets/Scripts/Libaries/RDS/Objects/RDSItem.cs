using UnityEngine;
using System.Collections;

namespace Pikl.Utils.RDS {
    public class RDSItem : RDSObject {
        public GameObject rdsObj;

        public RDSItem(GameObject obj, string rdsName) {
            rdsObj = obj;
            this.rdsName = rdsName;
        }

        public RDSItem(GameObject obj, string name, double probability) {
            rdsObj = obj;
            rdsName = name;
            rdsProbability = probability;
        }

        public override string ToString() {
            return string.Format("{0} \t Prob:{1},UAE:{2}{3}{4}", rdsName, rdsProbability, (rdsUnique ? "1" : "0"), (rdsAlways ? "1" : "0"), (rdsEnabled ? "1" : "0"));
        }
    }
}