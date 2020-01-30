using System;
using UnityEngine;
using System.Collections;
using Pikl.Data;

namespace Pikl.Utils.RDS {
    [Serializable] public class RDSItem : RDSObject {
        public Item item;
        public int supply;
        public float probability;

        public RDSItem() {
            rdsProbability = probability;
        }
        
        public override void OnRDSHit(System.EventArgs e) {
            base.OnRDSHit(e);

            if (--supply <= 0)
                rdsProbability = 0f;
        }

        public override string ToString() {
            return string.Format("{0} \t Prob:{1},UAE:{2}{3}{4}", rdsName, rdsProbability, (rdsUnique ? "1" : "0"), (rdsAlways ? "1" : "0"), (rdsEnabled ? "1" : "0"));
        }
    }
}