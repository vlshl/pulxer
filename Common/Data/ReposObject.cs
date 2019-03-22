namespace Common.Data
{
    /// <summary>
    /// Use for persist objects
    /// </summary>
    public class ReposObject
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int ReposID { get; set; }

        /// <summary>
        /// Unique key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Serialized data
        /// </summary>
        public string Data { get; set; }

        public ReposObject()
        {
            ReposID = 0; Key = ""; Data = "";
        }
    }
}
