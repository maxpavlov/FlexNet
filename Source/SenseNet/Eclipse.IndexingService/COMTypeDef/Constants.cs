using System;

namespace Eclipse.IndexingService.COMTypeDef
{
    public static class Constants
    {
        public const int S_OK = 0;
        public const string IFilterGUID = "89BCB740-6119-101A-BCB7-00DD010655AF";
        public const string IClassFactoryGUID = "00000001-0000-0000-C000-000000000046";
        public const string PH_INullFilter = "{098f2470-bae0-11cd-b579-08002b30bfeb}";
        public const string PH_IDefaultFilter = "{5e941d80-bf96-11cd-b579-08002b30bfeb}";

        public static class FMTIDs
        {
            /*****************************************
             * 
             * Indexing Service property sets
             * 
             *****************************************/
            /// <summary>
            /// {afafaca5-b5d1-11d0-8c62-00c04fc2db8d}
            /// </summary>
            public static readonly Guid DBPROPSET_CIFRMWRKCORE_EXT = new Guid("{ 0xafafaca5, 0xb5d1, 0x11d0, { 0x8c, 0x62, 0x00, 0xc0, 0x4f, 0xc2, 0xdb, 0x8d }}");
            /// <summary>
            /// {a9bd1526-6a80-11d0-8c9d-0020af1d740e}
            /// </summary>
            public static readonly Guid DBPROPSET_FSCIFRMWRK_EXT = new Guid("{ 0xA9BD1526, 0x6A80, 0x11D0, { 0x8C, 0x9D, 0x00, 0x20, 0xAF, 0x1D, 0x74, 0x0E } }");
            /// <summary>
            /// {aa6ee6b0-e828-11d0-b23e-00aa0047fc01}
            /// </summary>
            public static readonly Guid DBPROPSET_MSIDXS_ROWSETEXT = new Guid("{ 0xaa6ee6b0, 0xe828, 0x11d0, { 0xb2, 0x3e, 0x00, 0xaa, 0x00, 0x47, 0xfc, 0x01 } }");
            /// <summary>
            /// {a7ac77ed-f8d7-11ce-a798-0020f8008025}
            /// </summary>
            public static readonly Guid DBPROPSET_QUERYEXT = new Guid("{ 0xA7AC77ED, 0xF8D7, 0x11CE, { 0xA7, 0x98, 0x00, 0x20, 0xF8, 0x00, 0x80, 0x25 } }");
            /// <summary>
            /// {64440490-4c8b-11d1-8b70-080036b11a03}
            /// </summary>
            public static readonly Guid PSGUID_AUDIO = new Guid("{0x64440490, 0x4c8b, 0x11d1, {0x8b, 0x70, 0x8, 0x0, 0x36, 0xb1, 0x1a, 0x3}}");
            /// <summary>
            /// {aeac19e4-89ae-4508-b9b7-bb867abee2ed}
            /// </summary>
            public static readonly Guid PSGUID_DRM = new Guid("{0xaeac19e4, 0x89ae, 0x4508, {0xb9, 0xb7, 0xbb, 0x86, 0x7a, 0xbe, 0xe2, 0xed}}");
            /// <summary>
            /// {6444048f-4c8b-11d1-8b70-080036b11a03}
            /// </summary>
            public static readonly Guid PSGUID_IMAGESUMMARYINFORMATION = new Guid("{0x6444048f, 0x4c8b, 0x11d1,{0x8b, 0x70, 0x8, 0x00, 0x36, 0xb1, 0x1a, 0x03}}");
            /// <summary>
            /// {56a3372e-ce9c-11d2-9f0e-006097c686f6}
            /// </summary>
            public static readonly Guid PSGUID_MUSIC = new Guid("{0x56a3372e, 0xce9c, 0x11d2, {0x9f, 0xe, 0x0, 0x60, 0x97, 0xc6, 0x86, 0xf6}}");
            /// <summary>
            /// {49691c90-7e17-101a-a91c-08002b2ecda9}
            /// </summary>
            public static readonly Guid PSGUID_QUERY = new Guid("{0x49691c90,0x7e17,0x101a,{0xa9,0x1c,0x08,0x00,0x2b,0x2e,0xcd,0xa9}}");
            /// <summary>
            /// {b725f130-47ef-101a-a5f1-02608c9eebac}
            /// </summary>
            public static readonly Guid PSGUID_STORAGE = new Guid("{0xb725f130, 0x47ef, 0x101a, { 0xa5, 0xf1, 0x02, 0x60, 0x8c, 0x9e, 0xeb, 0xac }}");
            /// <summary>
            /// {64440491-4c8b-11d1-8b70-080036b11a03}
            /// </summary>
            public static readonly Guid PSGUID_VIDEO = new Guid("{0x64440491, 0x4c8b, 0x11d1, {0x8b, 0x70, 0x8, 0x0, 0x36, 0xb1, 0x1a, 0x3}}");


