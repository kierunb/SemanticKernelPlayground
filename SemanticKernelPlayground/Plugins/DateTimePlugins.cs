using Microsoft.SemanticKernel;

namespace SemanticKernelPlayground.Plugins;
public static class DateTimePlugins
{
    public static void RegisterPlugins(Kernel kernel)
    {
        kernel.Plugins.AddFromFunctions("time_plugin",
        [
            KernelFunctionFactory.CreateFromMethod(
                method: () => DateTime.Now,
                functionName: "get_time",
                description: "Get the current time"
            ),
            KernelFunctionFactory.CreateFromMethod(
                method: (DateTime start, DateTime end) => (end - start).TotalSeconds,
                functionName: "diff_time",
                description: "Get the difference between two times in seconds"
            )
        ]);
    }
}
