SET NOCOUNT ON

--====================================================================================== Nodetypes

DECLARE @id int
SELECT @id = PropertySetTypeId FROM [dbo].[SchemaPropertySetTypes] WHERE Name = 'NodeType'
INSERT INTO [dbo].[SchemaPropertySets] ([ParentId], [Name], [PropertySetTypeId], [ClassName])
	VALUES (null, 'ContentType', @id, 'SenseNet.ContentRepository.Schema.ContentType')
INSERT INTO [dbo].[SchemaPropertySets] ([ParentId], [Name], [PropertySetTypeId], [ClassName])
	VALUES (null, 'GenericContent', @id, 'SenseNet.ContentRepository.GenericContent')
GO
--====================================================================================== Create PropertyTypes and assign to NodeTypes
-------- Create PropertyGenerator procedure

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[xCreateAndAssignPropertyType]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[xCreateAndAssignPropertyType]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[xCreateAndAssignPropertyType]
	@PropertySetName varchar(450),
	@PropertyName varchar(450),
	@DataTypeName varchar(50),
	@Mapping int,
	@IsDeclared tinyint,
	@IsContentListProperty tinyint
AS
BEGIN
	-- @DataTypeName --> @DataTypeId
	DECLARE @DataTypeId int
	SELECT @DataTypeId = [DataTypeId] FROM [dbo].[SchemaDataTypes] WHERE [Name] = @DataTypeName
	-- @PropertySetName --> @PropertySetId
	DECLARE @PropertySetId int
	SELECT @PropertySetId = [PropertySetId] FROM [dbo].[SchemaPropertySets] WHERE [Name] = @PropertySetName
	-- Check PropertyType existence
	DECLARE @PropertyTypeId int
	SELECT @PropertyTypeId = [PropertyTypeId] FROM [dbo].[SchemaPropertyTypes] WHERE [Name] = @PropertyName
	-- Create PropertyType
	IF @PropertyTypeId IS NULL BEGIN
		INSERT INTO [dbo].[SchemaPropertyTypes] ([Name], [DataTypeId], [Mapping], [IsContentListProperty]) VALUES (@PropertyName, @DataTypeId, @Mapping, @IsContentListProperty)
		SET @PropertyTypeId = @@IDENTITY
	END
	-- Assign
	INSERT INTO [dbo].[SchemaPropertySetsPropertyTypes] ([PropertyTypeId], [PropertySetId], [IsDeclared]) VALUES (@PropertyTypeId, @PropertySetId, @IsDeclared)
END
GO
----------------------------------------------------------------------------------------------------

EXEC dbo.xCreateAndAssignPropertyType 'ContentType',           'Binary',           'Binary',      0, 1, 0
EXEC dbo.xCreateAndAssignPropertyType 'Folder',                'VersioningMode',   'Int',         0, 1, 0
EXEC dbo.xCreateAndAssignPropertyType 'GenericContent',        'VersioningMode',   'Int',         0, 1, 0

----------------------------------------------------------------------------------------------------

DROP PROCEDURE dbo.xCreateAndAssignPropertyType

