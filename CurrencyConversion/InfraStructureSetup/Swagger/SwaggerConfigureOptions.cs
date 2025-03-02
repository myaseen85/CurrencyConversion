using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

namespace CurrencyConversion.InfraStructureSetup.Swagger
{
    public class SwaggerConfigureOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        public SwaggerConfigureOptions(IApiVersionDescriptionProvider provider)
        {
            this._provider = provider;
        }

        public void Configure(SwaggerGenOptions swaggerGenOptions)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                swaggerGenOptions.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }
        }
        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo
            {
                Title = "Currency Conversion APIs",
                Version = description.ApiVersion.ToString(),
            };

            return info;
        }
    }
}
