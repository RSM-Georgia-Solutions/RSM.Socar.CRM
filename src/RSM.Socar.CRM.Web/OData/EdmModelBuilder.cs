using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Web.OData;

public static class EdmModelBuilder
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.Namespace = "Crm";
        builder.ContainerName = "CrmContainer";

        // Expose DTO, not the EF entity
        var users = builder.EntitySet<User>("Users");

        // Hide PasswordHash from the OData metadata & payloads
        users.EntityType.Ignore(u => u.PasswordHash);


        return builder.GetEdmModel();
    }
}
