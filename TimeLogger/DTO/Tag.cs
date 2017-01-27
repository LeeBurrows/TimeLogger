namespace TimeLogger.DTO
{
    //maps to sqlite table "Tags"
    internal class Tag
    {
        internal readonly int id;
        internal string label;

        internal Tag(int id, string label)
        {
            this.id = id;
            this.label = label;
        }

        internal string ToFullString()
        {
            return id.ToString() + " : " + label;
        }

        //should include all properties here.
        //this is a hack to make ComboBox use name property for its item labels
        public override string ToString()
        {
            return label; //id.ToString() + " : " + label;
        }

    }
}
