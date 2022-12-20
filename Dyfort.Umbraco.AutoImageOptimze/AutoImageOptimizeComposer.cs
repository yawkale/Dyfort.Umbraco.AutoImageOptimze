using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Dyfort.Umbraco.AutoImageOptimize
{
    public class AutoImageOptimizeComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            Configuration.Default.ImageFormatsManager.SetEncoder(WebpFormat.Instance, new WebpEncoder()
            {
                FileFormat = WebpFileFormatType.Lossy
            });

            builder.Services.AddTransient<IStartupFilter, AutoImageOptimizeStartupFilter>();
            builder.Services.AddOptions<AutoImageOptimizerSettings>()
                     .Bind(builder.Config.GetSection(AutoImageOptimizerSettings.ConfigurationName));
        }
    }
}