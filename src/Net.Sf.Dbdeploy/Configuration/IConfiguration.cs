namespace Net.Sf.Dbdeploy.Configuration
{
    public interface IConfiguration
    {
        string DbConnectionString { get; }
        string DbType { get; }
        string DbDeltaSet { get; }
        int? CurrentDbVersion { get; }
    }
}