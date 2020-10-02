namespace Pulxer.Drawing
{
    /// <summary>
    /// Indicator factory item
    /// </summary>
    public class FactoryItem
    {
        /// <summary>
        /// Key of indicator (reserved names)
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Display name of indicator
        /// </summary>
        public string Name { get; private set; }

        public FactoryItem(string key, string name)
        {
            Key = key; Name = name;
        }
    }
}
