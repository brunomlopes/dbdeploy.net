namespace Net.Sf.Dbdeploy.Appliers.NVelocityReplacements
{
    using System;

    using NVelocity.Runtime.Directive;

    public class DbDeployDirectiveManager : DirectiveManager
    {
        public override void Register(string directiveTypeName)
        {
            try
            {
                base.Register(directiveTypeName);
            }
            catch (Exception)
            {
                // if we can't register the directive in the original assembly, we might be running in an internalized scenario
                // try and find the type in this assembly
                base.Register(directiveTypeName.Replace(",NVelocity", "," + GetType().Assembly.GetName().Name));
            }
        }
    }
}