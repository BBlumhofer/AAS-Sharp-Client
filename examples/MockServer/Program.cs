using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();

// Simple mock AAS submodel endpoint
app.MapPut("/api/v3.0/submodels/{submodelId}", async ([FromRoute] string submodelId, [FromBody] JsonElement submodel) =>
{
    Console.WriteLine($"Received PUT request for submodel: {submodelId}");
    Console.WriteLine($"Submodel data: {submodel}");
    
    return Results.Ok(new { message = "Submodel updated successfully", submodelId });
});

app.MapGet("/api/v3.0/submodels/{submodelId}", ([FromRoute] string submodelId) =>
{
    Console.WriteLine($"Received GET request for submodel: {submodelId}");
    
    // Return a simple mock submodel
    var mockSubmodel = new
    {
        id = submodelId,
        idShort = "MachineSchedule",
        modelType = "Submodel",
        submodelElements = new object[]
        {
            new
            {
                idShort = "LastTimeUpdated",
                modelType = "Property",
                value = DateTime.UtcNow.ToString("o")
            },
            new
            {
                idShort = "HasOpenTasks",
                modelType = "Property", 
                value = true
            }
        }
    };
    
    return Results.Ok(mockSubmodel);
});

Console.WriteLine("Mock AAS Server starting on http://localhost:5000");
Console.WriteLine("Available endpoints:");
Console.WriteLine("- PUT /api/v3.0/submodels/{submodelId}");
Console.WriteLine("- GET /api/v3.0/submodels/{submodelId}");

app.Run("http://localhost:5000");