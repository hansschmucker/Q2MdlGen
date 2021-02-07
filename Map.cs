using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace Q2MdlGen
{
    class Map
    {
        static string ProcessSpawn(string mapPath,string[] args, string path, string origin,List<string> blocks)
        {
            mapPath = Path.GetFullPath(mapPath);
            var absPath = "";
            var filename = Path.GetFullPath(mapPath);
            var quakeDir = filename.LastIndexOf("quake", StringComparison.OrdinalIgnoreCase);
            if (quakeDir >= 0)
            {
                var quakeDirComplete = filename.IndexOf("\\", quakeDir);
                var quakeSub = filename.IndexOf("\\", quakeDirComplete + 1);
                absPath = mapPath.Substring(0, quakeSub);
            }

            var entities = new List<string>();
            var originS = origin.Split(' ');
            var originD = new double[] { double.Parse(originS[0]), double.Parse(originS[1]), double.Parse(originS[2]) };


            var model = OBJ.ObjReadFile(absPath + "\\" + path.Replace('/', '\\'));

            var soup = model.GetTriangleSoup();
            var floors = soup.FindFloors();
            var r= 256;
            soup.Subdivide(r*16);
            var clumps = soup.Clump(r*64,r*64,r*64);
            
            var blockTemplate =
@"{
( -1072 y0 -1024 ) ( -1024 y0 -1024 ) ( -1024 y0 -1088 ) clip 0 0 0 1 1
( -1072 y1 -1088 ) ( -1024 y1 -1088 ) ( -1024 y1 -1024 ) clip 0 0 0 1 1
( x0 -1752 -1024 ) ( x0 -1752 -1088 ) ( x0 -1672 -1088 ) clip 0 0 0 1 1
( x1 -1672 -1024 ) ( x1 -1672 -1088 ) ( x1 -1752 -1088 ) clip 0 0 0 1 1
( -1024 -1752 z1 ) ( -1072 -1752 z1 ) ( -1072 -1672 z1 ) clip 0 0 0 1 1
( -1024 -1672 z0 ) ( -1072 -1672 z0 ) ( -1072 -1752 z0 ) clip 0 0 0 1 1
}";

            for (var i = 0; i < clumps.Count; i++)
                blocks.Add(
                    blockTemplate
                    .Replace("x0", ((int)originD[0] - (clumps[i][0]) * r - 128).ToString())
                    .Replace("x1", ((int)originD[0] - (clumps[i][0]) * r + 128).ToString())
                    .Replace("y0", ((int)originD[1] + (clumps[i][1]) * r - 128).ToString())
                    .Replace("y1", ((int)originD[1] + (clumps[i][1]) * r + 128).ToString())
                    .Replace("z0", ((int)originD[2] + (clumps[i][2]) * r +   0).ToString())
                    .Replace("z1", ((int)originD[2] + (clumps[i][2]) * r + 128).ToString())
                );
            
            /*
            var wedgeTemplate=//z0 z1 as before, but there are now x0,y0,x1,y1,x2,y2 forming 3 faces
@"{
( x0 y0 z1 ) ( x2 y2 z1 ) ( y0 y1 z1 ) pink y0 0 y0 1.000000 1.000000
( x2 y2 z0 ) ( x0 y0 z0 ) ( y0 y1 z0 ) pink y0 0 y0 1.000000 1.000000
( x0 y0 z1 ) ( x0 y0 z0 ) ( x2 y2 z0 ) pink y0 0 y0 1.000000 1.000000
( x0 y0 z0 ) ( x0 y0 z1 ) ( y0 y1 z1 ) pink y0 0 y0 1.000000 1.000000
( y0 y1 z0 ) ( y0 y1 z1 ) ( x2 y2 z1 ) pink y0 0 y0 1.000000 1.000000
}";
            foreach(var z in floors)
                for (var i = 0; i < z.Value.Count; i++)
                    blocks.Add(
                        wedgeTemplate
                        .Replace("x0", ((int)originD[0] - (z.Value[i][4]) / 64).ToString())
                        .Replace("x1", ((int)originD[0] - (z.Value[i][2]) / 64).ToString())
                        .Replace("x2", ((int)originD[0] - (z.Value[i][0]) / 64).ToString())
                        .Replace("y0", ((int)originD[0] - (z.Value[i][5]) / 64).ToString())
                        .Replace("y1", ((int)originD[0] - (z.Value[i][3]) / 64).ToString())
                        .Replace("y2", ((int)originD[0] - (z.Value[i][1]) / 64).ToString())
                        .Replace("z0", ((int)originD[2] + z.Key/64 + 8).ToString())
                        .Replace("z1", ((int)originD[2] + z.Key/64).ToString())
                    );*/

            var models = Program.ProcessObj(absPath+"\\"+path.Replace('/','\\'),model, args);

            
            for(var i = 0; i < models.Count; i++)
            {
                var originRS = Path.GetFileNameWithoutExtension(models[i]).Split('_');
                var originRD = new double[] { double.Parse(originRS[originRS.Length-4])/64, double.Parse(originRS[originRS.Length - 3])/64, double.Parse(originRS[originRS.Length - 2])/64 };
                var originFD = new double[] { originD[0] - originRD[1], originD[1] + originRD[0], originD[2] + originRD[2] };
                
                entities.Add(@"{
""classname"" ""model_spawn""
""model"" """ + models[i].Replace('\\','/') + @"""
""angles"" ""90""
""origin"" """ + Math.Round(originFD[0])+" "+Math.Round(originFD[1])+" "+Math.Round(originFD[2]) + @"""
}");
            }

            return String.Join("\r\n", entities);

        }
        public static void ProcessMap(string path, string[] args)
        {
            var file = File.ReadAllText(path, Encoding.ASCII);
            var blocks = new List<string>();

            file = Regex.Replace(file, @"\{[\s\n]*""classname""\s+""model_spawn""[\s\n]+""model""\s+""([^""]+)""(.*?)""origin""\s+""([^""]+)""(.*?)\}", (m) =>
            Path.GetExtension(m.Groups[1].Value).ToLower() != ".obj" ? m.Value : ProcessSpawn(path,args,m.Groups[1].Value, m.Groups[3].Value,blocks)
            , RegexOptions.Singleline);

            file = Regex.Replace(file, @"(\{[\s\n]*""classname""\s+""worldspawn"".*?)\{", (m) => m.Groups[1].Value + String.Join("\r\n", blocks) + "\r\n{", RegexOptions.Singleline);
            File.WriteAllText(path, file, Encoding.ASCII);
        }
    }
}
