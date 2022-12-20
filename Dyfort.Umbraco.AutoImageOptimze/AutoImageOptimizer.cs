using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Extensions;

namespace Dyfort.Umbraco.AutoImageOptimize
{
    public class AutoImageOptimizer
    {        
        private readonly RequestDelegate _next;
        private readonly AutoImageOptimizerSettings settings;

        public AutoImageOptimizer(RequestDelegate next, IOptions<AutoImageOptimizerSettings> options)
        {
            _next = next;
            this.settings = options.Value;
        }

        private QueryString QueryModifier(QueryString q)
        {
            //Gets the imageprocessor quertystring
            var queryString = HttpUtility.ParseQueryString(q.Value);


            if (settings.ConvertWebp && queryString.Get("format") != "webp")
            {
                queryString.Remove("format");
                queryString["format"] = "webp";
            }


            if (settings.Quality != null && queryString.Get("quality") == null)
            {
                queryString["quality"] = settings.Quality.Value.ToString();
            }

            var maxSize = false;

            if (settings.MaxWidth !=null && queryString.Get("width") == null)
            {
                queryString["width"] = settings.MaxWidth.Value.ToString();
                maxSize = true;
            }

            if (settings.MaxHeight != null && queryString.Get("height") == null)
            {
                queryString["height"] = settings.MaxHeight.Value.ToString();
                maxSize = true;
            }

            //If maxsize is set and mode is missing set to max
            if (maxSize == true && queryString.Get("mode") == null)
            {
                queryString["mode"] = "max";
            }
            var qb1 = new QueryBuilder(queryString.AsEnumerable());
            return qb1.ToQueryString();
        }

        private bool IsImagePath(PathString path)
        {
            if (path == null || !path.HasValue)
                return false;

            return settings.AllowedExtentions.Any(x => path.Value.EndsWith(x, StringComparison.OrdinalIgnoreCase));
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (context.Request.Query["optimize"] == "false")
            {
                await _next(context);
                return;
            }

            if (!settings.Enabled)
            {
                await _next(context);
                return;
            }                

            var path = context.Request.Path;

            var isImage = IsImagePath(path);
            if (isImage == false)
            {
                await _next.Invoke(context);
                return;
            }

            var acceptWebP = context.Request.GetTypedHeaders().Accept.Any(aValue => aValue.MediaType.Value == "image/webp");

            if (!acceptWebP)
            {
                await _next(context);
                return;
            }
            context.Request.QueryString = QueryModifier(context.Request.QueryString);
            await _next(context);
        }
    }
}
