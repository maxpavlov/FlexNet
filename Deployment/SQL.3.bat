ECHO OFF

ECHO ===============================================================================
ECHO                              Install Sense/Net 6.1 SQL.3
ECHO ===============================================================================

Echo.

sqlcmd.exe -S %DATASOURCE% -d %INITIALCATALOG% -i "..\Source\SenseNet\Storage\Data\SqlClient\Scripts\Install_05_Data_Phase3.sql"

ECHO SQL.3 - Done.