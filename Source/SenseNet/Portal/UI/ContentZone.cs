using System;
using System.Collections.Generic;
using System.Text;

namespace SenseNet.Portal.UI
{
	internal class ContentZone
	{
		private string _contentPlaceHolderID;
		public string ContentPlaceHolderID
		{
			get { return _contentPlaceHolderID; }
			set { _contentPlaceHolderID = value; }
		}

		private string _ID;
		public string ID
		{
			get { return _ID; }
			set { _ID = value; }
		}

		private string _innerXml;
		public string InnerXml
		{
			get { return _innerXml; }
			set { _innerXml = value; }
		}

		private string _zoneID;
		public string ZoneID
		{
			get { return _zoneID; }
			set { _zoneID = value; }
		}

		public ContentZone(string contentPlaceHolderID, string ID, string innerXml, string zoneID)
		{
			_contentPlaceHolderID = contentPlaceHolderID;
			_ID = ID;
			_innerXml = innerXml;
			_zoneID = zoneID;
		}
	}
}