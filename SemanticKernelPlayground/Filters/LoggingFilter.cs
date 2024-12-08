using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticKernelPlayground.Filters;
public sealed class LoggingFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        Console.WriteLine($"FunctionInvoking - {context.Function.PluginName}.{context.Function.Name}");

        await next(context);

        Console.WriteLine($"FunctionInvoked - {context.Function.PluginName}.{context.Function.Name}");
    }
}
