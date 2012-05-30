using System;
using System.Collections.Generic;

using System.Linq;
using  SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.ContentRepository.Tests.ContentHandlers
{
	[ContentHandler]
	public class AutomobileHandler : GenericContent
	{
		public AutomobileHandler(Node parent) : this(parent, "Automobile") { }
		public AutomobileHandler(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected AutomobileHandler(NodeToken token) : base(token) { }

		[RepositoryProperty("Manufacturer")]
		public string Manufacturer
		{
			get { return this.GetProperty<string>("Manufacturer"); }
			set { this["Manufacturer"] = value; }
		}
		public override object GetProperty(string name)
		{
			switch (name)
			{
			    case "Manufacturer":
			        return this.Manufacturer;
			    default:
			        return base.GetProperty(name);
			}
		}
		public override void SetProperty(string name, object value)
		{
			switch (name)
			{
			    case "Manufacturer":
			        this.Manufacturer = (string)value;
					break;
			    default:
			        base.SetProperty(name, value);
					break;
			}
		}

		public const string ContentTypeDefinition = @"<?xml version='1.0' encoding='utf-8'?>
<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
	<DisplayName>Automobile [demo]</DisplayName>
	<Description>This is a demo automobile node definition</Description>
	<Icon>automobile.gif</Icon>
	<Fields>
		<Field name='Manufacturer' type='ShortText'>
			<DisplayName>Manufacturer's name</DisplayName>
			<Description>Enter the manufacturer's name</Description>
			<Icon>icon.gif</Icon>
			<Bind property='Manufacturer' />
			<Configuration>
				<Compulsory>true</Compulsory>
				<MaxLength>100</MaxLength>
				<Format>TitleCase</Format>
			</Configuration>
		</Field>
		<!--
		<Field name='Color' type='Color'>
		</Field>
		-->
	</Fields>
</ContentType>
";
		public const string ExtendedCTD = @"<?xml version='1.0' encoding='utf-8'?>
<ContentType name='Automobile' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.AutomobileHandler' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
	<DisplayName>Automobile [demo]</DisplayName>
	<Description>This is a demo automobile node definition</Description>
	<Icon>automobile.gif</Icon>
	<Fields>
		<Field name='Manufacturer' type='ShortText' handler='SenseNet.ContentRepository.Fields.ShortTextField'>
			<DisplayName>Manufacturer's name</DisplayName>
			<Description>Enter the manufacturer's name</Description>
			<Icon>icon.gif</Icon>
			<Bind property='Manufacturer' />
			<Configuration>
				<Compulsory>true</Compulsory>
				<MaxLength>100</MaxLength>
				<Format>TitleCase</Format>
			</Configuration>
		</Field>
		<Field name='Driver' type='ShortText'>
			<DisplayName>Driver's name</DisplayName>
			<Description>Enter the driver's name</Description>
			<Icon>icon.gif</Icon>
			<Bind property='Driver' />
			<Configuration>
				<Compulsory>true</Compulsory>
				<MaxLength>100</MaxLength>
			</Configuration>
		</Field>
	</Fields>
</ContentType>
";

	}
}