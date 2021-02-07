Getting started

You need a mod which supports MD3 loading via entity model_spawn. There's one included here, but if you have a better one go nuts:
https://github.com/hansschmucker/Q2MdlGen/blob/main/model_spawn.mod/gamex86_64.dll

Create a directory next to baseq2 and give it an appropriate name. This is your mod path. To launch Q2RTX with this mod create a shortcut
and add +set game MyModName to the commandline. In my case it's 

q2rtx.exe +set game model_spawn +set cheats 1 +set cl_gun 0 +bind q quit +bind l toggleconsole +bind p screenshot

(which also adds a few other niceties).

Create a maps and a models folder int your mod directory.

Place any old OBJ file in the models directory.

Open up your favorite map editor and put a model_spawn entity where you want the model to appear. Set the model key to the relative path, for example:

[model_spawn]
model=models/town.obj

Move it to the desired location of 0,0,0 in your model, save the map and run it through Q2MdlGen (If you're working
directly on the MAP file, be sure to save it, because Q2MdlGen will alter it):

Q2MdlGen Town.map

(You can get my latest build at https://github.com/hansschmucker/Q2MdlGen/blob/main/latest_build/Q2MdlGen.exe if you don't want to build it yourself)

The result will be a couple of new md3 files in your models directory and an updated map. Run that through your usual build process and you'll get
a map with all your favorite polygons in the right place.

You can also find my compile script, along with a sample model and map at

https://github.com/hansschmucker/Q2MdlGen/tree/main/sample_data

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

WIP:

colission brush generation

TODO:
Add support for multiple materials

Material texture parsing

Set correct bounds