--====================================================================================== Nodes
-------- Create NodeGenerator procedure

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[xCreateNode]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[xCreateNode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE dbo.xCreateNode
	@NodeTypeName nvarchar(1000),
	@Index int,
	@ParentPath nvarchar(1000),
	@Name nvarchar(1000)
AS
BEGIN
	DECLARE @path nvarchar(1000)
	DECLARE @nodeTypeId int
	DECLARE @nodeId int
	DECLARE @parentId int
	DECLARE @adminId int
	DECLARE @versionId int

	SELECT @path = @ParentPath + '/' + @Name
	SELECT @nodeTypeId = PropertySetId FROM SchemaPropertySets WHERE [Name] LIKE ('%' + @NodeTypeName)
	SELECT @parentId = NodeId FROM Nodes WHERE [Path] = @ParentPath
	
	SELECT @adminId = 1

	INSERT INTO [dbo].[Nodes]
			   ([NodeTypeId], [IsDeleted], [IsInherited], [ParentNodeId], [Name], [Path], [Index], [Locked], [LockedById], [ETag], [LockType], [LockTimeout],   [LockDate], [LockToken], [LastLockUpdate], [LastMinorVersionId], [LastMajorVersionId], [CreationDate], [CreatedById], [ModificationDate], [ModifiedById])
		 VALUES( @nodeTypeId,           0,             1,      @parentId,  @Name,  @path,  @Index,        0,         null,     '',          0,             0, '1900-01-01',          '',     '1900-01-01',              null,                 null,      getdate(),      @adminId,         getdate(),        @adminId)
	SELECT @nodeId = @@IDENTITY

	INSERT INTO [dbo].[Versions] ([NodeId], [MajorNumber], [MinorNumber], [CreationDate], [CreatedById], [ModificationDate], [ModifiedById])
		 VALUES                  ( @nodeId,             1,             0,   '2007-07-07',      @adminId,       '2007-07-08',       @adminId)
	SELECT @versionId = @@IDENTITY

	UPDATE [dbo].[Nodes] SET [LastMinorVersionId] = @versionId, [LastMajorVersionId] = @versionId WHERE NodeId = @nodeId

END
GO

SET NOCOUNT ON

--================================================================================================ System structure

EXEC dbo.xCreateNode 'SystemFolder',           3, '/Root',                                           /**/ 'System'
EXEC dbo.xCreateNode 'SystemFolder',           1, '/Root/System',                                    /**/     'Schema'
EXEC dbo.xCreateNode 'SystemFolder',           1, '/Root/System/Schema',                             /**/         'ContentTypes'

--================================================================================================

DROP PROCEDURE dbo.xCreateNode
GO

--====================================================================================== Security
-------- Create Default Security entries

DECLARE @RootNodeId int
DECLARE @AdministratorNodeId int
DECLARE @AdministratorGroupNodeId int
DECLARE @VisitorNodeId int
DECLARE @EveryoneGroupNodeId int


SELECT @RootNodeId = NodeId FROM Nodes WHERE Path = '/Root'
IF @RootNodeId IS NULL RAISERROR ('Root node cannot be found. Check the Install_04_Data_Phase2.sql.', 18, 1)

SELECT @AdministratorNodeId = NodeId FROM Nodes WHERE Path = '/Root/IMS/BuiltIn/Portal/Administrator'
IF @AdministratorNodeId IS NULL	RAISERROR ('Administrator node cannot be found. Check the Install_04_Data_Phase2.sql.', 18, 2)

SELECT @VisitorNodeId = NodeId FROM Nodes WHERE Path = '/Root/IMS/BuiltIn/Portal/Visitor'
IF @VisitorNodeId IS NULL RAISERROR ('Visitor node cannot be found. Check the Install_04_Data_Phase2.sql.', 18, 3)

SELECT @AdministratorGroupNodeId = NodeId FROM Nodes WHERE Path = '/Root/IMS/BuiltIn/Portal/Administrators'
IF @AdministratorGroupNodeId IS NULL RAISERROR ('Administrator Group node cannot be found. Check the Install_04_Data_Phase2.sql.', 18, 4)

SELECT @EveryoneGroupNodeId = NodeId FROM Nodes WHERE Path = '/Root/IMS/BuiltIn/Portal/Everyone'
IF @EveryoneGroupNodeId IS NULL RAISERROR ('Everyone Group node cannot be found. Check the Install_04_Data_Phase2.sql.', 18, 4)


-- allow everything for Administrator
INSERT INTO
	dbo.SecurityEntries (
	DefinedOnNodeId,
	PrincipalId,
	IsInheritable,
	PermissionValue1,
	PermissionValue2,
	PermissionValue3,
	PermissionValue4,
	PermissionValue5,
	PermissionValue6,
	PermissionValue7,
	PermissionValue8,
	PermissionValue9,
	PermissionValue10,
	PermissionValue11,
	PermissionValue12,
	PermissionValue13,
	PermissionValue14,
	PermissionValue15,
	PermissionValue16)
VALUES (
	@RootNodeId,
	@AdministratorNodeId,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1)


-- allow everything for Administrators group
INSERT INTO
	dbo.SecurityEntries (
	DefinedOnNodeId,
	PrincipalId,
	IsInheritable,
	PermissionValue1,
	PermissionValue2,
	PermissionValue3,
	PermissionValue4,
	PermissionValue5,
	PermissionValue6,
	PermissionValue7,
	PermissionValue8,
	PermissionValue9,
	PermissionValue10,
	PermissionValue11,
	PermissionValue12,
	PermissionValue13,
	PermissionValue14,
	PermissionValue15,
	PermissionValue16)
VALUES (
	@RootNodeId,
	@AdministratorGroupNodeId,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1,
	1)
	
-- allow See, Open deny all other for Visitor
INSERT INTO
	dbo.SecurityEntries (
	DefinedOnNodeId,
	PrincipalId,
	IsInheritable,
	PermissionValue1,
	PermissionValue2,
	PermissionValue3,
	PermissionValue4,
	PermissionValue5,
	PermissionValue6,
	PermissionValue7,
	PermissionValue8,
	PermissionValue9,
	PermissionValue10,
	PermissionValue11,
	PermissionValue12,
	PermissionValue13,
	PermissionValue14,
	PermissionValue15,
	PermissionValue16)
VALUES (
	@RootNodeId,
	@VisitorNodeId,
	1,
	1,
	1,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0)

-- allow See, Open deny all other for Everyone
INSERT INTO
	dbo.SecurityEntries (
	DefinedOnNodeId,
	PrincipalId,
	IsInheritable,
	PermissionValue1,
	PermissionValue2,
	PermissionValue3,
	PermissionValue4,
	PermissionValue5,
	PermissionValue6,
	PermissionValue7,
	PermissionValue8,
	PermissionValue9,
	PermissionValue10,
	PermissionValue11,
	PermissionValue12,
	PermissionValue13,
	PermissionValue14,
	PermissionValue15,
	PermissionValue16)
VALUES (
	@RootNodeId,
	@EveryoneGroupNodeId,
	1,
	1,
	1,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0)

GO


SET NOCOUNT OFF