            /*****************************************
            * 
            * IStorage property sets
            * 
            *****************************************/
            public static readonly Guid FMTID_SummaryInformation = new Guid("{F29F85E0-4FF9-1068-AB91-08002B27B3D9}");
            public static readonly Guid FMTID_DocSummaryInformation = new Guid("{D5CDD502-2E9C-101B-9397-08002B2CF9AE}");
            public static readonly Guid FMTID_UserDefinedProperties = new Guid("{D5CDD505-2E9C-101B-9397-08002B2CF9AE}");

        }

        /// <summary>
        /// from Windows Search 2.X-3.X
        /// </summary>
        public static class PropSchema
        {
            public class PropSchemaEntity
            {
                public Guid Guid_WDS2X;
                public Guid Guid_WDS3X;
                public string name_WDS2X;
                public uint propId_WDS2X;
                public string name_WDS3X;
                public uint propId_WDS3X;
                public ulKind ulKind2X
                {
                    get
                    {
                        return string.IsNullOrEmpty(name_WDS2X)? ulKind.PRSPEC_PROPID : ulKind.PRSPEC_LPWSTR;
                    }
                }
                public ulKind ulKind3X
                {
                    get
                    {
                        return string.IsNullOrEmpty(name_WDS3X) ? ulKind.PRSPEC_PROPID : ulKind.PRSPEC_LPWSTR;
                    }
                }
            }


