namespace Common.Data
{
    /// <summary>
    /// DbTestConfig
    /// </summary>
    public class DbTestConfig
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int TestConfigID { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Serialized data
        /// </summary>
        public string DataStr { get; set; }

        public DbTestConfig()
        {
            Name = "";
            DataStr = "";
        }
    }
}
