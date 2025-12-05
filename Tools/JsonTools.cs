using System.Text.Json;
using System.Text.Json.Serialization;

namespace AasSharpClient.Tools;

public static class JsonTools
{
    public static JsonSerializerOptions CreateDefaultOptions(bool indented = true)
    {
        return new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = null,
            WriteIndented = indented
        };
    }
}
