namespace Net.Sf.Dbdeploy.Scripts
{
    public class StubChangeScript : ChangeScript
    {
        private readonly string changeContents;

        public StubChangeScript(int changeNumber, string fileName, string changeContents)
            : base("v1.0", changeNumber, fileName)
        {
            this.changeContents = changeContents;
        }

        public override string GetContent()
        {
            return changeContents;
        }
    }
}