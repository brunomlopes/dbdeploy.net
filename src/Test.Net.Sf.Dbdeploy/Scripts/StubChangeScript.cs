namespace Net.Sf.Dbdeploy.Scripts
{
    public class StubChangeScript : ChangeScript
    {
        private readonly string changeContents;

        public StubChangeScript(int changeNumber, string fileName, string changeContents)
            : base("Scripts", changeNumber, fileName)
        {
            this.changeContents = changeContents;
        }

        public override string GetContent()
        {
            return changeContents;
        }
    }
}