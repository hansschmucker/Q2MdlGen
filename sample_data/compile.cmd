IF EXIST "%~n1.*" DEL /F "%~dn1.*"
COPY "%~dpn1.map" .\
IF NOT EXIST "%~n1.map" GOTO :END
Q2MdlGen "%~n1.map"
qbsp3 "%~n1"
IF NOT EXIST "%~n1.bsp" GOTO :END
timvis3 -fast -threads 8 "%~n1"
move /y "%~n1.bsp" "%~dp1\"
..\..\q2rtx.exe +set game model_spawn +set cheats 1 +set cl_gun 0 +bind q quit +bind l toggleconsole +map "%~n1"
:END