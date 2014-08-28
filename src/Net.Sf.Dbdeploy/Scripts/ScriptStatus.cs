namespace Net.Sf.Dbdeploy.Scripts
{
    /// <summary>
    /// Status of the script execution.
    /// </summary>
    public enum ScriptStatus
    {
        /// <summary>
        /// The script failed to execute fully.
        /// </summary>
        Failure = 0,

        /// <summary>
        /// The script ran successfully.
        /// </summary>
        Success = 1,

        /// <summary>
        /// The problem was resolved from a previous script failure, and it should be retried.
        /// </summary>
        ProblemResolved = 2,

        /// <summary>
        /// The script has started execution.
        /// </summary>
        Started = 3,

        /// <summary>
        /// The script was revised for the user and ran successfully
        /// </summary>
        SucessRevisedUser = 4
    }
}
