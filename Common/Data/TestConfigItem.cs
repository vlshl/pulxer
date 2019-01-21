namespace Common.Data
{
    public class TestConfigItem
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int TestConfigID { get; private set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        public TestConfigItem(int testConfigID, string name)
        {
            TestConfigID = testConfigID;
            Name = name;
        }
    }
}
