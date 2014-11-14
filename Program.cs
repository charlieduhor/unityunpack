using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace UnityUnpack {
    class Program {
        public static void Main(string[] args) {
            string[] files = new string[] {
                /*"resources.assets",
                "sharedassets0.assets",
                "sharedassets1.assets",
                "sharedassets2.assets",
                "sharedassets3.assets",
                "sharedassets4.assets",
                "sharedassets5.assets",
                "sharedassets6.assets",
                "sharedassets7.assets",
                "sharedassets8.assets",
                "sharedassets9.assets",
                "sharedassets10.assets",
                "sharedassets11.assets",
                "sharedassets12.assets",
                "sharedassets13.assets",
                "sharedassets14.assets",
                "sharedassets15.assets",
                "sharedassets16.assets",
                "sharedassets17.assets",
                "sharedassets18.assets",
                "sharedassets19.assets",
                "sharedassets20.assets",
                "sharedassets21.assets",
                "sharedassets22.assets",
                "sharedassets23.assets",
                "sharedassets24.assets",
                "sharedassets25.assets",
                "sharedassets26.assets",
                "sharedassets27.assets",
                "sharedassets28.assets",
                "sharedassets29.assets",
                "sharedassets30.assets",
                "sharedassets31.assets",
                "sharedassets32.assets",
                "sharedassets33.assets",
                "sharedassets34.assets",
                "sharedassets35.assets",
                "sharedassets36.assets",
                "sharedassets37.assets",
                "sharedassets38.assets",
                "sharedassets39.assets",
                "sharedassets40.assets",
                "sharedassets41.assets",
                "sharedassets42.assets",
                "sharedassets43.assets",
                "sharedassets44.assets",
                "sharedassets45.assets",
                "sharedassets46.assets",
                "sharedassets47.assets",
                "sharedassets48.assets",
                "sharedassets49.assets",*/
                "level45",
            };

            string source = "C:\\LarryReloaded.app\\Contents\\Data";
            string dest   = "C:\\Larry.Disasm";

            foreach (string file in files) {
                AssetFile assetFile = new AssetFile(Path.Combine(source, file));

                string folder = Path.Combine(dest, file);

                if (!Directory.Exists(folder)) {
                    Directory.CreateDirectory(folder);
                }

                assetFile.ExtractToFolder(folder);
            }
        }
    }
}
