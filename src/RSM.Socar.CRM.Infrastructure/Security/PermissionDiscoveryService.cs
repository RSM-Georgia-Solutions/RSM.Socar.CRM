using System.Reflection;
using RSM.Socar.CRM.Domain.Common;
using RSM.Socar.CRM.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace RSM.Socar.CRM.Infrastructure.Security;

public interface IPermissionDiscoveryService
{
    Task<List<Permission>> DiscoverAsync();
}

public sealed class PermissionDiscoveryService : IPermissionDiscoveryService
{
    private readonly Assembly _domainAssembly;

    public PermissionDiscoveryService()
    {
        _domainAssembly = typeof(BaseEntity).Assembly;
    }

    public Task<List<Permission>> DiscoverAsync()
    {
        var permissions = new List<Permission>();

        var entities = _domainAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(BaseEntity)));

        foreach (var entity in entities)
        {
            var entityName = entity.Name;

            // Basic CRUD
            permissions.AddRange(new[]
            {
                New($"{entityName}.Read"),
                New($"{entityName}.Create"),
                New($"{entityName}.Update"),
                New($"{entityName}.Delete"),
            });

            // Column-level permissions
            foreach (var prop in entity.GetProperties())
            {
                if (prop.PropertyType.Namespace == "System") // exclude nav
                {
                    permissions.Add(New($"{entityName}.Property.{prop.Name}.Read"));
                    permissions.Add(New($"{entityName}.Property.{prop.Name}.Update"));
                }
            }
        }

        return Task.FromResult(permissions);
    }

    private static Permission New(string name) =>
        new Permission { Name = name };
}
