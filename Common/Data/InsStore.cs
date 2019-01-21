using Platform;

namespace Common.Data
{
    /// <summary>
    /// History quotes of the fin. instrument
    /// </summary>
    public class InsStore
    {
        public InsStore()
        {
            this.InsStoreID = 0;
            this.InsID = 0;
            this.Tf = 0;
            this.IsEnable = false;
        }

        /// <summary>
        /// Primary key
        /// </summary>
        public int InsStoreID { get; set; }

        /// <summary>
        /// Fin. instrument reference
        /// </summary>
        public int InsID { get; set; }

        /// <summary>
        /// Time frame
        /// </summary>
        public Timeframes Tf { get; set; }

        /// <summary>
        /// Enable for synchronization
        /// </summary>
        public bool IsEnable { get; set; }
    }
}
