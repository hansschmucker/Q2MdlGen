using System;
using System.Collections.Generic;

namespace Q2MdlGen
{
    public class Model
    {
        public string Name = "";
        public List<double[]> Vertices = new List<double[]>();
        public List<double[]> Uvs = new List<double[]>();
        public List<double[]> Normals = new List<double[]>();
        public List<int[][]> Faces = new List<int[][]>();
        public Int32 OffsetX = 0;
        public Int32 OffsetY = 0;
        public Int32 OffsetZ = 0;
        private bool Harmonized = false;

        public Model[] SplitModelInt16()
        {
            var harmonizedModel = this;

            if (!harmonizedModel.Harmonized)
                throw new Exception("Model needs to be harmonized");

            var blocks = new Dictionary<int, Dictionary<int, Dictionary<int, Model>>>();
            var vertexMapping = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>(); //blockX, blockY, blockZ, srcVertexNum = modelVertexNum

            //first sort all polygons NOT crossing block boundaries into the corresponding blocks
            //We use some overlap so that most polygons do not have to split. A max block size is therefore defined not as 32768, but 24576 in each direction, 49153 total size instead of 65535
            /*
             *          -0.5  -0.375  0    0.375   0.5   0.875    1.25   1.375   1.75
             *         [            Block 0         ]               [           Block 2
             *         -32768 -24576   0   24576  32768  40960   65536   90112   98304
             *                               [           Block 1            ]
             */
            //BlockN0 = (P<0?-1:1)*Math.Max(0,(Math.Abs(P)-32768))/57344
            //BlockN1 = (P<0?-1:1)*Math.Max(0,(Math.Abs(P)-24576))/57344

            while (harmonizedModel.Faces.Count > 0)
            {
                int[][] face;
                List<long[]> intersections;
                do
                {
                    face = harmonizedModel.Faces[0];
                    var faceXMin = Math.Min(Math.Min(harmonizedModel.Vertices[face[0][0]][0], harmonizedModel.Vertices[face[1][0]][0]), harmonizedModel.Vertices[face[2][0]][0]);
                    var faceXMax = Math.Max(Math.Max(harmonizedModel.Vertices[face[0][0]][0], harmonizedModel.Vertices[face[1][0]][0]), harmonizedModel.Vertices[face[2][0]][0]);
                    var faceYMin = Math.Min(Math.Min(harmonizedModel.Vertices[face[0][0]][1], harmonizedModel.Vertices[face[1][0]][1]), harmonizedModel.Vertices[face[2][0]][1]);
                    var faceYMax = Math.Max(Math.Max(harmonizedModel.Vertices[face[0][0]][1], harmonizedModel.Vertices[face[1][0]][1]), harmonizedModel.Vertices[face[2][0]][1]);
                    var faceZMin = Math.Min(Math.Min(harmonizedModel.Vertices[face[0][0]][2], harmonizedModel.Vertices[face[1][0]][2]), harmonizedModel.Vertices[face[2][0]][2]);
                    var faceZMax = Math.Max(Math.Max(harmonizedModel.Vertices[face[0][0]][2], harmonizedModel.Vertices[face[1][0]][2]), harmonizedModel.Vertices[face[2][0]][2]);

                    var distX = Math.Abs(faceXMin - faceXMax);
                    var distY = Math.Abs(faceYMin - faceYMax);
                    var distZ = Math.Abs(faceZMin - faceZMax);


                    var faceXMin0 = (faceXMin < 0 ? -1 : 1) * (Int64)Math.Max(0, (Math.Abs(faceXMin) + 32768)) / 57344;
                    var faceXMin1 = (faceXMin < 0 ? -1 : 1) * (Int64)Math.Max(0, (Math.Abs(faceXMin) + 24576)) / 57344;
                    var faceXMax0 = (faceXMax < 0 ? -1 : 1) * (Int64)Math.Max(0, (Math.Abs(faceXMax) + 32768)) / 57344;
                    var faceXMax1 = (faceXMax < 0 ? -1 : 1) * (Int64)Math.Max(0, (Math.Abs(faceXMax) + 24576)) / 57344;
                    var faceYMin0 = (faceYMin < 0 ? -1 : 1) * (Int64)Math.Max(0, (Math.Abs(faceYMin) + 32768)) / 57344;
                    var faceYMin1 = (faceYMin < 0 ? -1 : 1) * (Int64)Math.Max(0, (Math.Abs(faceYMin) + 24576)) / 57344;
                    var faceYMax0 = (faceYMax < 0 ? -1 : 1) * (Int64)Math.Max(0, (Math.Abs(faceYMax) + 32768)) / 57344;
                    var faceYMax1 = (faceYMax < 0 ? -1 : 1) * (Int64)Math.Max(0, (Math.Abs(faceYMax) + 24576)) / 57344;
                    var faceZMin0 = (faceZMin < 0 ? -1 : 1) * (Int64)Math.Max(0, (Math.Abs(faceZMin) + 32768)) / 57344;
                    var faceZMin1 = (faceZMin < 0 ? -1 : 1) * (Int64)Math.Max(0, (Math.Abs(faceZMin) + 24576)) / 57344;
                    var faceZMax0 = (faceZMax < 0 ? -1 : 1) * (Int64)Math.Max(0, (Math.Abs(faceZMax) + 32768)) / 57344;
                    var faceZMax1 = (faceZMax < 0 ? -1 : 1) * (Int64)Math.Max(0, (Math.Abs(faceZMax) + 24576)) / 57344;

                    var possibleMins = new List<long[]>()
                {
                    new long[]{faceXMin0,faceYMin0,faceZMin0},
                    new long[]{faceXMin0,faceYMin0,faceZMin1},
                    new long[]{faceXMin0,faceYMin1,faceZMin0},
                    new long[]{faceXMin0,faceYMin1,faceZMin1},
                    new long[]{faceXMin1,faceYMin0,faceZMin0},
                    new long[]{faceXMin1,faceYMin0,faceZMin1},
                    new long[]{faceXMin1,faceYMin1,faceZMin0},
                    new long[]{faceXMin1,faceYMin1,faceZMin1}
                };
                    var possibleMaxs = new List<long[]>()
                {
                    new long[] { faceXMax0, faceYMax0, faceZMax0 },
                    new long[] { faceXMax0, faceYMax0, faceZMax1 },
                    new long[] { faceXMax0, faceYMax1, faceZMax0 },
                    new long[] { faceXMax0, faceYMax1, faceZMax1 },
                    new long[] { faceXMax1, faceYMax0, faceZMax0 },
                    new long[] { faceXMax1, faceYMax0, faceZMax1 },
                    new long[] { faceXMax1, faceYMax1, faceZMax0 },
                    new long[] { faceXMax1, faceYMax1, faceZMax1 }
                };

                    intersections = new List<long[]>();
                    //find the one cube where min and max where min and max both fit. Sort by distance to 0,0, and take the one with the lowest distance

                    for (var m = 0; m < possibleMins.Count; m++)
                        for (var x = 0; x < possibleMins.Count; x++)
                            if (
                                possibleMins[m][0] == possibleMaxs[m][0]
                                && possibleMins[m][1] == possibleMaxs[m][1]
                                && possibleMins[m][2] == possibleMaxs[m][2]
                                )
                                intersections.Add(possibleMins[m]);

                    if (intersections.Count == 0) {
                        var newVertex = new double[]
                        {
                        (harmonizedModel.Vertices[face[1][0]][0]+harmonizedModel.Vertices[face[2][0]][0])/2,
                        (harmonizedModel.Vertices[face[1][0]][1]+harmonizedModel.Vertices[face[2][0]][1])/2,
                        (harmonizedModel.Vertices[face[1][0]][2]+harmonizedModel.Vertices[face[2][0]][2])/2
                        };

                        var newNormal = new double[]
                        {
                        (harmonizedModel.Normals[face[1][0]][0]+harmonizedModel.Normals[face[2][0]][0])/2,
                        (harmonizedModel.Normals[face[1][0]][1]+harmonizedModel.Normals[face[2][0]][1])/2,
                        (harmonizedModel.Normals[face[1][0]][2]+harmonizedModel.Normals[face[2][0]][2])/2
                        };

                        var newUv = new double[]
                        {
                        (harmonizedModel.Uvs[face[1][0]][0]+harmonizedModel.Uvs[face[2][0]][0])/2,
                        (harmonizedModel.Uvs[face[1][0]][1]+harmonizedModel.Uvs[face[2][0]][1])/2,
                        (harmonizedModel.Uvs[face[1][0]][2]+harmonizedModel.Uvs[face[2][0]][2])/2
                        };
                        harmonizedModel.Vertices.Add(newVertex);
                        harmonizedModel.Normals.Add(newNormal);
                        harmonizedModel.Uvs.Add(newUv);

                        var newIndex = harmonizedModel.Vertices.Count - 1;
                        harmonizedModel.Faces.Add(new int[3][]
                        {
                            new int[]{newIndex,newIndex,newIndex},
                            (int[])face[2].Clone(),
                            (int[])face[0].Clone()
                        });
                        harmonizedModel.Faces[0] = new int[][] { new int[] { newIndex, newIndex, newIndex }, (int[])face[0].Clone(), (int[])face[1].Clone() };
                    }
                } while (intersections.Count == 0);

                intersections.Sort((i0, i1) => (int)(Math.Sqrt(i0[0] * i0[0] + i0[1] * i0[1] + i0[2] * i0[2]) - Math.Sqrt(i1[0] * i1[0] + i1[1] * i1[1] + i1[2] * i1[2])));

                var matchingCube = intersections[0];

                if (!vertexMapping.ContainsKey((int)matchingCube[0]))
                    vertexMapping.Add((int)matchingCube[0], new Dictionary<int, Dictionary<int, Dictionary<int, int>>>());
                if (!vertexMapping[(int)matchingCube[0]].ContainsKey((int)matchingCube[1]))
                    vertexMapping[(int)matchingCube[0]].Add((int)matchingCube[1], new Dictionary<int, Dictionary<int, int>>());
                if (!vertexMapping[(int)matchingCube[0]][(int)matchingCube[1]].ContainsKey((int)matchingCube[2]))
                {
                    vertexMapping[(int)matchingCube[0]][(int)matchingCube[1]].Add((int)matchingCube[2], new Dictionary<int, int>());
                }

                var vertexMappingSlice = vertexMapping[(int)matchingCube[0]][(int)matchingCube[1]][(int)matchingCube[2]];

                if (!blocks.ContainsKey((int)matchingCube[0]))
                    blocks.Add((int)matchingCube[0], new Dictionary<int, Dictionary<int, Model>>());
                if (!blocks[(int)matchingCube[0]].ContainsKey((int)matchingCube[1]))
                    blocks[(int)matchingCube[0]].Add((int)matchingCube[1], new Dictionary<int, Model>());
                if (!blocks[(int)matchingCube[0]][(int)matchingCube[1]].ContainsKey((int)matchingCube[2]))
                {
                    blocks[(int)matchingCube[0]][(int)matchingCube[1]].Add((int)matchingCube[2], new Model()
                    {
                        OffsetX = (int)matchingCube[0] * 57344,
                        OffsetY = (int)matchingCube[1] * 57344,
                        OffsetZ = (int)matchingCube[2] * 57344,
                        Harmonized = this.Harmonized,
                        Name = this.Name
                    });

                }
                var slice = blocks[(int)matchingCube[0]][(int)matchingCube[1]][(int)matchingCube[2]];

                for(var b=0;b<3;b++)
                    if (!vertexMappingSlice.ContainsKey(face[b][0]) && face[b][0] >= 0)
                    {
                        vertexMappingSlice.Add(face[b][0], slice.Vertices.Count);
                        var sX = harmonizedModel.Vertices[face[b][0]][0];
                        var sY = harmonizedModel.Vertices[face[b][0]][1];
                        var sZ = harmonizedModel.Vertices[face[b][0]][2];
                        var mX = sX - slice.OffsetX;
                        var mY = sY - slice.OffsetY;
                        var mZ = sZ - slice.OffsetZ;
                        slice.Vertices.Add(new double[]{mX,mY,mZ});

                        slice.Uvs.Add(harmonizedModel.Uvs[face[b][1]]);
                        slice.Normals.Add(harmonizedModel.Normals[face[b][2]]);
                        if(Math.Abs(mX)>32768 || Math.Abs(mY) > 32768|| Math.Abs(mZ) > 32768)
                        {
                            throw new Exception("Vertex out of range, really shouldn't happen.  "+mX+" "+mY+" "+mZ);
                        }
                    }




                var v0=new int[3] {
                            vertexMappingSlice[face[0][0]],
                            vertexMappingSlice[face[0][1]],
                            vertexMappingSlice[face[0][2]] };

                var v1 = new int[3] {
                            vertexMappingSlice[face[1][0]],
                            vertexMappingSlice[face[1][1]],
                            vertexMappingSlice[face[1][2]] };

                var v2 = new int[3] {
                            vertexMappingSlice[face[2][0]],
                            vertexMappingSlice[face[2][1]],
                            vertexMappingSlice[face[2][2]] };

                slice.Faces.Add(new int[3][] {v0,v1,v2});
                harmonizedModel.Faces.RemoveAt(0);
            }

            
            var r = new List<Model>();
            foreach (var i in blocks)
                foreach (var j in i.Value)
                    foreach (var k in j.Value)
                        r.Add(k.Value);

            return r.ToArray();
        }

