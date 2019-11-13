using UnityEngine;

namespace Pikl.Utils.RDS {
    /// <summary>Removes itself from the table once it's been evaluated, regardless of whether it was selected or not.</summary>
    public class RDSUniqueItem : RDSItem {
        public RDSUniqueItem(GameObject obj, string name, double probability) : base(obj, name, probability) {
            rdsAmount = 1;
            rdsUnique = true;
        }

        public override void OnRDSPostResultEvaluation(ResultEventArgs e) {
            //rdsEnabled = false;
            //rdsTable.rdsEnabled = false;
            base.OnRDSPostResultEvaluation(e);
        }
    }
}