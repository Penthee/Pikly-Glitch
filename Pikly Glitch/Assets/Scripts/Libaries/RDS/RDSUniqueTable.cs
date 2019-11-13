using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pikl.Utils.RDS {
    /// <summary>
    /// Holds a table of RDS objects. This class is "the randomizer" of the RDS.
    /// The Result implementation of the IRDSTable interface uses the RDSRandom class
    /// to determine which elements are hit.
    /// </summary>
    public class RDSUniqueTable : RDSTable {
        #region CONSTRUCTORS
        /// <summary>
        /// Initializes a new instance of the <see cref="RDSTable"/> class.
        /// </summary>
        public RDSUniqueTable()
            : this(null, 1, 1, -1, true, false, true) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RDSTable"/> class.
        /// </summary>
        /// <param name="contents">The contents.</param>
        /// <param name="count">The count.</param>
        /// <param name="probability">The probability.</param>
        public RDSUniqueTable(IEnumerable<IRDSObject> contents, int count, double probability)
            : this(contents, count, probability, -1, true, false, true) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RDSTable"/> class.
        /// </summary>
        /// <param name="contents">The contents.</param>
        /// <param name="count">The count.</param>
        /// <param name="probability">The probability.</param>
        /// <param name="unique">if set to <c>true</c> any item of this table (or contained sub tables) can be in the result only once.</param>
        /// <param name="always">if set to <c>true</c> the probability is disabled and the result will always contain (count) entries of this table.</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        public RDSUniqueTable(IEnumerable<IRDSObject> contents, int count, double probability, int amount, bool unique, bool always, bool enabled) {
            if (contents != null)
                mcontents = contents.ToList();
            else
                ClearContents();
            rdsCount = count;
            rdsProbability = probability;
            rdsAmount = amount;
            rdsUnique = unique;
            rdsAlways = always;
            rdsEnabled = enabled;
        }
        #endregion

        public override void OnRDSPostResultEvaluation(ResultEventArgs e) {
            base.OnRDSPostResultEvaluation(e);
            UnityEngine.Debug.Log("post result event on table");
            rdsEnabled = false;
        }
    }
}
