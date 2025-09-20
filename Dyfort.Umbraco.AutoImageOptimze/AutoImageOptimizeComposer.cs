using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using Smidge;
using System.Linq;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Imaging.ImageSharp;

namespace Dyfort.Umbraco.AutoImageOptimize
{
    [ComposeAfter(typeof(ImageSharpComposer))]
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
			builder.Services.Configure<ImageSharpMiddlewareOptions>(options =>
			{			
				var onParseCommandsAsync = options.OnParseCommandsAsync;
			
				options.OnParseCommandsAsync = async context =>
				{
					if (!settings.Enabled)
					{
						await onParseCommandsAsync(context);
						return;
					}

					if (context.Context != null)
					{					
						var path = context.Context.Request.Path.ToString();

						// Don't convert when the noformat query string is set
						if (context.Context.Request.QueryString.Value?.Contains("noformat") == true)
						{
							context.Commands.Add("noformat", "1");
						}

						if (context.Context.Request.QueryString.Value?.Contains("optimize") == true)
						{
							context.Commands.Add("noformat", "1");
						}

						// Exclude /umbraco/assets and don't convert if WebP is not supported
						var excludePath = settings.ExcludedFolderPaths.Any(x => path.Contains(x));


						if (excludePath == false &&
							context.Context.Request.GetTypedHeaders().Accept.Any(x => x.MediaType.Value == "image/webp"))
						{

							if (context.Commands.Contains("webp") == false &&
								context.Commands.Contains("noformat") == false && path.EndsWithOneOf(settings.AllowedExtentions))
							{
								context.Commands.Remove("format");
								context.Commands.Add("format", "webp");

								if (context.Commands.Contains("quality") == false)
									context.Commands.Add("quality", settings.Quality.ToString());

								context.Context.Response.Headers["Vary"] = "Accept";
							}
						}
					}

					if (context.Commands.Count > 0)
					{
						// Check width and height to provide very basic security
						var width = context.Parser.ParseValue<uint>(
						context.Commands.GetValueOrDefault(ResizeWebProcessor.Width),
						context.Culture);

						var height = context.Parser.ParseValue<uint>(
							context.Commands.GetValueOrDefault(ResizeWebProcessor.Height),
							context.Culture);

						// If width exceeds limit, remove it from request
						if (width > 2400)
							context.Commands.Remove(ResizeWebProcessor.Width);

						// If height exceeds limit, remove it from request
						if (height > 2400)
							context.Commands.Remove(ResizeWebProcessor.Height);

						// Remove format command if noformat command has been set
						if (context.Commands.TryGetValue("noformat", out string value))
						{
							context.Commands.Remove("format");
						}
					}
                    await onParseCommandsAsync(context);
                };
			});
        }
	}
}