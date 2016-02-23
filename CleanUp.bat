@ECHO OFF
Echo Deleting content from ALL bin and obj folders and also removing those folders in any subfolder from here.

FOR /d /r . %%d IN (bin,obj) DO @IF EXIST "%%d" RD /s/q "%%d"

ECHO Done
PAUSE