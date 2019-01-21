namespace Common.Data
{
    public class TickSourceItem
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int TickSourceID { get; private set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        public TickSourceItem(int tickSourceID, string name)
        {
            TickSourceID = tickSourceID;
            Name = name;
        }
    }
}
