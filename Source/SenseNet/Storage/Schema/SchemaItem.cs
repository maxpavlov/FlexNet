using System;
using System.Collections.Generic;
using System.Text;

namespace SenseNet.ContentRepository.Storage.Schema
{
	public abstract class SchemaItem
	{
		//============================================================================== Fields

		private ISchemaRoot _schemaRoot;
		private int _id;
		private string _name;

		//============================================================================== Properties

		internal ISchemaRoot SchemaRoot
		{
			get { return _schemaRoot; }
		}
		public int Id
		{
			get { return _id; }
		}
		public virtual string Name
		{
			get { return _name; }
		}

		//============================================================================== Construction

		internal SchemaItem(ISchemaRoot schemaRoot, string name, int id)
		{
			_id = id;
			_name = name;
			_schemaRoot = schemaRoot;
		}

		//============================================================================== Methods

		public override string ToString()
		{
			return String.Concat(this.GetType().Name, ", '", this.Name, "'");
		}
	}
}