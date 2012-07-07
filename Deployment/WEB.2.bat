ECHO OFF

Rem ================================== Important Note!
Rem #1 Before running this batch file 
Rem Check the connectionstrings section of the Import tool.

ECHO ===============================================================================
ECHO                              Install Sense/Net 6.1 WEB.2
ECHO ===============================================================================

Echo.

ECHO ===============================================================================
ECHO             Install FieldConfig and ContentTypes, Import Demo Files
ECHO ===============================================================================

..\Source\SenseNet\Tools\Import\bin\Debug\Import.exe -CTD ..\Source\SenseNet\WebSite\Root\System\Schema\ContentTypes -SOURCE ..\Source\SenseNet\WebSite\Root -TARGET /Root -ASM ..\Source\SenseNet\WebSite\bin 

ECHO WEB.2 - Done.