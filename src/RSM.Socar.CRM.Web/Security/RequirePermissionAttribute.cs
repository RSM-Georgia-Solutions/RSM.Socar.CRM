namespace RSM.Socar.CRM.Web.Security
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class RequirePermissionAttribute(string permission)
    : Attribute
    {
        public string Permission { get; } = permission;
    }
}
