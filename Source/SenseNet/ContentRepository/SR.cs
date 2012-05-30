namespace SenseNet.ContentRepository
{
	internal static class SR
	{
		internal static class Exceptions
		{
			internal static class Content
			{
				internal static string Msg_CannotCreateNewContentWithNullArgument = "Cannot create new Content: Argument cannot be null in this construction mechanism.";
				internal static string Msg_UnknownContentType = "Unknown ContentType";
			}
			internal static class Registration
			{
				internal static string Msg_NodeTypeMustBeInheritedFromNode_1 = "NodeType must be inherited from Node: {0}";
				internal static string Msg_DefinedHandlerIsNotAContentHandler = "Invalid ContentTypeDefinition: defined handler is not a ContentHandler";
				internal static string Msg_UnknownParentContentType = "Parent ContentType is not found";
				internal static string Msg_DataTypeCollisionInTwoProperties_4 = "DataType collision in two properties. NodeType = '{0}', PropertyType = '{1}', original DataType = {2}, passed DataType = {3}.";

				//-- Attribute parsing
				internal static string Msg_PropertyTypeAttributesWithTheSameName_2 = "PropertyAttributes with the same name are not allowed. Class: {0}, Property: {1}";

				//-- Content registration
				internal static string Msg_InvalidContentTypeDefinitionXml = "Invalid ContentType Definition XML";
				internal static string Msg_InvalidContentListDefinitionXml = "Invalid ContentList Definition XML";
				internal static string Msg_ContentHandlerNotFound = "ContentHandler does not found";
				internal static string Msg_UnknownFieldType = "Unknown FieldType";
				internal static string Msg_UnknownFieldSettingType = "Unknown FieldSetting Type";
				internal static string Msg_FieldTypeNotSpecified = "FieldType does not specified";
				internal static string Msg_NotARepositoryDataType = "Type does not a Sense/Net Content Repository DataType";
				internal static string Msg_FieldBindingsCount_1 = "The length of Field's Bindings list must be {0}";
				internal static string Msg_InconsistentContentTypeName = "Cannot modify ContentTypeDefinition: ContentTypeSetting's name and ContentType name in XML content are not equal.";
				internal static string Msg_PropertyAndFieldAreNotConnectable = "Property and Field are not connectable";
				internal static string Msg_InvalidReferenceField_2 = "Field cannot connect a property as a Reference. Type of property must be IEnumerable<Node>, Node or a class that is inherited from Node. ContentType: {0}, Field: {1}";
			}
            internal static class Configuration
            {
                internal static string Msg_DirectoryProviderImplementationDoesNotExist = "DirectoryProvider implementation does not exist";
                internal static string Msg_InvalidDirectoryProviderImplementation = "DirectoryProvider implementation must be inherited from SenseNet.ContentRepository.DirectoryProvider";
            }

            internal static class i18n
            {
                internal static string LoadResourcesParameterValueNull = "LoadResourcesParameterValueNull";
            }
		}
	}

}