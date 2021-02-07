using System;
using System.Collections.Generic;
using System.IO;

namespace Q2MdlGen
{

    class Program
    {

        static void ProcessTextures(string baseName)
        {
            if (File.Exists(baseName + "(Color).png") && (!File.Exists(baseName + ".tga") || new FileInfo(baseName + "(Color).png").LastWriteTime != new FileInfo(baseName + ".tga").LastWriteTime || (File.Exists(baseName + "(Roughness).png") && new FileInfo(baseName + "(Roughness).png").LastWriteTime != new FileInfo(baseName + ".tga").LastWriteTime)))
            {
                var newDate = new FileInfo(baseName + "(Color).png").LastWriteTime;
                var baseTga = new TGA();
                baseTga.LoadImage(baseName + "(Color).png");
                if (File.Exists(baseName + "(Roughness).png"))
                {
                    var potNewDate = new FileInfo(baseName + "(Roughness).png").LastWriteTime;
                    if (potNewDate > newDate)
                        newDate = potNewDate;
                    var baseAlphaTga = new TGA();
                    baseAlphaTga.LoadImage(baseName + "(Roughness).png");
                    baseTga.MergeAlpha(baseAlphaTga);
                }
                baseTga.WriteTGA(baseName + ".tga");

                File.SetLastWriteTime(baseName + ".tga", newDate);
                if (File.Exists(baseName + "(Color).png")) File.SetLastWriteTime(baseName + "(Color).png", newDate);
                if (File.Exists(baseName + "(Roughness).png")) File.SetLastWriteTime(baseName + "(Roughness).png", newDate);
            }
            if (File.Exists(baseName + "(Normal).png") && (!File.Exists(baseName + ".tga") || new FileInfo(baseName + "(Normal).png").LastWriteTime != new FileInfo(baseName + "_n.tga").LastWriteTime || (File.Exists(baseName + "(Metallic).png") && new FileInfo(baseName + "(Metallic).png").LastWriteTime != new FileInfo(baseName + "_n.tga").LastWriteTime)))
            {
                var newDate = new FileInfo(baseName + "(Normal).png").LastWriteTime;
                var baseTga = new TGA();
                baseTga.LoadImage(baseName + "(Normal).png");
                if (File.Exists(baseName + "(Metallic).png"))
                {
                    var potNewDate = new FileInfo(baseName + "(Metallic).png").LastWriteTime;
                    if (potNewDate > newDate)
                        newDate = potNewDate;
                    var baseAlphaTga = new TGA();
                    baseAlphaTga.LoadImage(baseName + "(Metallic).png");
                    baseTga.MergeAlpha(baseAlphaTga);
                }
                baseTga.WriteTGA(baseName + "_n.tga");

                File.SetLastWriteTime(baseName + "_n.tga", newDate);
                if (File.Exists(baseName + "(Normal).png")) File.SetLastWriteTime(baseName + "(Normal).png", newDate);
                if (File.Exists(baseName + "(Metallic).png")) File.SetLastWriteTime(baseName + "(Metallic).png", newDate);
            }
            if (File.Exists(baseName + "(Light).png") && (!File.Exists(baseName + "_light.tga") || new FileInfo(baseName + "(Light).png").LastWriteTime != new FileInfo(baseName + "_light.tga").LastWriteTime))
            {
                var newDate = new FileInfo(baseName + "(Light).png").LastWriteTime;
                var baseTga = new TGA();
                baseTga.LoadImage(baseName + "(Light).png");
                baseTga.WriteTGA(baseName + "_light.tga");

                File.SetLastWriteTime(baseName + "_light.tga", newDate);
                if (File.Exists(baseName + "(Light).png")) File.SetLastWriteTime(baseName + "(Light).png", newDate);
            }
        }

