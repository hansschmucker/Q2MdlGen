using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Q2MdlGen
{
    public static class ASE
    {
        public static void AseWriteHeader(string filename)
        {
            File.WriteAllText(filename,
     @"*3DSMAX_ASCIIEXPORT	200
*COMMENT	""OBJ""
*SCENE	{
	*SCENE_FILENAME	""OBJ""
	*SCENE_FIRSTFRAME	0
	*SCENE_LASTFRAME	100
	*SCENE_FRAMESPEED	30
	*SCENE_TICKSPERFRAME	160
	*SCENE_BACKGROUND_STATIC	0.0000	0.0000	0.0000
	*SCENE_AMBIENT_STATIC	0.0000	0.0000	0.0000
}
*MATERIAL_LIST	{
	*MATERIAL_COUNT	1
	*MATERIAL	0	{
		*MATERIAL_NAME	""textures/Standard""
		*MATERIAL_CLASS	""Standard""
		*MATERIAL_DIFFUSE	1.000000	1.000000	1.000000
		*MATERIAL_SHADING Phong
		*MAP_DIFFUSE	{
			*MAP_NAME	""textures/Standard""
			*MAP_CLASS	""Bitmap""
			*MAP_SUBNO	1
			*MAP_AMOUNT	1.0
			*MAP_TYPE	Screen
			*BITMAP	""..\textures\Standard.tga""
			*BITMAP_FILTER	Pyramidal
		}
	}
}
");
        }

        public static void AseAppendMesh(string filename, Model model)
        {
            AseAppendMesh(model.Name, filename, model.Vertices, model.Uvs, model.Normals, model.Faces);
        }
        public static void AseAppendMesh(string modelName, string filename, List<double[]> Vertices, List<double[]> Uvs, List<double[]> Normals, List<int[][]> Faces)
        {

            File.AppendAllText(filename,
@"*GEOMOBJECT	{
	*NODE_NAME	""" + modelName + @"""
	*NODE_TM	{
		*NODE_NAME	""" + modelName + @"""
		*INHERIT_POS	0	0	0
		*INHERIT_ROT	0	0	0
		*INHERIT_SCL	0	0	0
		*TM_ROW0	1.0	0	0
		*TM_ROW1	0	1.0	0
		*TM_ROW2	0	0	1.0
		*TM_ROW3	0	0	0
		*TM_POS	0.000000	0.000000	0.000000
	}
	*MESH	{
		*TIMEVALUE	0
		*MESH_NUMVERTEX	" + Vertices.Count + @"
		*MESH_NUMFACES	" + Faces.Count + @"
		*COMMENT	""SURFACETYPE    MST_PLANAR""
");

            File.AppendAllText(filename,
@"		*MESH_VERTEX_LIST	{
");
            for (var i = 0; i < Vertices.Count; i++)
                File.AppendAllText(filename,
@"			*MESH_VERTEX	" + i + @"	" + (Vertices[i][0]/64).ToString(CultureInfo.InvariantCulture) + @"	" + (Vertices[i][1]/64).ToString(CultureInfo.InvariantCulture) + @"	" + (Vertices[i][2]/64).ToString(CultureInfo.InvariantCulture) + @"
");

            File.AppendAllText(filename,
@"		}
");

            File.AppendAllText(filename,
@"		*MESH_NORMALS	{
");

            var VertexNormals = new List<double[]>();
            for (var i = 0; i < Vertices.Count; i++)
                VertexNormals.Add(new double[3] { 1, 1, 1 });
            for (var i = 0; i < Faces.Count; i++)
            {
                VertexNormals[Faces[i][0][0]] = Normals[Faces[i][0][2]];
                VertexNormals[Faces[i][1][0]] = Normals[Faces[i][1][2]];
                VertexNormals[Faces[i][2][0]] = Normals[Faces[i][2][2]];
            }

            for (var i = 0; i < Vertices.Count; i++)
                File.AppendAllText(filename,
@"			*MESH_VERTEXNORMAL	" + i + @"	" + (VertexNormals[i][0]).ToString(CultureInfo.InvariantCulture) + @"	" + (VertexNormals[i][1]).ToString(CultureInfo.InvariantCulture) + @"	" + (VertexNormals[i][2]).ToString(CultureInfo.InvariantCulture) + @"
");

            File.AppendAllText(filename,
@"		}
");

            File.AppendAllText(filename,
@"		*MESH_FACE_LIST	{
");

            for (var i = 0; i < Faces.Count; i++)
                File.AppendAllText(filename,
@"			*MESH_FACE	" + i + @"	A:	" + Faces[i][0][0] + @"	B:	" + Faces[i][1][0] + @"	C:	" + Faces[i][2][0] + @"	AB:	1	BC:	1	CA:	1	*MESH_SMOOTHING	0	*MESH_MTLID	0
");

            File.AppendAllText(filename,
@"		}
");

            File.AppendAllText(filename,
@"      *MESH_NUMTVERTEX	" + Uvs.Count + @"
        *MESH_TVERTLIST	{
");

            for (var i = 0; i < Uvs.Count; i++)
                File.AppendAllText(filename,
@"			*MESH_TVERT	" + i + @"	" + Uvs[i][0].ToString(CultureInfo.InvariantCulture) + @"	" + Uvs[i][1].ToString(CultureInfo.InvariantCulture) + @"	" + Uvs[i][2].ToString(CultureInfo.InvariantCulture) + @"
");

            File.AppendAllText(filename,
@"		}
");

            File.AppendAllText(filename,
@"      *MESH_NUMTVFACES	" + Faces.Count + @"
        *MESH_TFACELIST 	{
");

            for (var i = 0; i < Faces.Count; i++)
                File.AppendAllText(filename,
@"			*MESH_TFACE	" + i + @"	" + Faces[i][0][1].ToString(CultureInfo.InvariantCulture) + @"	" + Faces[i][1][1].ToString(CultureInfo.InvariantCulture) + @"	" + Faces[i][2][1].ToString(CultureInfo.InvariantCulture) + @"
");

            File.AppendAllText(filename,
@"		}
	}
	*PROP_MOTIONBLUR	0
	*PROP_CASTSHADOW	1
	*PROP_RECVSHADOW	1
	*MATERIAL_REF	0
}
");

        }


    }
}
