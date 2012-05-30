ECHO OFF

Rem ================================== Important Note!
Rem #1 Before running this batch file 
Rem Check the connectionstrings section of the Import.

ECHO ===============================================================================
ECHO                     Install Sense/Net 6.0 demo structure 
ECHO ===============================================================================

Echo.

SET DATASOURCE=MySenseNetContentRepositoryDatasource
SET INITIALCATALOG=SenseNetContentRepository

REM ECHO -------------------------------------------------------------------------------
REM ECHO                                Create Database
REM ECHO -------------------------------------------------------------------------------

REM ECHO Creating database...

sqlcmd.exe -S %DATASOURCE% -i "Create_SenseNet_Database.sql" >> InstallSenseNet.log

ECHO -------------------------------------------------------------------------------
ECHO                            Create Database structure
ECHO -------------------------------------------------------------------------------

echo Run Install_01_Schema.sql >> InstallSenseNet.log
sqlcmd.exe -S %DATASOURCE% -d %INITIALCATALOG% -i "Install_01_Schema.sql" >> InstallSenseNet.log

echo Run Install_02_Procs.sql >> InstallSenseNet.log
sqlcmd.exe -S %DATASOURCE% -d %INITIALCATALOG% -i "Install_02_Procs.sql" >> InstallSenseNet.log

echo Run Install_03_Data_Phase1.sql >> InstallSenseNet.log
sqlcmd.exe -S %DATASOURCE% -d %INITIALCATALOG% -i "Install_03_Data_Phase1.sql" >> InstallSenseNet.log

echo Run Install_04_Data_Phase2.sql >> InstallSenseNet.log
sqlcmd.exe -S %DATASOURCE% -d %INITIALCATALOG% -i "Install_04_Data_Phase2.sql" >> InstallSenseNet.log

sqlcmd.exe -S %DATASOURCE% -d %INITIALCATALOG% -i "SqlWorkflowInstanceStoreSchema.sql"
sqlcmd.exe -S %DATASOURCE% -d %INITIALCATALOG% -i "SqlWorkflowInstanceStoreLogic.sql"

ECHO -------------------------------------------------------------------------------
ECHO                            Import necessary contents
ECHO -------------------------------------------------------------------------------


Import.exe -CTD ContentTypes -SOURCE Root -TARGET /Root

ECHO Done.
