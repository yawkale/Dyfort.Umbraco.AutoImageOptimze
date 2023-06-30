namespace Dyfort.Umbraco.AutoImageOptimize
{
    public class AutoImageOptimizerSettings
    {
        public static string ConfigurationName = "AutoImageOptimizer";
        public int? Quality { get; set; } = 92;
        public int? MaxWidth { get; set; }
        public int? MaxHeight { get; set; }
        public string[] AllowedExtentions { get; set; } = new string[] {
            ".png",
            ".jpg",
            ".jpeg"
        };

        public bool ConvertWebp { get; set; } = true;
        public bool Enabled { get; set; } = true;
    }
}
