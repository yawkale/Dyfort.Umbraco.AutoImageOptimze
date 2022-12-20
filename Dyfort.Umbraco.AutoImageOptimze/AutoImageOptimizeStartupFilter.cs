using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Dyfort.Umbraco.AutoImageOptimize
{
    public class AutoImageOptimizeStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.UseMiddleware<AutoImageOptimizer>();
            next(app);
        };
    }
}