namespace RSM.Socar.CRM.Application.Security
{
    public interface IPermissionEvaluator
    {
        Task<bool> HasPermissionAsync(string permission, CancellationToken ct);
    }
}
