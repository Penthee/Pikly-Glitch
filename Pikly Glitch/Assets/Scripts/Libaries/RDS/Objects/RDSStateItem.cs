using UnityEngine;
using System.Collections;
using Pikl.States;

namespace Pikl.Utils.RDS {
    public class RDSStateItem : RDSObject {
        public State rdsState;

        public RDSStateItem(State state, string rdsName) {
            rdsState = state;
            this.rdsName = rdsName;
        }

        public RDSStateItem(State state, string name, float probability) {
            rdsState = state;
            rdsName = name;
            rdsProbability = probability;
        }

        public override string ToString() {
            return string.Format("{0} \t Prob:{1},UAE:{2}{3}{4}", rdsName, rdsProbability, (rdsUnique ? "1" : "0"), (rdsAlways ? "1" : "0"), (rdsEnabled ? "1" : "0"));
        }
    }
}