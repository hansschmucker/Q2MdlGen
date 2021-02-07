Syntax:
   Q2MdlGen SourceObjectFile [NoTGA] [GenASE] [ModelName MyRelativeModelNameWithoutDotMD3] [MatName MyTexturePath]
   or Q2MdlGen SourceMapFile

For example, to compile MyObjFile.obj to MyObjFile_N.md3 where N is an increasing number starting at 0 if
your model is too big to fit into a single file:
   Obj2Md3 MyObjFile.obj
For example will give you MyObjFile_0.md3, MyObjFile_1.md3 and so on depending on the size of your OBJ.

Execute it INSIDE your maps directory and you'll get a relative path inside the MD3 that you won't have to
fix, or include a ModelName, which should be the MD3's realtive path.

Texture is set to the same path but with extension .tga instead of _N.md3

If you have a set of files named the same as your Obj file with suffix
(Color).png (Roughness).png (Normal).png (Metallic).png (Light).png , for example Foo(Roughness).png they
will be compiled into TGAs, so you don't have to fiddle with Alpha channels, unless you include NoTGA in
your arguments.

If you include GenASE in your commandline you'll get ASE files along with your MD3 for analysis.

If you run it on a map file, note that the map file will be modified. Specifically, Q2MdlGen will filter out
all model_spawn OBJ entities, split the models if necessary and add the required number of entitites
at the correct offset, along with a few clip brushes to approximate colission,

WIP
colission brush generation

TODO:
Add support for multiple materials
Material texture parsing
Set correct bounds