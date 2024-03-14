using Domain.Contracts;
using Domain.Extensions;
using Domain.FluentValidators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace EventHubValidator
{
    /// <remarks>
    /// https://json-everything.net/json-schema allows lots to be done in the name of schema
    /// </remarks>
    internal class Program
    {
        static void Main(string[] args)
        {
            JsonSchemaExample();
            DataAnnotationsAndNewtonsoftExample();
            FluentValidatedExample();
            SystemTextJsonExample();
        }

        private static void SystemTextJsonExample()
        {
            string jsonData =
                @"
            {
                ""SourceSystemId"": ""Ipsum aut consequatur eius provident molestiae consequatur odit.\nExpedita animi sed aperiam.\nEarum i"",
                ""Products"": []
            }";

            var manifestDetail = System.Text.Json.JsonSerializer.Deserialize<ManifestDetail>(
                jsonData
            );
            if (!manifestDetail.IsValid(out var validationResults))
            {
                Console.WriteLine("System.Text.JSON data is not valid. Validation errors:");
                foreach (var validationResult in validationResults!)
                {
                    Console.WriteLine($"- {validationResult.ErrorMessage}");
                }
            }
        }

        private static void FluentValidatedExample()
        {
            string jsonData =
                @"
            {
                ""SourceSystemId"": ""Ipsum aut consequatur eius provident molestiae consequatur odit.\nExpedita animi sed aperiam.\nEarum i"",
            }";
            var manifestDetail = JsonConvert.DeserializeObject<ManifestDetail>(jsonData);
            var validator = new ManifestDetailValidator();
            var validationResult = validator.Validate(manifestDetail!);

            if (!validationResult.IsValid)
            {
                Console.WriteLine("Fluent validated JSON data is not valid. Validation errors:");
                foreach (var error in validationResult.Errors)
                {
                    Console.WriteLine($"- {error.ErrorMessage}");
                }
            }
        }

        private static void DataAnnotationsAndNewtonsoftExample()
        {
            string jsonData =
                @"
            {
                ""SourceSystemId"": ""Ipsum aut consequatur eius provident molestiae consequatur odit.\nExpedita animi sed aperiam.\nEarum i"",
            }";
            var manifestDetail = JsonConvert.DeserializeObject<ManifestDetail>(jsonData);
            if (!manifestDetail.IsValid(out var validationResults))
            {
                Console.WriteLine("Manifest data is not valid. Validation errors:");
                foreach (var validationResult in validationResults!)
                {
                    Console.WriteLine($"- {validationResult.ErrorMessage}");
                }
            }
        }

        private static void JsonSchemaExample()
        {
            var jsonSchema = File.ReadAllText(@"./Schemas/SchemaExample.json");
            JSchema schema = JSchema.Parse(jsonSchema);
            string validJsonData =
                @"
            {
                ""SourceSystemId"": ""Ipsum aut consequatur eius provident molestiae consequatur odit.\nExpedita animi sed aperiam.\nEarum i"",
                ""EventId"": ""Enim voluptatem ipsum ratione cumque recusandae impedit. Porro omnis sed. Possimus omnis et hic quo"",
                ""Timestamp"": ""2024-03-12T22:26:46.3780055+00:00"",
                ""PurchaseOrderNumber"": ""omnisA"",
                ""DeliveryLocation"": 152,
                ""Products"": [
                    {
                        ""Sku"": ""Nemo libero atque."",
                        ""SerialNumber"": ""utAAAA"",
                        ""Fugiat"": ""Consequatur provident magnam.""
                    }
                ]
            }";

            string invalidJsonData =
                @"
            {
                ""SourceSystemId"": ""i"",
                ""Products"": []
            }";
            // Succeed Scenario
            JObject jsonDataObject = JObject.Parse(validJsonData);
            bool isValid = jsonDataObject.IsValid(schema);
            Console.WriteLine($"Valid data sample is {isValid}");
            var invalidJsonDataObject = JObject.Parse(invalidJsonData);
            // Fail Scenario
            bool isOtherValid = invalidJsonDataObject.IsValid(
                schema,
                out IList<ValidationError>? validationErrors
            );
            Console.WriteLine($"Invalid data sample is {isOtherValid}");
            foreach (var error in validationErrors)
            {
                Console.WriteLine(
                    $"Property '{error.Path}' failed with {error.ErrorType} issue because {error.Message}"
                );
            }
        }
    }
}
