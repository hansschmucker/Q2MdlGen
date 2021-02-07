using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Q2MdlGen
{

    class MD3Vertex
    {
        public MD3Vertex(double[] vertex, double[] normal)
        {
            var sY = Math.Round(vertex[0]-1);
            var sX = Math.Round(-vertex[1]-1);
            var sZ = Math.Round(vertex[2]-1);
            Y = (Int16)sY;
            X = (Int16)sX;
            Z = (Int16)sZ;
            if(sY!=Y || sX!=X|| sZ != Z)
            {
                throw new Exception("Vertex out of range. This really shouldn't happen.");
            }

            if (normal[0] == 0 && normal[1] == 0)
                NORMAL = (Int16)(normal[2] > 0 ? 0 : 128);
            else
            {
                var b0 = (Int32)(Math.Atan2(normal[0], -normal[1]) * 255 / (2 * Math.PI));
                var b1 = (Int32)(Math.Acos(normal[2]) * 255 / (2 * Math.PI));
                NORMAL = (Int16)(((b0 & 255) << 8) | (b1 & 255));
            }
        }

        readonly Int16 X = 0;
        readonly Int16 Y = 0;
        readonly Int16 Z = 0;
        readonly Int16 NORMAL = 0;

        public byte[] GetBytes()
        {
            return System.BitConverter.GetBytes(X).Concat(System.BitConverter.GetBytes(Y)).Concat(System.BitConverter.GetBytes(Z)).Concat(System.BitConverter.GetBytes(NORMAL)).ToArray();
        }


        public Int32 Size=8;
        public bool SizeIsStatic = true;

        public Int32 CopyTo(byte[] destination, int offset)
        {
            System.BitConverter.GetBytes(X).CopyTo(destination, offset + 0);
            System.BitConverter.GetBytes(Y).CopyTo(destination, offset + 2);
            System.BitConverter.GetBytes(Z).CopyTo(destination, offset + 4);
            System.BitConverter.GetBytes(NORMAL).CopyTo(destination, offset + 6);

            return offset+Size;
        }
    }

    class MD3TexCoord
    {
        public MD3TexCoord(double[] uv)
        {
            ST = new float[2] { (float)uv[0], (float)(1-uv[1]) };
        }
        readonly float[] ST;
        public byte[] GetBytes()
        {
            return System.BitConverter.GetBytes(ST[0]).Concat(System.BitConverter.GetBytes(ST[1])).ToArray();
        }

        public Int32 Size = 8;
        public bool SizeIsStatic = true;

        public Int32 CopyTo(byte[] destination, int offset)
        {
            System.BitConverter.GetBytes(ST[0]).CopyTo(destination, offset + 0);
            System.BitConverter.GetBytes(ST[1]).CopyTo(destination, offset + 4);

            return offset + Size;
        }
    }


    class MD3Triangle
    {
        public MD3Triangle(Int32[] indexes)
        {
            INDEXES = indexes;
        }
        readonly Int32[] INDEXES;
        public byte[] GetBytes()
        {
            return System.BitConverter.GetBytes(INDEXES[2]).Concat(System.BitConverter.GetBytes(INDEXES[1])).Concat(System.BitConverter.GetBytes(INDEXES[0])).ToArray();
        }

        public Int32 Size = 12;
        public bool SizeIsStatic = true;

        public Int32 CopyTo(byte[] destination, int offset)
        {
            System.BitConverter.GetBytes(INDEXES[2]).CopyTo(destination, offset + 0);
            System.BitConverter.GetBytes(INDEXES[1]).CopyTo(destination, offset + 4);
            System.BitConverter.GetBytes(INDEXES[0]).CopyTo(destination, offset + 8);

            return offset + Size;
        }
    }

    class MD3Shader
    {
        public MD3Shader(string name, Int32 index)
        {
            Array.Copy(Encoding.ASCII.GetBytes(name), 0, NAME, 0, name.Length);
            SHADER_INDEX = index;
        }
        readonly byte[] NAME = new byte[64];
        readonly Int32 SHADER_INDEX;

        public byte[] GetBytes()
        {
            return NAME.Concat(System.BitConverter.GetBytes(SHADER_INDEX)).ToArray();
        }

        public Int32 Size = 68;
        public bool SizeIsStatic = true;

        public Int32 CopyTo(byte[] destination, int offset)
        {
            NAME.CopyTo(destination, offset + 0);
            System.BitConverter.GetBytes(SHADER_INDEX).CopyTo(destination, offset + 64);

            return offset + Size;
        }
    }

    class MD3Frame
    {
        public MD3Frame()
        {
        }
        public MD3Frame(double[] minBounds, double[] maxBounds, double[] localOrigin, double radius, string name)
        {
            MIN_BOUNDS[0] = (float)minBounds[0];
            MIN_BOUNDS[1] = (float)minBounds[1];
            MIN_BOUNDS[2] = (float)minBounds[2];

            MAX_BOUNDS[0] = (float)maxBounds[0];
            MAX_BOUNDS[1] = (float)maxBounds[1];
            MAX_BOUNDS[2] = (float)maxBounds[2];

            LOCAL_ORIGIN[0] = (float)localOrigin[0];
            LOCAL_ORIGIN[1] = (float)localOrigin[1];
            LOCAL_ORIGIN[2] = (float)localOrigin[2];

            RADIUS = (float)radius;

            Array.Copy(Encoding.ASCII.GetBytes(name), 0, NAME, 0, name.Length);
        }

        readonly float[] MIN_BOUNDS = new float[3];
        readonly float[] MAX_BOUNDS = new float[3];
        readonly float[] LOCAL_ORIGIN = new float[3];
        readonly float RADIUS;
        readonly byte[] NAME = new byte[16];

        public byte[] GetBytes()
        {
            return
                System.BitConverter.GetBytes(MIN_BOUNDS[0]).Concat(System.BitConverter.GetBytes(MIN_BOUNDS[1])).Concat(System.BitConverter.GetBytes(MIN_BOUNDS[2]))
                .Concat(System.BitConverter.GetBytes(MAX_BOUNDS[0])).Concat(System.BitConverter.GetBytes(MAX_BOUNDS[1])).Concat(System.BitConverter.GetBytes(MAX_BOUNDS[2]))
                .Concat(System.BitConverter.GetBytes(LOCAL_ORIGIN[0])).Concat(System.BitConverter.GetBytes(LOCAL_ORIGIN[1])).Concat(System.BitConverter.GetBytes(LOCAL_ORIGIN[2]))
                .Concat(System.BitConverter.GetBytes(RADIUS))
                .Concat(NAME)
                .ToArray();
        }

        public Int32 Size = 56;
        public bool SizeIsStatic = true;

        public Int32 CopyTo(byte[] destination, int offset)
        {
            System.BitConverter.GetBytes(MIN_BOUNDS[0]).CopyTo(destination, offset + 0);
            System.BitConverter.GetBytes(MIN_BOUNDS[1]).CopyTo(destination, offset + 4);
            System.BitConverter.GetBytes(MIN_BOUNDS[2]).CopyTo(destination, offset + 8);
            System.BitConverter.GetBytes(MAX_BOUNDS[0]).CopyTo(destination, offset + 12);
            System.BitConverter.GetBytes(MAX_BOUNDS[1]).CopyTo(destination, offset + 16);
            System.BitConverter.GetBytes(MAX_BOUNDS[2]).CopyTo(destination, offset + 20);
            System.BitConverter.GetBytes(LOCAL_ORIGIN[0]).CopyTo(destination, offset + 24);
            System.BitConverter.GetBytes(LOCAL_ORIGIN[1]).CopyTo(destination, offset + 28);
            System.BitConverter.GetBytes(LOCAL_ORIGIN[2]).CopyTo(destination, offset + 32);
            System.BitConverter.GetBytes(RADIUS).CopyTo(destination, offset + 36);
            NAME.CopyTo(destination, offset + 40);

            return offset + Size;
        }
    }

    class MD3Tag
    {
        public MD3Tag(string name, double[] origin, double[][] axis)
        {
            Array.Copy(Encoding.ASCII.GetBytes(name), 0, NAME, 0, name.Length);
            ORIGIN[0] = (float)origin[0];
            ORIGIN[1] = (float)origin[1];
            ORIGIN[2] = (float)origin[2];

            AXIS[0] = new float[3] { (float)axis[0][0], (float)axis[0][1], (float)axis[0][2] };
            AXIS[1] = new float[3] { (float)axis[1][0], (float)axis[1][1], (float)axis[1][2] };
            AXIS[2] = new float[3] { (float)axis[2][0], (float)axis[2][1], (float)axis[2][2] };
        }
        readonly byte[] NAME = new byte[64];
        readonly float[] ORIGIN = new float[3];
        readonly float[][] AXIS = new float[3][];

        public byte[] GetBytes()
        {
            return
                NAME
                .Concat(System.BitConverter.GetBytes(ORIGIN[0])).Concat(System.BitConverter.GetBytes(ORIGIN[1])).Concat(System.BitConverter.GetBytes(ORIGIN[2]))
                .Concat(System.BitConverter.GetBytes(AXIS[0][0])).Concat(System.BitConverter.GetBytes(AXIS[0][1])).Concat(System.BitConverter.GetBytes(AXIS[0][2]))
                .Concat(System.BitConverter.GetBytes(AXIS[1][0])).Concat(System.BitConverter.GetBytes(AXIS[1][1])).Concat(System.BitConverter.GetBytes(AXIS[1][2]))
                .Concat(System.BitConverter.GetBytes(AXIS[2][0])).Concat(System.BitConverter.GetBytes(AXIS[2][1])).Concat(System.BitConverter.GetBytes(AXIS[2][2]))
                .ToArray();
        }

        public Int32 Size = 112;
        public bool SizeIsStatic = true;

        public Int32 CopyTo(byte[] destination, int offset)
        {
            NAME.CopyTo(destination, offset + 0);
            System.BitConverter.GetBytes(ORIGIN[0]).CopyTo(destination, offset + 64);
            System.BitConverter.GetBytes(ORIGIN[1]).CopyTo(destination, offset + 68);
            System.BitConverter.GetBytes(ORIGIN[2]).CopyTo(destination, offset + 72);
            System.BitConverter.GetBytes(AXIS[0][0]).CopyTo(destination, offset + 76);
            System.BitConverter.GetBytes(AXIS[0][1]).CopyTo(destination, offset + 80);
            System.BitConverter.GetBytes(AXIS[0][2]).CopyTo(destination, offset + 84);
            System.BitConverter.GetBytes(AXIS[1][0]).CopyTo(destination, offset + 88);
            System.BitConverter.GetBytes(AXIS[1][1]).CopyTo(destination, offset + 92);
            System.BitConverter.GetBytes(AXIS[1][2]).CopyTo(destination, offset + 96);
            System.BitConverter.GetBytes(AXIS[2][0]).CopyTo(destination, offset + 100);
            System.BitConverter.GetBytes(AXIS[2][1]).CopyTo(destination, offset + 104);
            System.BitConverter.GetBytes(AXIS[2][2]).CopyTo(destination, offset + 108);

            return offset + Size;
        }
    }


    class MD3Surface
    {
        public MD3Surface(Model model,string matName)
        {
            Shader = new MD3Shader[1] { new MD3Shader(matName, 0) };

            Array.Copy(Encoding.ASCII.GetBytes(model.Name), 0, NAME, 0, model.Name.Length);
            FLAGS = 0;
            NUM_FRAMES = 1;
            NUM_SHADERS = 1;
            NUM_VERTS = model.Vertices.Count;
            NUM_TRIANGLES = model.Faces.Count;
            OFS_SHADERS = 108;
            Triangle = new MD3Triangle[NUM_TRIANGLES];
            for (var i = 0; i < model.Faces.Count; i++)
                Triangle[i] = new MD3Triangle(new int[] { model.Faces[i][0][0], model.Faces[i][1][0], model.Faces[i][2][0] });

            St = new MD3TexCoord[model.Uvs.Count];
            for (var i = 0; i < model.Uvs.Count; i++)
                St[i] = new MD3TexCoord(model.Uvs[i]);

            XYZNormal = new MD3Vertex[model.Vertices.Count];
            for (var i = 0; i < model.Vertices.Count; i++)
                XYZNormal[i] = new MD3Vertex(model.Vertices[i], model.Normals[i]);


            OFS_TRIANGLES = OFS_SHADERS + (Shader.Length > 0 ? Shader[0].Size * Shader.Length : 0);
            OFS_ST = OFS_TRIANGLES + (Triangle.Length > 0 ? Triangle[0].Size * Triangle.Length : 0);
            OFS_XYZNORMAL = OFS_ST + (St.Length > 0 ? St[0].Size * St.Length : 0);
            OFS_END = OFS_XYZNORMAL + (XYZNormal.Length > 0 ? XYZNormal[0].Size * XYZNormal.Length : 0);
        }

        readonly byte[] IDENT = new byte[] { (byte)'I', (byte)'D', (byte)'P', (byte)'3' };
        readonly byte[] NAME = new byte[64];
        readonly Int32 FLAGS;
        readonly Int32 NUM_FRAMES;
        readonly Int32 NUM_SHADERS;
        readonly Int32 NUM_VERTS;
        readonly Int32 NUM_TRIANGLES;
        readonly Int32 OFS_TRIANGLES;
        readonly Int32 OFS_SHADERS;
        readonly Int32 OFS_ST;
        readonly Int32 OFS_XYZNORMAL;
        readonly Int32 OFS_END;
        readonly MD3Shader[] Shader;
        readonly MD3Triangle[] Triangle;
        readonly MD3TexCoord[] St;
        readonly MD3Vertex[] XYZNormal;

        public IEnumerable<byte> GetBytes()
        {
            var data =
                IDENT
                .Concat(NAME)
                .Concat(System.BitConverter.GetBytes(FLAGS))
                .Concat(System.BitConverter.GetBytes(NUM_FRAMES))
                .Concat(System.BitConverter.GetBytes(NUM_SHADERS))
                .Concat(System.BitConverter.GetBytes(NUM_VERTS))
                .Concat(System.BitConverter.GetBytes(NUM_TRIANGLES))
                .Concat(System.BitConverter.GetBytes(OFS_TRIANGLES))
                .Concat(System.BitConverter.GetBytes(OFS_SHADERS))
                .Concat(System.BitConverter.GetBytes(OFS_ST))
                .Concat(System.BitConverter.GetBytes(OFS_XYZNORMAL))
                .Concat(System.BitConverter.GetBytes(OFS_END));

            for (var i = 0; i < Shader.Length; i++)
                data = data.Concat(Shader[i].GetBytes());

            for (var i = 0; i < Triangle.Length; i++)
                data = data.Concat(Triangle[i].GetBytes());

            for (var i = 0; i < St.Length; i++)
                data = data.Concat(St[i].GetBytes());

            for (var i = 0; i < XYZNormal.Length; i++)
                data = data.Concat(XYZNormal[i].GetBytes());

            return data.ToArray();
        }

        public Int32 Size { get { return 108+(Shader.Length==0?0:Shader.Length*Shader[0].Size)+ (Triangle.Length == 0 ? 0 : Triangle.Length * Triangle[0].Size)+ (St.Length == 0 ? 0 : St.Length * St[0].Size)+ (XYZNormal.Length == 0 ? 0 : XYZNormal.Length * XYZNormal[0].Size); } }
        public bool SizeIsStatic = false;

        public Int32 CopyTo(byte[] destination, int offset)
        {
            IDENT.CopyTo(destination, offset + 0);
            NAME.CopyTo(destination, offset + 4);
            System.BitConverter.GetBytes(FLAGS).CopyTo(destination, offset + 68);
            System.BitConverter.GetBytes(NUM_FRAMES).CopyTo(destination, offset + 72);
            System.BitConverter.GetBytes(NUM_SHADERS).CopyTo(destination, offset + 76);
            System.BitConverter.GetBytes(NUM_VERTS).CopyTo(destination, offset + 80);
            System.BitConverter.GetBytes(NUM_TRIANGLES).CopyTo(destination, offset + 84);
            System.BitConverter.GetBytes(OFS_TRIANGLES).CopyTo(destination, offset + 88);
            System.BitConverter.GetBytes(OFS_SHADERS).CopyTo(destination, offset + 92);
            System.BitConverter.GetBytes(OFS_ST).CopyTo(destination, offset + 96);
            System.BitConverter.GetBytes(OFS_XYZNORMAL).CopyTo(destination, offset + 100);
            System.BitConverter.GetBytes(OFS_END).CopyTo(destination, offset + 104);

            var lastLocation = offset + 108;
            for (var i = 0; i < Shader.Length; i++)
                lastLocation = Shader[i].CopyTo(destination, lastLocation);
            for (var i = 0; i < Triangle.Length; i++)
                lastLocation = Triangle[i].CopyTo(destination, lastLocation);
            for (var i = 0; i < St.Length; i++)
                lastLocation = St[i].CopyTo(destination, lastLocation);
            for (var i = 0; i < XYZNormal.Length; i++)
                lastLocation = XYZNormal[i].CopyTo(destination, lastLocation);

            return lastLocation;
        }
    }

    class MD3Model
    {
        public MD3Model(Model[] model,string name,string matname)
        {
            Array.Copy(Encoding.ASCII.GetBytes(name), 0, NAME, 0, name.Length);

            NUM_SURFACES = Math.Min(model.Length,30);
            OFS_FRAMES = 104;
            OFS_TAGS = OFS_FRAMES + 56 * NUM_FRAMES;
            OFS_SURFACES = OFS_TAGS + 112 * NUM_TAGS;
            Frame = new MD3Frame[1] { new MD3Frame() };
            Tag = new MD3Tag[0];
            Surface = new MD3Surface[Math.Min(model.Length, 30)];
            for (var i = 0; i < Math.Min(model.Length, 30); i++)
            {
                Surface[i] = new MD3Surface(model[i],matname);
            }
        }

        readonly byte[] IDENT = new byte[] { (byte)'I', (byte)'D', (byte)'P', (byte)'3' };
        readonly Int32 VERSION = 15;
        readonly byte[] NAME = new byte[64];
        const Int32 FLAGS=0;
        readonly Int32 NUM_FRAMES = 1;
        readonly Int32 NUM_TAGS = 0;
        readonly Int32 NUM_SURFACES;
        readonly Int32 NUM_SKINS = 0;
        readonly Int32 OFS_FRAMES;
        readonly Int32 OFS_TAGS;
        readonly Int32 OFS_SURFACES;
        readonly MD3Frame[] Frame;
        readonly MD3Tag[] Tag;
        readonly MD3Surface[] Surface;


        public IEnumerable<byte> GetBytes()
        {
            var data =
                IDENT
                .Concat(System.BitConverter.GetBytes(VERSION))
                .Concat(NAME)
                .Concat(System.BitConverter.GetBytes(FLAGS))
                .Concat(System.BitConverter.GetBytes(NUM_FRAMES))
                .Concat(System.BitConverter.GetBytes(NUM_TAGS))
                .Concat(System.BitConverter.GetBytes(NUM_SURFACES))
                .Concat(System.BitConverter.GetBytes(NUM_SKINS))
                .Concat(System.BitConverter.GetBytes(OFS_FRAMES))
                .Concat(System.BitConverter.GetBytes(OFS_TAGS))
                .Concat(System.BitConverter.GetBytes(OFS_SURFACES));

            for (var i = 0; i < Frame.Length; i++)
                data = data.Concat(Frame[i].GetBytes());

            for (var i = 0; i < Tag.Length; i++)
                data = data.Concat(Tag[i].GetBytes());

            for (var i = 0; i < Surface.Length; i++)
                data = data.Concat(Surface[i].GetBytes());

            return data.ToArray();
        }

        public Int32 Size
        {
            get
            {
                var size = 104 + (Frame.Length == 0 ? 0 : Frame.Length * Frame[0].Size) + (Tag.Length == 0 ? 0 : Tag.Length * Tag[0].Size);
                for (var i = 0; i < Surface.Length; i++)
                    size += Surface[i].Size;
                return size;
            }
        }
        public bool SizeIsStatic = false;

        public Int32 CopyTo(byte[] destination, int offset)
        {
            IDENT.CopyTo(destination, offset + 0);
            System.BitConverter.GetBytes(VERSION).CopyTo(destination, offset + 4);
            NAME.CopyTo(destination, offset + 8);
            System.BitConverter.GetBytes(FLAGS).CopyTo(destination, offset + 72);
            System.BitConverter.GetBytes(NUM_FRAMES).CopyTo(destination, offset + 76);
            System.BitConverter.GetBytes(NUM_TAGS).CopyTo(destination, offset + 80);
            System.BitConverter.GetBytes(NUM_SURFACES).CopyTo(destination, offset + 84);
            System.BitConverter.GetBytes(NUM_SKINS).CopyTo(destination, offset + 88);
            System.BitConverter.GetBytes(OFS_FRAMES).CopyTo(destination, offset + 92);
            System.BitConverter.GetBytes(OFS_TAGS).CopyTo(destination, offset + 96);
            System.BitConverter.GetBytes(OFS_SURFACES).CopyTo(destination, offset + 100);
            var lastLocation = offset+104;
            for (var i = 0; i < Frame.Length; i++)
                lastLocation=Frame[i].CopyTo(destination, lastLocation);

            for (var i = 0; i < Tag.Length; i++)
                lastLocation = Tag[i].CopyTo(destination, lastLocation);

            for (var i = 0; i < Surface.Length; i++)
                lastLocation = Surface[i].CopyTo(destination, lastLocation);

            return lastLocation;
        }
    }
}
