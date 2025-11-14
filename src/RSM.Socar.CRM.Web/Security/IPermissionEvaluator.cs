namespace RSM.Socar.CRM.Web.Security;

public interface IPermissionEvaluator
{
    Task<bool> HasPermissionAsync(string permission, CancellationToken ct);
}
