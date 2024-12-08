using Microsoft.SemanticKernel;

namespace SemanticKernelPlayground.Plugins;

public sealed class LocationPlugin
{
    [KernelFunction]
    public string GetCurrentLocation() => "Warsaw";
}