        public TriangleSoup GetTriangleSoup()
        {
            this.TriangulateModel();
            var r = new TriangleSoup
            {
                Triangles = new List<double[]>()
            };
            for (var i = 0; i < Faces.Count; i++)
                r.Triangles.Add(new double[] {
                    Vertices[Faces[i][0][0]][1], Vertices[Faces[i][0][0]][0], Vertices[Faces[i][0][0]][2],
                    Vertices[Faces[i][1][0]][1], Vertices[Faces[i][1][0]][0], Vertices[Faces[i][1][0]][2],
                    Vertices[Faces[i][2][0]][1], Vertices[Faces[i][2][0]][0], Vertices[Faces[i][2][0]][2]
                });

            return r;
        }

        public Model[] SplitModel()
        {
            var harmonizedModel = this;

            if (!harmonizedModel.Harmonized)
                throw new Exception("Model needs to be harmonized");

            var models = new List<Model>();

            while (harmonizedModel.Faces.Count > 0)
            {
                var vertexMapping = new Dictionary<int, int>() { { -1, -1 } };
                var slice = new Model()
                {
                    Harmonized = this.Harmonized,
                    OffsetX = this.OffsetX,
                    OffsetY=this.OffsetY,
                    OffsetZ=this.OffsetZ,
                    Name=this.Name

                };
                while (slice.Faces.Count < 8192 && slice.Vertices.Count < 4094 && harmonizedModel.Faces.Count > 0)
                {
                    if (!vertexMapping.ContainsKey(harmonizedModel.Faces[0][0][0]) && harmonizedModel.Faces[0][0][0] >= 0)
                    {
                        vertexMapping.Add(harmonizedModel.Faces[0][0][0], slice.Vertices.Count);
                        slice.Vertices.Add(harmonizedModel.Vertices[harmonizedModel.Faces[0][0][0]]);
                        slice.Uvs.Add(harmonizedModel.Uvs[harmonizedModel.Faces[0][0][1]]);
                        slice.Normals.Add(harmonizedModel.Normals[harmonizedModel.Faces[0][0][2]]);
                    }
                    if (!vertexMapping.ContainsKey(harmonizedModel.Faces[0][1][0]) && harmonizedModel.Faces[0][1][0] >= 0)
                    {
                        vertexMapping.Add(harmonizedModel.Faces[0][1][0], slice.Vertices.Count);
                        slice.Vertices.Add(harmonizedModel.Vertices[harmonizedModel.Faces[0][1][0]]);
                        slice.Uvs.Add(harmonizedModel.Uvs[harmonizedModel.Faces[0][1][1]]);
                        slice.Normals.Add(harmonizedModel.Normals[harmonizedModel.Faces[0][1][2]]);
                    }
                    if (!vertexMapping.ContainsKey(harmonizedModel.Faces[0][2][0]) && harmonizedModel.Faces[0][2][0] >= 0)
                    {
                        vertexMapping.Add(harmonizedModel.Faces[0][2][0], slice.Vertices.Count);
                        slice.Vertices.Add(harmonizedModel.Vertices[harmonizedModel.Faces[0][2][0]]);
                        slice.Uvs.Add(harmonizedModel.Uvs[harmonizedModel.Faces[0][2][1]]);
                        slice.Normals.Add(harmonizedModel.Normals[harmonizedModel.Faces[0][2][2]]);
                    }

                    slice.Faces.Add(new int[3][] {
                        new int[3] { vertexMapping[harmonizedModel.Faces[0][0][0]], vertexMapping[harmonizedModel.Faces[0][0][1]], vertexMapping[harmonizedModel.Faces[0][0][2]] },
                        new int[3] { vertexMapping[harmonizedModel.Faces[0][1][0]], vertexMapping[harmonizedModel.Faces[0][1][1]], vertexMapping[harmonizedModel.Faces[0][1][2]] },
                        new int[3] { vertexMapping[harmonizedModel.Faces[0][2][0]], vertexMapping[harmonizedModel.Faces[0][2][1]], vertexMapping[harmonizedModel.Faces[0][2][2]] }
                    });

                    harmonizedModel.Faces.RemoveAt(0);
                }

                if (slice.Faces.Count > 0)
                    models.Add(slice);
            }

            return models.ToArray();
        }

