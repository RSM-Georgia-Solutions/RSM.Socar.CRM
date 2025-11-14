using System.Reflection;
using RSM.Socar.CRM.Domain.Common;

namespace RSM.Socar.CRM.Infrastructure.Security;

public interface IPermissionDiscoveryService
{
    Task<List<string>> DiscoverAsync();
}

public sealed class PermissionDiscoveryService : IPermissionDiscoveryService
{
    private readonly Assembly[] _assemblies;

    public PermissionDiscoveryService()
    {
        _assemblies =
        [
            typeof(BaseEntity).Assembly,                       // Domain
            Assembly.Load("RSM.Socar.CRM.Web")                // Web (reflection only)
        ];
    }

    public Task<List<string>> DiscoverAsync()
    {
        var output = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var asm in _assemblies)
            ScanEntities(asm, output);

        foreach (var asm in _assemblies)
            ScanControllersForRequirePermission(asm, output);

        return Task.FromResult(output.ToList());
    }

    // ---------------------------------------------------------------------
    // ENTITY CRUD + COLUMN PERMISSIONS
    // ---------------------------------------------------------------------
    private static void ScanEntities(Assembly asm, HashSet<string> output)
    {
        var entities = asm
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(BaseEntity)));

        foreach (var entity in entities)
        {
            var name = entity.Name;

            output.Add($"{name}.Read");
            output.Add($"{name}.Create");
            output.Add($"{name}.Update");
            output.Add($"{name}.Delete");

            foreach (var p in entity.GetProperties())
            {
                if (IsPrimitive(p.PropertyType))
                {
                    output.Add($"{name}.Property.{p.Name}.Read");
                    output.Add($"{name}.Property.{p.Name}.Update");
                }
            }
        }
    }

    private static bool IsPrimitive(Type type)
    {
        return type.IsPrimitive ||
               type == typeof(string) ||
               type == typeof(DateTime) ||
               type == typeof(decimal) ||
               type.IsEnum;
    }

    // ---------------------------------------------------------------------
    // SCAN CONTROLLERS FOR [RequirePermission("X")]
    // But WITHOUT referencing the attribute type directly
    // ---------------------------------------------------------------------
    private static void ScanControllersForRequirePermission(Assembly asm, HashSet<string> output)
    {
        foreach (var type in asm.GetTypes().Where(t => t.IsClass && t.Name.EndsWith("Controller")))
        {
            foreach (var method in type.GetMethods())
            {
                foreach (var attr in method.GetCustomAttributes())
                {
                    var attrType = attr.GetType();

                    if (attrType.Name == "RequirePermissionAttribute")
                    {
                        // read property "Permission"
                        var nameProp = attrType.GetProperty("Permission");
                        var perm = nameProp?.GetValue(attr) as string;

                        if (!string.IsNullOrWhiteSpace(perm))
                            output.Add(perm);
                    }
                }
            }
        }
    }
}