        static void WriteMd3(string filename, string name, string matName, Model[] model)
        {
            var md3 = new MD3Model(model, name, matName);
            var buffer = new byte[md3.Size];
            md3.CopyTo(buffer, 0);

            File.WriteAllBytes(filename, buffer);
        }
        static void PrintHelp()
        {
            var readme = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\README.txt";
            if (File.Exists(readme))
            {
                Console.WriteLine(File.ReadAllText(readme));
                return;
            }

            Console.WriteLine(@"Seems like the README is missing (you should have gotten it along with this application).
Best to get it again.");
        }


        static List<string> ProcessModel(string objPath, Model model, string baseName, string relPath, string ModelName, string matPath, bool WriteASE)
        {
            var srcDate = new FileInfo(objPath).LastWriteTime;

            var relPaths = new List<string>();


            //read file and get it into shape
            if(model==null)
            model = OBJ.ObjReadFile(objPath);
            model.TriangulateModel();
            model.HarmonizeFaceElements();

            var locModels = new List<Model>(model.SplitModelInt16());
            for (var k = 0; k < locModels.Count; k++)
            {
                var models = new List<Model>(locModels[k].SplitModel());

                var j = 0;
                do
                {
                    var ext = "_"+models[0].OffsetX + "_" + models[0].OffsetY + "_" + models[0].OffsetZ + "_" + j;
                    //Set internal name for each mesh (max 30 per file)
                    for (var i = 0; i < models.Count && i < 30; i++)
                        models[0].Name = "Model" + i;

                    //Write MD3
                    WriteMd3(baseName + (ext) + ".md3", (ModelName != "" ? ModelName : relPath + "\\" + Path.GetFileNameWithoutExtension(objPath)) + (ext) + ".md3", matPath, models.ToArray());

                    relPaths.Add((ModelName != "" ? ModelName : relPath + "\\" + Path.GetFileNameWithoutExtension(objPath)) + (ext) + ".md3");

                    //Write ASE start
                    if (WriteASE)
                        ASE.AseWriteHeader(baseName + (ext) + ".ase");

                    for (var i = 0; models.Count > 0 && i < 30; i++)
                    {
                        //Write ASE model
                        if (WriteASE)
                            ASE.AseAppendMesh(baseName + (ext) + ".ase", models[0]);
                        models.RemoveAt(0);

                    }

                    //Set last modified so we can recheck next time and skip build if unchanged
                    if (WriteASE)
                        File.SetLastWriteTime(baseName + (ext) + ".ase", srcDate);

                    File.SetLastWriteTime(baseName + (ext) + ".md3", srcDate);

                    j++;
                } while (models.Count > 0);
            }


            return relPaths;

        }


        public static List<string> ProcessObj(string objPath, Model model, string[] args)
        {
            var Args = new List<string>(args);

            //Parse parameters
            var WriteASE = Args.Contains("GenASE");
            var ModelName = Args.Contains("ModelName") && Args.IndexOf("ModelName") < (Args.Count - 1) ? Args[Args.IndexOf("ModelName") + 1] : "";
            var MatName = Args.Contains("MatName") && Args.IndexOf("MatName") < (Args.Count - 1) ? Args[Args.IndexOf("MatName") + 1] : "";

            //Calculate paths
            var filename = Path.GetFullPath(objPath);
            var baseName = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename);

            var quakeDir = filename.LastIndexOf("quake", StringComparison.OrdinalIgnoreCase);
            string relPath = baseName + ".md3";
            if (quakeDir >= 0)
            {
                var quakeDirComplete = filename.IndexOf("\\", quakeDir);
                var quakeSub = filename.IndexOf("\\", quakeDirComplete + 1);
                relPath = filename.Substring(quakeSub + 1);
            }

            relPath = Path.GetDirectoryName(relPath);
            var matPath = MatName == "" ? relPath + "\\" + Path.GetFileNameWithoutExtension(objPath) + ".tga" : MatName;

            //Process textures
            if (!Args.Contains("NoTGA"))
                ProcessTextures(baseName);

            List<string> relPaths;

            relPaths = ProcessModel(objPath, model, baseName, relPath, ModelName, matPath, WriteASE);

            return relPaths;
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            if (File.Exists(args[0]) && Path.GetExtension(args[0]).ToLower() == ".obj")
                ProcessObj(args[0],null, args);
            else if (File.Exists(args[0]) && Path.GetExtension(args[0]).ToLower() == ".map")
                Map.ProcessMap(args[0],args);
        }



    }
}
