SET NOCOUNT ON

-------- Create Default Security entries part 2

DECLARE @SystemFolder int
DECLARE @AdministratorNodeId int
DECLARE @AdministratorGroupNodeId int
DECLARE @DevelopersGroupId int


SELECT @SystemFolder = NodeId FROM Nodes WHERE Path LIKE '/Root/System/Schema/ContentTypes/%/SystemFolder'
IF @SystemFolder IS NULL RAISERROR ('SystemFolder node cannot be found. Check the Install_05_Data_Phase3.sql.', 18, 1)

SELECT @AdministratorNodeId = NodeId FROM Nodes WHERE Path = '/Root/IMS/BuiltIn/Portal/Administrator'
IF @AdministratorNodeId IS NULL	RAISERROR ('Administrator node cannot be found. Check the Install_05_Data_Phase3.sql.', 18, 2)

SELECT @AdministratorGroupNodeId = NodeId FROM Nodes WHERE Path = '/Root/IMS/BuiltIn/Portal/Administrators'
IF @AdministratorGroupNodeId IS NULL RAISERROR ('Administrator Group node cannot be found. Check the Install_05_Data_Phase3.sql.', 18, 4)

SELECT @DevelopersGroupId = NodeId FROM Nodes WHERE Path = '/Root/IMS/Demo/Developers'
IF @DevelopersGroupId IS NULL RAISERROR ('Developers Group node cannot be found. Check the Install_05_Data_Phase3.sql.', 18, 4)

-- Break the permission inheritance on the SystemFolder
UPDATE Nodes SET IsInherited = 0 WHERE NodeId = @SystemFolder

-- Allow See, Open on SystemFolder for Developers
INSERT INTO dbo.SecurityEntries (DefinedOnNodeId,PrincipalId,IsInheritable,PermissionValue1,PermissionValue2,PermissionValue3,PermissionValue4,PermissionValue5,PermissionValue6,PermissionValue7,PermissionValue8,PermissionValue9,PermissionValue10,PermissionValue11,PermissionValue12,PermissionValue13,PermissionValue14,PermissionValue15,PermissionValue16)
	VALUES (@SystemFolder,@DevelopersGroupId,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0)
-- Allow full control on SystemFolder for Administrators
INSERT INTO dbo.SecurityEntries (DefinedOnNodeId,PrincipalId,IsInheritable,PermissionValue1,PermissionValue2,PermissionValue3,PermissionValue4,PermissionValue5,PermissionValue6,PermissionValue7,PermissionValue8,PermissionValue9,PermissionValue10,PermissionValue11,PermissionValue12,PermissionValue13,PermissionValue14,PermissionValue15,PermissionValue16)
	VALUES (@SystemFolder,@AdministratorGroupNodeId,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1)
-- Allow full control on SystemFolder for Administrator
INSERT INTO dbo.SecurityEntries (DefinedOnNodeId,PrincipalId,IsInheritable,PermissionValue1,PermissionValue2,PermissionValue3,PermissionValue4,PermissionValue5,PermissionValue6,PermissionValue7,PermissionValue8,PermissionValue9,PermissionValue10,PermissionValue11,PermissionValue12,PermissionValue13,PermissionValue14,PermissionValue15,PermissionValue16)
	VALUES (@SystemFolder,@AdministratorNodeId,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1)

SET NOCOUNT OFF