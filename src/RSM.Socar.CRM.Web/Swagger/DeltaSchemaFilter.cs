using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

namespace RSM.Socar.CRM.Web.Swagger;

public sealed class DeltaSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var t = context.Type;
        if (!t.IsGenericType) return;

        var gtd = t.GetGenericTypeDefinition();
        if (gtd.FullName is null) return;

        // Matches Microsoft.AspNetCore.OData.Deltas.Delta`1 and DeltaOfT subclasses
        if (gtd.FullName.StartsWith("Microsoft.AspNetCore.OData.Deltas.Delta"))
        {
            var innerType = t.GetGenericArguments()[0];
            var innerSchema = context.SchemaGenerator.GenerateSchema(innerType, context.SchemaRepository);

            // Show Delta<T> as a partial T (object with T's properties)
            schema.Type = "object";
            schema.Properties = innerSchema.Properties;
            schema.Required = innerSchema.Required;
            schema.AdditionalPropertiesAllowed = true; // PATCH may send partials
        }
    }
}
