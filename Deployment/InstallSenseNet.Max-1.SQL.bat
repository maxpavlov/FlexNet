ECHO OFF

Rem ================================== Important Note!
Rem #1 Before running this batch file 
Rem Check the connectionstrings section of the Import tool.

ECHO ===============================================================================
ECHO                              Install Sense/Net 6.0 
ECHO ===============================================================================

Echo.

ECHO ===============================================================================
ECHO                                Install Database
ECHO ===============================================================================

SET DATASOURCE=SQL	
SET INITIALCATALOG=SenseNetContentRepository

ECHO Creating database...

sqlcmd.exe -S %DATASOURCE% -i "..\Source\SenseNet\Storage\Data\SqlClient\Scripts\Create_SenseNet_Database.sql"
sqlcmd.exe -S %DATASOURCE% -d %INITIALCATALOG% -i "..\Source\SenseNet\Storage\Data\SqlClient\Scripts\Install_01_Schema.sql"
sqlcmd.exe -S %DATASOURCE% -d %INITIALCATALOG% -i "..\Source\SenseNet\Storage\Data\SqlClient\Scripts\Install_02_Procs.sql"
sqlcmd.exe -S %DATASOURCE% -d %INITIALCATALOG% -i "..\Source\SenseNet\Storage\Data\SqlClient\Scripts\Install_03_Data_Phase1.sql"
sqlcmd.exe -S %DATASOURCE% -d %INITIALCATALOG% -i "..\Source\SenseNet\Storage\Data\SqlClient\Scripts\Install_04_Data_Phase2.sql"

ECHO ===============================================================================
ECHO			         Install Workflow Store
ECHO ===============================================================================

sqlcmd.exe -S %DATASOURCE% -d %INITIALCATALOG% -i "..\Source\SenseNet\Storage\Data\SqlClient\Scripts\SqlWorkflowInstanceStoreSchema.sql"
sqlcmd.exe -S %DATASOURCE% -d %INITIALCATALOG% -i "..\Source\SenseNet\Storage\Data\SqlClient\Scripts\SqlWorkflowInstanceStoreLogic.sql"


