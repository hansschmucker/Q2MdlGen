using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q2MdlGen
{
    public class TriangleSoup
    {
        /**
         * Very simple class to hold raw triangles, or rather their vertex positions. Rather than having a dedicated vertex list, all positions are shoved directly into the Triangles, resulting in each face being a single 9-entry array.
         */
        public TriangleSoup()
        {

        }

        /*
         * Subdivides all triangles until none exceeds the given length. Triangles are split one by one until all triangles have the desired size.
         */
        public void Subdivide(double MaxLength)
        {
            

            for (var i = 0; i < Triangles.Count; i++)
            {
                bool reTest;
                do
                {
                    reTest = false;
                    for (var n = 0; n < 3 && !reTest; n++)
                    {
                        var dX = (Triangles[i][((n + 0) % 3) * 3 + 0])- (Triangles[i][((n + 1) % 3) * 3 + 0]);
                        var dY = (Triangles[i][((n + 0) % 3) * 3 + 1])- (Triangles[i][((n + 1) % 3) * 3 + 1]);
                        var dZ = (Triangles[i][((n + 0) % 3) * 3 + 2])- (Triangles[i][((n + 1) % 3) * 3 + 2]);
                        var len=Math.Sqrt(dX*dX + dY*dY + dZ*dZ);
                        if (len > MaxLength)
                        {
                            var newX = ((Triangles[i][((n + 0) % 3) * 3 + 0]) + (Triangles[i][((n + 1) % 3) * 3 + 0])) / 2;
                            var newY = ((Triangles[i][((n + 0) % 3) * 3 + 1]) + (Triangles[i][((n + 1) % 3) * 3 + 1])) / 2;
                            var newZ = ((Triangles[i][((n + 0) % 3) * 3 + 2]) + (Triangles[i][((n + 1) % 3) * 3 + 2])) / 2;
                            Triangles[i] = new double[] {
                                Triangles[i][((n + 2) % 3) * 3 + 0], Triangles[i][((n + 2) % 3) * 3 + 1], Triangles[i][((n + 2) % 3) * 3 + 2],
                                Triangles[i][((n + 0) % 3) * 3 + 0], Triangles[i][((n + 0) % 3) * 3 + 1], Triangles[i][((n + 0) % 3) * 3 + 2],
                                newX,newY,newZ
                            };
                            Triangles.Add(new double[] {
                                newX,newY,newZ,
                                Triangles[i][((n + 1) % 3) * 3 + 0], Triangles[i][((n + 1) % 3) * 3 + 1], Triangles[i][((n + 1) % 3) * 3 + 2],
                                Triangles[i][((n + 2) % 3) * 3 + 0], Triangles[i][((n + 2) % 3) * 3 + 1], Triangles[i][((n + 2) % 3) * 3 + 2]
                            });
                            reTest = true;
                        }
                    }
                } while (reTest);
            }
        }

        /**
         * Groups all vertices down into a three dimensional int array [x/y/z] where every block is set to true if there is a vertex in it.
         */
        public List<int[]> Clump(int sizeX,int sizeY,int sizeZ)
        {
            var clumps = new Dictionary<int, Dictionary<int, Dictionary<int, bool>>>();

            for(var i = 0; i < Triangles.Count; i++)
            {
                for (var n = 0; n < 3; n++)
                {
                    var x = (int)Math.Round(Triangles[i][((n + 0) % 3) * 3 + 0] / sizeX);
                    var y = (int)Math.Round(Triangles[i][((n + 0) % 3) * 3 + 1] / sizeY);
                    var z = (int)Math.Round(Triangles[i][((n + 0) % 3) * 3 + 2] / sizeZ);

                    if (!clumps.ContainsKey(x))
                        clumps.Add(x, new Dictionary<int, Dictionary<int, bool>>());

                    if (!clumps[x].ContainsKey(y))
                        clumps[x].Add(y, new Dictionary<int, bool>());

                    if (!clumps[x][y].ContainsKey(z))
                        clumps[x][y].Add(z,true);
                }
            }


            var r = new List<int[]>();
            foreach (var x in clumps)
                foreach (var y in x.Value)
                    foreach (var z in y.Value)
                        r.Add(new int[] { x.Key, y.Key, z.Key });

            return r;
        }

        public Dictionary<double, List<double[]>> FindFloors()
        {
            var r = new Dictionary<double,List<double[]>>();
            
            for(var i = 0; i < Triangles.Count; i++)
            {
                if(Math.Abs(Triangles[i][2]- Triangles[i][5])==0 && Math.Abs(Triangles[i][5] - Triangles[i][8]) == 0 && Math.Abs(Triangles[i][8] - Triangles[i][2]) == 0)
                {
                    var avg = ((Triangles[i][2]) + Triangles[i][5] + Triangles[i][8]) / 3;
                    if (!r.ContainsKey(avg))
                    {
                        r.Add(avg, new List<double[]>());
                    }
                    r[avg].Add(new double[] { Triangles[i][0], Triangles[i][1], Triangles[i][3], Triangles[i][4], Triangles[i][6], Triangles[i][7] });
                }
            }

            return r;

        }

        public List<double[]> Triangles = null;
    }
}