            public static PropSchemaEntity System_ItemFolderPathDisplay = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E3E0584C-B788-4A5A-BB20-7F5A44C9ACDD"), name_WDS2X = "DisplayFolder", propId_WDS3X = 6 };
            public static PropSchemaEntity System_ItemPathDisplay = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E3E0584C-B788-4A5A-BB20-7F5A44C9ACDD"), name_WDS2X = "DisplayUrl", propId_WDS3X = 7 };
            public static PropSchemaEntity System_ItemAuthors = new PropSchemaEntity { Guid_WDS2X = new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), Guid_WDS3X = new Guid("D0A04F0A-462A-48A4-BB2F-3706E88DBD7D"), propId_WDS2X = 4, propId_WDS3X = 100 };
            public static PropSchemaEntity System_ItemNamePrefix = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("D7313FF1-A77A-401C-8C99-3DBDD68ADD36"), name_WDS2X = "DocTitlePrefix", propId_WDS3X = 100 };
            public static PropSchemaEntity System_ItemName = new PropSchemaEntity { Guid_WDS2X = new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), Guid_WDS3X = new Guid("6B8DA074-3B5C-43BC-886F-0A2CDCE00B6F"), propId_WDS2X = 2, propId_WDS3X = 100 };
            public static PropSchemaEntity System_ItemTypeText = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("B725F130-47EF-101A-A5F1-02608C9EEBAC"), name_WDS2X = "FileExtDesc", propId_WDS3X = 4 };
            public static PropSchemaEntity System_ItemFolderNameDisplay = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("B725F130-47EF-101A-A5F1-02608C9EEBAC"), name_WDS2X = "FolderName", propId_WDS3X = 2 };
            public static PropSchemaEntity System_IsAttachment = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("F23F425C-71A1-4FA8-922F-678EA4A60408"), name_WDS2X = "IsAttachment", propId_WDS3X = 100 };
            public static PropSchemaEntity System_IsDeleted = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("5CDA5FC8-33EE-4FF3-9094-AE7BD8868C4D"), name_WDS2X = "IsDeleted", propId_WDS3X = 100 };
            public static PropSchemaEntity System_DateAccessed = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("B725F130-47EF-101A-A5F1-02608C9EEBAC"), name_WDS2X = "LastViewed", propId_WDS3X = 16 };
            public static PropSchemaEntity System_ItemParticipants = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("D4D0AA16-9948-41A4-AA85-D97FF9646993"), name_WDS2X = "People", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Kind = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("1E3EE840-BC2B-476C-8237-2ACD1A839B22"), name_WDS2X = "PerceivedType", propId_WDS3X = 3 };
            public static PropSchemaEntity System_KindText = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("F04BEF95-C585-4197-A2B7-DF46FDC9EE6D"), name_WDS2X = "PerceivedTypeName", propId_WDS3X = 100 };
            public static PropSchemaEntity System_ItemDate = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("F7DB74B4-4287-4103-AFBA-F1B13DCD75CF"), name_WDS2X = "PrimaryDate", propId_WDS3X = 100 };
            public static PropSchemaEntity System_DueDate = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("3F8472B5-E0AF-4DB2-8071-C53FE76AE7CE"), name_WDS2X = "DueDate", propId_WDS3X = 100 };
            public static PropSchemaEntity System_IsIncomplete = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("346C8BD1-2E6A-4C45-89A4-61B78E8E700F"), name_WDS2X = "IsIncomplete", propId_WDS3X = 100 };
            public static PropSchemaEntity System_IsFlaggedComplete = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("A6F360D2-55F9-48DE-B909-620E090A647C"), name_WDS2X = "IsFlaggedCompleted", propId_WDS3X = 100 };
            public static PropSchemaEntity System_IsFlagged = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("5DA84765-E3FF-4278-86B0-A27967FBDD03"), name_WDS2X = "IsFlagged", propId_WDS3X = 100 };
            public static PropSchemaEntity System_FlagStatusText = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("DC54FD2E-189D-4871-AA01-08C2F57A4ABC"), name_WDS2X = "FlagText", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Identity = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("A26F4AFC-7346-4299-BE47-EB1AE613139F"), name_WDS2X = "Identity", propId_WDS3X = 100 };
            public static PropSchemaEntity System_IsRead = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E3E0584C-B788-4A5A-BB20-7F5A44C9ACDD"), name_WDS2X = "IsRead", propId_WDS3X = 10 };
            public static PropSchemaEntity System_Importance = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E3E0584C-B788-4A5A-BB20-7F5A44C9ACDD"), name_WDS2X = "Importance", propId_WDS3X = 11 };
            public static PropSchemaEntity System_Search_ContainerHash = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("BCEEE283-35DF-4D53-826A-F36A3EEFC6BE"), name_WDS2X = "ContainerHash", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Search_Store = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("A06992B3-8CAF-4ED7-A547-B259E32AC9FC"), name_WDS2X = "Store", propId_WDS3X = 100 };
            public static PropSchemaEntity System_FileExtension = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E4F10A3C-49E6-405D-8288-A23BD4EEAA6C"), name_WDS2X = "FileExt", propId_WDS3X = 100 };
            public static PropSchemaEntity System_FileName10 = new PropSchemaEntity { Guid_WDS2X = new Guid("B725F130-47EF-101A-A5F1-02608C9EEBAC"), Guid_WDS3X = new Guid("41CF5AE0-F75A-4806-BD87-59C7D9248EB9"), propId_WDS2X = 10, propId_WDS3X = 100 };
            public static PropSchemaEntity System_FileName20 = new PropSchemaEntity { Guid_WDS2X = new Guid("B725F130-47EF-101A-A5F1-02608C9EEBAC"), Guid_WDS3X = new Guid("41CF5AE0-F75A-4806-BD87-59C7D9248EB9"), propId_WDS2X = 20, propId_WDS3X = 100 };
            public static PropSchemaEntity System_Message_AttachmentNames = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E3E0584C-B788-4A5A-BB20-7F5A44C9ACDD"), name_WDS2X = "AttachmentNames", propId_WDS3X = 21 };
            public static PropSchemaEntity System_Message_BccAddress = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E3E0584C-B788-4A5A-BB20-7F5A44C9ACDD"), name_WDS2X = "BccAddress", propId_WDS3X = 2 };
            public static PropSchemaEntity System_Message_BccName = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E3E0584C-B788-4A5A-BB20-7F5A44C9ACDD"), name_WDS2X = "BccName", propId_WDS3X = 3 };
            public static PropSchemaEntity System_Message_CcAddress = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E3E0584C-B788-4A5A-BB20-7F5A44C9ACDD"), name_WDS2X = "CcAddress", propId_WDS3X = 4 };
            public static PropSchemaEntity System_Message_CcName = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E3E0584C-B788-4A5A-BB20-7F5A44C9ACDD"), name_WDS2X = "CcName", propId_WDS3X = 5 };
            public static PropSchemaEntity System_Message_ConversationID = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("DC8F80BD-AF1E-4289-85B6-3DFC1B493992"), name_WDS2X = "ConversationID", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Message_FromAddress = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E3E0584C-B788-4A5A-BB20-7F5A44C9ACDD"), name_WDS2X = "FromAddress", propId_WDS3X = 13 };
            public static PropSchemaEntity System_Message_FromName = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E3E0584C-B788-4A5A-BB20-7F5A44C9ACDD"), name_WDS2X = "FromName", propId_WDS3X = 14 };
            public static PropSchemaEntity System_Message_IsFwdOrReply = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("9A9BC088-4F6D-469E-9919-E705412040F9"), name_WDS2X = "FwdRply", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Message_HasAttachments = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("9C1FCF74-2D97-41BA-B4AE-CB2E3661A6E4"), name_WDS2X = "HasAttach", propId_WDS3X = 8 };
            public static PropSchemaEntity System_Message_DateReceived = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E3E0584C-B788-4A5A-BB20-7F5A44C9ACDD"), name_WDS2X = "ReceivedDate", propId_WDS3X = 20 };
            public static PropSchemaEntity System_Message_ToAddress = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E3E0584C-B788-4A5A-BB20-7F5A44C9ACDD"), name_WDS2X = "ToAddress", propId_WDS3X = 16 };
            public static PropSchemaEntity System_Message_ToName = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E3E0584C-B788-4A5A-BB20-7F5A44C9ACDD"), name_WDS2X = "ToName", propId_WDS3X = 17 };
            public static PropSchemaEntity System_Communication_TaskStatusText = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("A6744477-C237-475B-A075-54F34498292A"), name_WDS2X = "TaskStatus", propId_WDS3X = 100 };
            public static PropSchemaEntity System_EndDate = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("C75FAA05-96FD-49E7-9CB4-9F601082D553"), name_WDS2X = "EndDate", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Calendar_IsRecurring = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("315B9C8D-80A9-4EF9-AE16-8E746DA51D70"), name_WDS2X = "IsRecurring", propId_WDS3X = 100 };
            public static PropSchemaEntity System_StartDate = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("48FD6EC8-8A12-4CDF-A03E-4EC5A511EDDE"), name_WDS2X = "StartDate", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Calendar_Duration = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("293CA35A-09AA-4DD2-B180-1FE245728A52"), name_WDS2X = "Duration", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Calendar_IsOnline = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("BFEE9149-E3E2-49A7-A862-C05988145CEC"), name_WDS2X = "IsOnline", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Calendar_Location = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("F6272D18-CECC-40B1-B26A-3911717AA7BD"), name_WDS2X = "Location", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_Anniversary = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("9AD5BADB-CEA7-4470-A03D-B84E51B9949E"), name_WDS2X = "Anniversary", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_AssistantName = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("CD102C9C-5540-4A88-A6F6-64E4981C8CD1"), name_WDS2X = "AssistantName", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_AssistantTelephone = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("9A93244D-A7AD-4FF8-9B99-45EE4CC09AF6"), name_WDS2X = "AssistantTelephone", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_Birthday = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("176DC63C-2688-4E89-8143-A347800F25E9"), name_WDS2X = "Birthday", propId_WDS3X = 47 };
            public static PropSchemaEntity System_Contact_BusinessAddressCity = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("402B5934-EC5A-48C3-93E6-85E86A2D934E"), name_WDS2X = "BusinessAddressCity", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_BusinessAddressPostalCode = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E1D4A09E-D758-4CD1-B6EC-34A8B5A73F80"), name_WDS2X = "BusinessAddressPostalCode", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_BusinessAddressPostOfficeBox = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("BC4E71CE-17F9-48D5-BEE9-021DF0EA5409"), name_WDS2X = "BusinessAddressPostOfficeBox", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_BusinessAddressState = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("446F787F-10C4-41CB-A6C4-4D0343551597"), name_WDS2X = "BusinessAddressState", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_BusinessAddressStreet = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("DDD1460F-C0BF-4553-8CE4-10433C908FB0"), name_WDS2X = "BusinessAddressStreet", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_BusinessAddressCountry = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("B0B87314-FCF6-4FEB-8DFF-A50DA6AF561C"), name_WDS2X = "BusinessAddressCountry", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_BusinessHomePage = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("56310920-2491-4919-99CE-EADB06FAFDB2"), name_WDS2X = "BusinessHomePage", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_CallbackTelephone = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("BF53D1C3-49E0-4F7F-8567-5A821D8AC542"), name_WDS2X = "CallbackTelephone", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_CarTelephone = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("8FDC6DEA-B929-412B-BA90-397A257465FE"), name_WDS2X = "CarTelephone", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Keywords = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), name_WDS2X = "Categories", propId_WDS3X = 5 };
            public static PropSchemaEntity System_Contact_Children = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("D4729704-8EF1-43EF-9024-2BD381187FD5"), name_WDS2X = "Children", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_CompanyMainTelephone = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("8589E481-6040-473D-B171-7FA89C2708ED"), name_WDS2X = "CompanyMainTelephone", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_EmailAddress = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("F8FA7FA3-D12B-4785-8A4E-691A94F7A3E7"), name_WDS2X = "EmailAddress", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_EmailName = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("CC6F4F24-6083-4BD4-8754-674D0DE87AB8"), name_WDS2X = "EmailName", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_FirstName = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("14977844-6B49-4AAD-A714-A4513BF60460"), name_WDS2X = "FirstName", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_FullName = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("635E9051-50A5-4BA2-B9DB-4ED056C77296"), name_WDS2X = "FullName", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_Gender = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("3C8CEE58-D4F0-4CF9-B756-4E5D24447BCD"), name_WDS2X = "Gender", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_Hobbies = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("5DC2253F-5E11-4ADF-9CFE-910DD01E3E70"), name_WDS2X = "Hobby", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_HomeAddressCity = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("176DC63C-2688-4E89-8143-A347800F25E9"), name_WDS2X = "HomeAddressCity", propId_WDS3X = 65 };
            public static PropSchemaEntity System_Contact_HomeAddressCountry = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("08A65AA1-F4C9-43DD-9DDF-A33D8E7EAD85"), name_WDS2X = "HomeAddressCountry", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_HomeAddressPostalCode = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("8AFCC170-8A46-4B53-9EEE-90BAE7151E62"), name_WDS2X = "HomeAddressPostalCode", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_HomeAddressState = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("C89A23D0-7D6D-4EB8-87D4-776A82D493E5"), name_WDS2X = "HomeAddressState", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_HomeAddressStreet = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("0ADEF160-DB3F-4308-9A21-06237B16FA2A"), name_WDS2X = "HomeAddressStreet", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_HomeFaxNumber = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("660E04D6-81AB-4977-A09F-82313113AB26"), name_WDS2X = "HomeFaxNumber", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_BusinessFaxNumber = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("91EFF6F3-2E27-42CA-933E-7C999FBE310B"), name_WDS2X = "BusinessFaxNumber", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_HomeTelephone = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("176DC63C-2688-4E89-8143-A347800F25E9"), name_WDS2X = "HomeTelephone", propId_WDS3X = 20 };
            public static PropSchemaEntity System_Contact_IMAddress = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("D68DBD8A-3374-4B81-9972-3EC30682DB3D"), name_WDS2X = "IMAddress", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_JobTitle = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("176DC63C-2688-4E89-8143-A347800F25E9"), name_WDS2X = "JobTitle", propId_WDS3X = 6 };
            public static PropSchemaEntity System_Contact_MiddleName = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("176DC63C-2688-4E89-8143-A347800F25E9"), name_WDS2X = "MiddleName", propId_WDS3X = 71 };
            public static PropSchemaEntity System_Contact_MobileTelephone = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("176DC63C-2688-4E89-8143-A347800F25E9"), name_WDS2X = "MobileTelephone", propId_WDS3X = 35 };
            public static PropSchemaEntity System_Contact_NickName = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("176DC63C-2688-4E89-8143-A347800F25E9"), name_WDS2X = "NickName", propId_WDS3X = 74 };
            public static PropSchemaEntity System_Contact_OfficeLocation = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("176DC63C-2688-4E89-8143-A347800F25E9"), name_WDS2X = "Office", propId_WDS3X = 7 };
            public static PropSchemaEntity System_Contact_BusinessTelephone = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("6A15E5A0-0A1E-4CD7-BB8C-D2F1B0C929BC"), name_WDS2X = "OfficeTelephone", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_PagerTelephone = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("D6304E01-F8F5-4F45-8B15-D024A6296789"), name_WDS2X = "PagerTelephone", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_LastName = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("8F367200-C270-457C-B1D4-E07C5BCD90C7"), name_WDS2X = "LastName", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_PersonalTitle = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("176DC63C-2688-4E89-8143-A347800F25E9"), name_WDS2X = "PersonalTitle", propId_WDS3X = 69 };
            public static PropSchemaEntity System_Contact_PrimaryTelephone = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("176DC63C-2688-4E89-8143-A347800F25E9"), name_WDS2X = "PrimaryTelephone", propId_WDS3X = 25 };
            public static PropSchemaEntity System_Contact_Profession = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("7268AF55-1CE4-4F6E-A41F-B6E4EF10E4A9"), name_WDS2X = "Profession", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_SpouseName = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("9D2408B6-3167-422B-82B0-F583B7A7CFE3"), name_WDS2X = "Spouse", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_Suffix = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("176DC63C-2688-4E89-8143-A347800F25E9"), name_WDS2X = "Suffix", propId_WDS3X = 73 };
            public static PropSchemaEntity System_Contact_TelexNumber = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("C554493C-C1F7-40C1-A76C-EF8C0614003E"), name_WDS2X = "TelexNumber", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_TTYTDDTelephone = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("AAF16BAC-2B55-45E6-9F6D-415EB94910DF"), name_WDS2X = "TTYTDDTelephone", propId_WDS3X = 100 };
            public static PropSchemaEntity System_Contact_WebPage = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("E3E0584C-B788-4A5A-BB20-7F5A44C9ACDD"), name_WDS2X = "WebPage", propId_WDS3X = 18 };
            public static PropSchemaEntity System_Media_Duration = new PropSchemaEntity { Guid_WDS2X = new Guid("56A3372E-CE9C-11D2-9F0E-006097C686F6"), Guid_WDS3X = new Guid("64440490-4C8B-11D1-8B70-080036B11A03"), propId_WDS2X = 8, propId_WDS3X = 3 };
            public static PropSchemaEntity System_Photo_DateTaken = new PropSchemaEntity { Guid_WDS2X = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), Guid_WDS3X = new Guid("14B81DA1-0135-4D31-96D9-6CBFC9671A99"), name_WDS2X = "DateTaken", propId_WDS3X = 36867 };
            public static PropSchemaEntity System_Image_ColorSpace = new PropSchemaEntity { Guid_WDS2X = new Guid("6444048F-4C8B-11D1-8B70-080036B11A03"), Guid_WDS3X = new Guid("14B81DA1-0135-4D31-96D9-6CBFC9671A99"), propId_WDS2X = 8, propId_WDS3X = 40961 };
            public static PropSchemaEntity System_Image_Compression = new PropSchemaEntity { Guid_WDS2X = new Guid("6444048F-4C8B-11D1-8B70-080036B11A03"), Guid_WDS3X = new Guid("14B81DA1-0135-4D31-96D9-6CBFC9671A99"), propId_WDS2X = 9, propId_WDS3X = 259 };
            public static PropSchemaEntity System_Video_StreamNumber = new PropSchemaEntity { Guid_WDS2X = new Guid("64440491-4C8B-11D1-8B70-080036B11A03"), Guid_WDS3X = new Guid("64440491-4C8B-11D1-8B70-080036B11A03"), propId_WDS2X = 7, propId_WDS3X = 11 };


            // TestDriven.Net 
            //static void Read()
            //{
            //    var fs = File.CreateText(@"C:\Users\?\Desktop\schema~.txt");
            //    foreach (var line in File.ReadAllLines(@"C:\Users\?\Desktop\schema.txt"))
            //    {
            //        var ar = line.Split(new []{" "}, StringSplitOptions.RemoveEmptyEntries);
            //        int r1;
            //        fs.WriteLine(string.Format(@"public static PropSchemaEntity {0} = new PropSchemaEntity{{Guid_WDS2X = new Guid(""{1}""), Guid_WDS3X = new Guid(""{2}""), {3} = {4}, {5} = {6}}};",
            //            ar[0].Replace('.', '_'), ar[1].Split('/')[0], ar[2].Split('/')[0], int.TryParse(ar[1].Split('/')[1], out r1) ? "propId_WDS2X" : "name_WDS2X", int.TryParse(ar[1].Split('/')[1], out r1) ? (object)r1 : "\"" + ar[1].Split('/')[1].Trim('\'') + "\"",
            //            int.TryParse(ar[2].Split('/')[1], out r1) ? "propId_WDS3X" : "name_WDS3X", int.TryParse(ar[2].Split('/')[1], out r1) ? (object)r1 : "\"" + ar[2].Split('/')[1].Trim('\'') + "\""
            //                                )
            //            );
            //    }
            //    fs.Close();
            //}

        }


    }
}
