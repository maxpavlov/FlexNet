ECHO OFF

Rem ================================== Important Note!
Rem #1 Before running this batch file 
Rem Check the connectionstrings section of the Import tool.

ECHO ===============================================================================
ECHO                              Install Sense/Net 6.1 WEB.4
ECHO ===============================================================================

Echo.

rem build index files
..\Source\SenseNet\WebSite\bin\IndexPopulator.exe

ECHO WEB.4 - Done.