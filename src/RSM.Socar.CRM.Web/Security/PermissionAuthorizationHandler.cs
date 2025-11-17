using Microsoft.AspNetCore.Authorization;
using RSM.Socar.CRM.Application.Security;

namespace RSM.Socar.CRM.Web.Security;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionEvaluator _evaluator;

    public PermissionAuthorizationHandler(IPermissionEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (await _evaluator.HasPermissionAsync(requirement.Permission, CancellationToken.None))
            context.Succeed(requirement);
    }
}
