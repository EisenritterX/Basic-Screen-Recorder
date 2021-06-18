using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Basic_Screen_Recorder
{
    class PathSelector
    {
        public static string outputPath { get; set; }

        public static string PathSelection()
        {
            // Create output path:
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Select an output folder";

            if (folderBrowser.ShowDialog()== DialogResult.OK)
            {
                outputPath = @folderBrowser.SelectedPath;
            }

            return outputPath;
        }
    }
}
