using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using Smidge;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Dyfort.Umbraco.AutoImageOptimize
{
    public class AutoImageOptimizeComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddOptions<AutoImageOptimizerSettings>()
                     .Bind(builder.Config.GetSection(AutoImageOptimizerSettings.ConfigurationName));

            var settings = builder.Config.GetSection(AutoImageOptimizerSettings.ConfigurationName).Get<AutoImageOptimizerSettings>();

            if (settings == null)
            {
                settings = new AutoImageOptimizerSettings();
            }

            builder.Services.AddImageSharp(options =>
            {
                // Enable Lossless compression for transparent PNG
                options.OnBeforeSaveAsync = c =>
                {
                    if (c.Encoder.GetType().Name == "WebpEncoder" && c.Image.PixelType.BitsPerPixel == 32)
                    {
                        c.Encoder = new WebpEncoder()
                        {
                            FileFormat = WebpFileFormatType.Lossless,
                        };
                    }
                    return Task.CompletedTask;
                };

                options.OnParseCommandsAsync = c =>
                {
                    if (!settings.Enabled)
                    {
                        return Task.CompletedTask;
                    }


                    if (c.Context != null)
                    {

                        var path = c.Context.Request.Path.ToString();

                        // Don't convert when the noformat query string is set
                        if (c.Context.Request.QueryString.Value?.Contains("noformat") == true)
                            c.Commands.Add("noformat", "1");

                        if (c.Context.Request.QueryString.Value?.Contains("optimize") == true)
                            c.Commands.Add("noformat", "1");                        

                        var excludePath = settings.ExcludedFolderPaths.Any(x => path.Contains(x));

                        if (excludePath == false &&
                            c.Context.Request.GetTypedHeaders().Accept.Any(x => x.MediaType.Value == "image/webp"))
                        {

                            if (c.Commands.Contains("webp") == false &&
                                c.Commands.Contains("noformat") == false && path.EndsWithOneOf(settings.AllowedExtentions))
                            {
                                c.Commands.Remove("format");
                                c.Commands.Add("format", "webp");

                                if (c.Commands.Contains("quality") == false)
                                    c.Commands.Add("quality", settings.Quality.ToString());

                                c.Context.Response.Headers.Add("Vary", "Accept");
                            }
                        }
                    }

                    if (c.Commands.Count > 0)
                    {
                        // Check width and height to provide very basic security
                        var width = c.Parser.ParseValue<uint>(
                        c.Commands.GetValueOrDefault(ResizeWebProcessor.Width),
                        c.Culture);

                        var height = c.Parser.ParseValue<uint>(
                            c.Commands.GetValueOrDefault(ResizeWebProcessor.Height),
                            c.Culture);

                        // If width exceeds limit, remove it from request
                        if (width > 2400)
                            c.Commands.Remove(ResizeWebProcessor.Width);

                        // If height exceeds limit, remove it from request
                        if (height > 2400)
                            c.Commands.Remove(ResizeWebProcessor.Height);

                        // Remove format command if noformat command has been set
                        if (c.Commands.TryGetValue("noformat", out string value))
                        {
                            c.Commands.Remove("format");
                        }
                    }

                    return Task.CompletedTask;
                };
            });
        }
    }
}