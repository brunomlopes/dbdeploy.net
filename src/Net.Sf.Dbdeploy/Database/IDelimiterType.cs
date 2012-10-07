namespace Net.Sf.Dbdeploy.Database
{
    public interface IDelimiterType
    {
        bool Matches(string line, string delimiter);
    }
}
