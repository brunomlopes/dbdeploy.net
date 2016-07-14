namespace Net.Sf.Dbdeploy.Database
{
    /// <summary>
    /// Oracle syntax.
    /// </summary>
    public class Oracle8iDbmsSyntax : OracleDbmsSyntax
    {
        /// <summary>
        /// Gets the get timestamp.
        /// </summary>
        /// <value>
        /// The get timestamp.
        /// </value>
        public override string CurrentTimestamp
        {
            get { return "SYSDATE"; }
        }
    }
}