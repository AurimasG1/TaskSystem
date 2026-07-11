using System.Text.Json;
using TaskSystem.Application.Common;

namespace TaskSystem.Tests.Common;

public sealed class OptionalJsonConverterTests
{
    private sealed class UpdateRequest
    {
        public Optional<string> Title { get; set; }
    }

    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new();

        options.Converters.Add(new OptionalJsonConverter<string>());

        return options;
    }

    [Fact]
    public void Deserialize_WhenPropertyIsMissing_HasNoValue()
    {
        JsonSerializerOptions options = CreateOptions();

        UpdateRequest request = JsonSerializer.Deserialize<UpdateRequest>("{}", options)!;

        Assert.False(request.Title.HasValue);
    }

    [Fact]
    public void Deserialize_WhenPropertyIsNull_HasValueWithNull()
    {
        JsonSerializerOptions options = CreateOptions();

        UpdateRequest request = JsonSerializer.Deserialize<UpdateRequest>(
            """
            {
                "Title": null
            }
            """,
            options
        )!;

        Assert.True(request.Title.HasValue);

        Assert.Null(request.Title.Value);
    }

    [Fact]
    public void Deserialize_WhenPropertyHasText_HasValueWithText()
    {
        JsonSerializerOptions options = CreateOptions();

        UpdateRequest request = JsonSerializer.Deserialize<UpdateRequest>(
            """
            {
                "Title": "Updated title"
            }
            """,
            options
        )!;

        Assert.True(request.Title.HasValue);

        Assert.Equal("Updated title", request.Title.Value);
    }
}
