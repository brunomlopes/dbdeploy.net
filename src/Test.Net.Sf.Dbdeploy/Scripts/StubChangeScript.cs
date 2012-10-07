namespace Net.Sf.Dbdeploy.Scripts
{
    public class StubChangeScript : ChangeScript
    {
        private readonly string changeContents;

        public StubChangeScript(int changeNumber, string description, string changeContents)
            : base(changeNumber, description)
        {
            this.changeContents = changeContents;
        }

        public override string GetContent()
        {
            return changeContents;
        }
    }
}