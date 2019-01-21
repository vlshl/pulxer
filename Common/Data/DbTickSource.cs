namespace Common.Data
{
    /// <summary>
    /// DbTickSource
    /// </summary>
    public class DbTickSource
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int TickSourceID { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Serialized data
        /// </summary>
        public string DataStr { get; set; }

        public DbTickSource()
        {
            Name = "";
            DataStr = "";
        }
    }
}
