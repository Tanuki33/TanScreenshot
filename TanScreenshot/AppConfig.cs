using System;
using System.Collections.Generic;

namespace TanScreenshot
{
    public class AppConfig
    {
        public List<string> HotKeys { get; set; } = new List<string> {};
        public string ScreenshotDirectory { get; set; } =
            System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "Screenshots");
        public bool CopyToClipboard { get; set; } = true; // Default enabled
    }
}