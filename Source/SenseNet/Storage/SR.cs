using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace SenseNet.ContentRepository.Storage
{
    /// <summary>
    /// Language independent strings.
    /// </summary>
	static class SR
	{
		internal static class Exceptions
		{
			internal static class General
			{
				internal static string Msg_ParameterValueCannotBeZero = "The value of parameter cannot be zero.";

				internal static string Msg_InvalidPath_1 = "The path is invalid: {0}";
				internal static string Msg_PathTooLong = "Path exceeds the maximum lenght allowed";
				internal static string Msg_ParentNodeDoesNotExists = "Parent Node does not exists";
				internal static string Msg_NameCannotBeEmpty = "Name cannot be Empty";
				internal static string Msg_InvalidName = "The Name is invalid";
				internal static string Msg_InvalidParameter = "Invalid parameter";
				internal static string Msg_FileNameExtensionCannotBeNull = "The value of fileName.Extension cannot be null.";
				internal static string Msg_ParamtereIsNotABinaryFileName = "The type of paramter value must be a BinaryFileName";
				internal static string Msg_AssociatedBinaryDataDoesNotExist = "Associated BinaryData does not exist.";
				internal static Exception Exc_LessThanDateTimeMinValue()
				{
					return new ArgumentOutOfRangeException(String.Concat("DateTime value cannot be less than ",
						Data.DataProvider.Current.DateTimeMinValue.ToString(CultureInfo.CurrentCulture)));
				}
				internal static Exception Exc_BiggerThanDateTimeMaxValue()
				{
					return new ArgumentOutOfRangeException(String.Concat("DateTime value cannot be bigger than ",
						Data.DataProvider.Current.DateTimeMaxValue.ToString(CultureInfo.CurrentCulture)));
				}
			}
			internal static class Schema
			{
				internal static string Msg_InconsistentHierarchy = "Inconsistent hierarchy";
				internal static string Msg_KeyAndTypeNameAreNotEqual = "Key and TypeName are not equal";
				internal static string Msg_UnknownPropertySetType = "Unknown PropertySet type";
				internal static string Msg_UnknownNodeType = "Cannot create Node with unknown NodeType";
				internal static string Msg_CircularReference = "NodeType and his parent cannot be same.";
				internal static string Msg_MappingAlreadyExists = "Mapping already exists";
				internal static string Msg_ProtectedPropetyTypeDeleteViolation = "Protected PropetyType Delete violation";
				internal static string Msg_PropertyTypeDoesNotExist = "PropertyType does not exist";
				internal static string Msg_NodeAttributeDoesNotEsist = "NodeAttribute does not exist:";
			}
			internal static class VersionNumber
			{
				internal static string Msg_InvalidVersionFormat = "[Msg_InvalidVersionFormat]";
			}
			internal static class Search
			{
				internal static string Msg_UnknownStringOperator_1 = "Unknown StringOperator: {0}";
				internal static string Msg_UnknownValueOperator_1 = "Unknown ValueOperator: {0}";
				internal static string Msg_PageSizeOutOfRange = "PageSize minimum value is 1.";
				internal static string Msg_StartIndexOutOfRange = "StartIndex minimum value is 1.";
                internal static string Msg_SkipOutOfRange = "Skip minimum value is 0.";
				internal static string Msg_TopOutOfRange = "Top minimum value is 1.";
				internal static string Msg_InvalidNodeQueryXml = "Invalid NodeQuery XML";
			}
			internal static class Configuration
			{
				internal static string Msg_DataProviderImplementationDoesNotExist = "DataProvider implementation does not exist";
				internal static string Msg_InvalidDataProviderImplementation = "DataProvider implementation must be inherited from SenseNet.ContentRepository.Storage.Data.DataProvider";
				internal static string Msg_AccessProviderImplementationDoesNotExist = "AccessProvider implementation does not exist";
				internal static string Msg_InvalidAccessProviderImplementation = "AccessProvider implementation must be inherited from SenseNet.ContentRepository.Storage.Security.AccessProvider";
			}
			internal static class XmlSchema
			{
				internal static string Msg_SchemaNotLoaded = "Cannot validate: schema was not loaded";
			}
		}
	}
}