        public void TriangulateModel()
        {
            var model = this;
            for (var i = 0; i < model.Faces.Count; i++)
            {
                if (model.Faces[i].Length < 4)
                    continue;

                if (model.Faces[i].Length == 4)
                {
                    model.Faces.Add(new int[3][] { new int[] { model.Faces[i][2][0], model.Faces[i][2][1], model.Faces[i][2][2] }, new int[] { model.Faces[i][3][0], model.Faces[i][3][1], model.Faces[i][3][2] }, new int[] { model.Faces[i][0][0], model.Faces[i][0][1], model.Faces[i][0][2] } });
                    model.Faces[i] = new int[][] { model.Faces[i][0], model.Faces[i][1], model.Faces[i][2] };
                    return;
                }

                throw new Exception("5+ sided polygon encountered. Triangulation not yet implemented exceeding quads.");
            }
        }

        //makes each combination of Position/Normal/UV unique so that only one index is needed
        public void HarmonizeFaceElements()
        {
            var model = this;

            List<double[]> Vertices = new List<double[]>();
            List<double[]> Uvs = new List<double[]>();
            List<double[]> Normals = new List<double[]>();
            List<int[][]> Faces = new List<int[][]>();


            var vertexUvNormalDict = new Dictionary<Int32, Dictionary<Int32, Dictionary<Int32, Int32>>>();

            for (var i = 0; i < model.Faces.Count; i++)
            {
                var newFace = new int[3];

                for (var j = 0; j < 3; j++)
                {

                    if (!vertexUvNormalDict.ContainsKey(model.Faces[i][j][0]))
                        vertexUvNormalDict.Add(model.Faces[i][j][0], new Dictionary<Int32, Dictionary<Int32, Int32>>());

                    if (!vertexUvNormalDict[model.Faces[i][j][0]].ContainsKey(model.Faces[i][j][1]))
                        vertexUvNormalDict[model.Faces[i][j][0]].Add(model.Faces[i][j][1], new Dictionary<Int32, Int32>());

                    if (!vertexUvNormalDict[model.Faces[i][j][0]][model.Faces[i][j][1]].ContainsKey(model.Faces[i][j][2]))
                    {
                        vertexUvNormalDict[model.Faces[i][j][0]][model.Faces[i][j][1]].Add(model.Faces[i][j][2], Vertices.Count);
                        Vertices.Add(model.Vertices[model.Faces[i][j][0]]);
                        Uvs.Add(model.Uvs[model.Faces[i][j][1]]);
                        Normals.Add(model.Normals[model.Faces[i][j][2]]);
                    }

                    newFace[j] = vertexUvNormalDict[model.Faces[i][j][0]][model.Faces[i][j][1]][model.Faces[i][j][2]];
                }
                Faces.Add(new int[][] { new int[] { newFace[0], newFace[0], newFace[0] }, new int[] { newFace[1], newFace[1], newFace[1] }, new int[] { newFace[2], newFace[2], newFace[2] } });
            }

            model.Vertices = Vertices;
            model.Uvs = Uvs;
            model.Normals = Normals;
            model.Faces = Faces;
            model.Harmonized = true;
        }

    }
}
