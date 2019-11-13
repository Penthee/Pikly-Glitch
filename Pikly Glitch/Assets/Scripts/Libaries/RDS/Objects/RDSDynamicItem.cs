using UnityEngine;
using System.Collections;

namespace Pikl.Utils.RDS {
    /// <summary>
    /// Each time this item is chosen, it becomes more/less likely to be picked again the next time.
    /// </summary>
    public class RDSDynamicItem : RDSItem {
        public double probabilityChange;
        
        public RDSDynamicItem(GameObject obj, string name, double probability, double probabilityChange) : base(obj, name, probability) {
            rdsProbability = 1;
            this.probabilityChange = probabilityChange;
        }

        public override void OnRDSPreResultEvaluation(System.EventArgs e) {
            base.OnRDSPreResultEvaluation(e);
        }

        public override void OnRDSPostResultEvaluation(ResultEventArgs e) {
            base.OnRDSPostResultEvaluation(e);
        }

        public override void OnRDSHit(System.EventArgs e) {
            base.OnRDSHit(e);

            rdsProbability *= probabilityChange;

            if (rdsProbability < 0)
                rdsProbability = 0.01f;
        }
    }
}