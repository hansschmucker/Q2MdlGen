using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Q2MdlGen
{
    public static class OBJ
    {
        //Reads an OBJ file and returns a model
        public static Model ObjReadFile(string filename)
        {
            var Vertices = new List<double[]>();
            var Uvs = new List<double[]>();
            var Normals = new List<double[]>();
            var Faces = new List<int[][]>();
            var inFile = File.ReadAllText(filename, Encoding.UTF8).Replace("\r", "").Split('\n');

            var findVertex = new Regex(@"^v\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)(?:\s+([^\s]+))?$");
            var findFace = new Regex(@"^f\s+(.*)$");
            var findNormal = new Regex(@"^vn\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)$");
            var findUv = new Regex(@"^vt\s+([^\s]+)(?:\s+([^\s]+)(?:\s+([^\s]+))?)?$");
            var findFacePoint = new Regex(@"([0-9]+)(?:/([0-9]*)(?:/([0-9]+))?)?");
            for (var i = 0; i < inFile.Length; i++)
            {
                var foundVertex = findVertex.Match(inFile[i]);
                if (foundVertex.Success)
                {
                    var vert = new double[4] { 0, 0, 0, 1.0 };
                    if (foundVertex.Groups[1].Success && !String.IsNullOrWhiteSpace(foundVertex.Groups[1].Value))
                        vert[0] = double.Parse(foundVertex.Groups[1].Value, CultureInfo.InvariantCulture)*64;
                    if (foundVertex.Groups[2].Success && !String.IsNullOrWhiteSpace(foundVertex.Groups[2].Value))
                        vert[1] = double.Parse(foundVertex.Groups[2].Value, CultureInfo.InvariantCulture)*64;
                    if (foundVertex.Groups[3].Success && !String.IsNullOrWhiteSpace(foundVertex.Groups[3].Value))
                        vert[2] = double.Parse(foundVertex.Groups[3].Value, CultureInfo.InvariantCulture)*64;
                    if (foundVertex.Groups[4].Success && !String.IsNullOrWhiteSpace(foundVertex.Groups[4].Value))
                        vert[3] = double.Parse(foundVertex.Groups[4].Value, CultureInfo.InvariantCulture);

                    Vertices.Add(vert);
                    continue;
                }
                var foundFace = findFace.Match(inFile[i]);
                if (foundFace.Success)
                {

                    var facePoints = findFacePoint.Matches(foundFace.Groups[1].Value);
                    var face = new int[facePoints.Count][];
                    for (var j = 0; j < facePoints.Count; j++)
                    {
                        face[j] = new int[3] {
                            facePoints[j].Groups[1].Success && !String.IsNullOrWhiteSpace(facePoints[j].Groups[1].Value) ? int.Parse(facePoints[j].Groups[1].Value)-1 : -1,
                            facePoints[j].Groups[2].Success && !String.IsNullOrWhiteSpace(facePoints[j].Groups[2].Value) ? int.Parse(facePoints[j].Groups[2].Value)-1 : -1,
                            facePoints[j].Groups[3].Success && !String.IsNullOrWhiteSpace(facePoints[j].Groups[3].Value) ? int.Parse(facePoints[j].Groups[3].Value)-1 : -1
                        };
                    }
                    Faces.Add(face);
                    continue;
                }
                var foundNormal = findNormal.Match(inFile[i]);
                if (foundNormal.Success)
                {
                    var norm = new double[3] { 0, 0, 0 };
                    if (foundNormal.Groups[1].Success && !String.IsNullOrWhiteSpace(foundNormal.Groups[1].Value))
                        norm[0] = double.Parse(foundNormal.Groups[1].Value, CultureInfo.InvariantCulture);
                    if (foundNormal.Groups[2].Success && !String.IsNullOrWhiteSpace(foundNormal.Groups[2].Value))
                        norm[2] = -double.Parse(foundNormal.Groups[2].Value, CultureInfo.InvariantCulture);
                    if (foundNormal.Groups[3].Success && !String.IsNullOrWhiteSpace(foundNormal.Groups[3].Value))
                        norm[1] = double.Parse(foundNormal.Groups[3].Value, CultureInfo.InvariantCulture);

                    Normals.Add(norm);
                    continue;
                }

                var foundUv = findUv.Match(inFile[i]);
                if (foundUv.Success)
                {
                    var uv = new double[3] { 0, 0, 1.0 };
                    if (foundUv.Groups[1].Success && !String.IsNullOrWhiteSpace(foundUv.Groups[1].Value))
                        uv[0] = double.Parse(foundUv.Groups[1].Value, CultureInfo.InvariantCulture);
                    if (foundUv.Groups[2].Success && !String.IsNullOrWhiteSpace(foundUv.Groups[2].Value))
                        uv[1] = double.Parse(foundUv.Groups[2].Value, CultureInfo.InvariantCulture);
                    if (foundUv.Groups[3].Success && !String.IsNullOrWhiteSpace(foundUv.Groups[3].Value))
                        uv[2] = double.Parse(foundUv.Groups[3].Value, CultureInfo.InvariantCulture);

                    Uvs.Add(uv);
                    continue;
                }

            }
            var nullUv = -1;
            for (var i = 0; i < Faces.Count; i++)
            {
                if (Faces[i][0][1] < 0)
                {
                    if (nullUv < 0)
                    {
                        nullUv = Uvs.Count;
                        Uvs.Add(new double[] { 0, 0, 1 });
                    }
                    Faces[i][0][1] = nullUv;
                }
                if (Faces[i][1][1] < 0)
                {
                    if (nullUv < 0)
                    {
                        nullUv = Uvs.Count;
                        Uvs.Add(new double[] { 0, 0, 1 });
                    }
                    Faces[i][1][1] = nullUv;
                }
                if (Faces[i][2][1] < 0)
                {
                    if (nullUv < 0)
                    {
                        nullUv = Uvs.Count;
                        Uvs.Add(new double[] { 0, 0, 1 });
                    }
                    Faces[i][2][1] = nullUv;
                }
            }

            return new Model() { Faces = Faces, Normals = Normals, Uvs = Uvs, Vertices = Vertices };
        }

    }
}
