using Microsoft.SemanticKernel;

namespace SemanticKernelPlayground.Plugins;

public sealed class WeatherPlugin
{
    [KernelFunction]
    public string GetWeather(string location) => $"Weather in {location} is 70°F.";
}
