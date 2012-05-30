using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace SenseNet.Tools.EventLogCreator
{
	internal class EventLogDeployer
	{
		private string _machineName = ".";

		private int _logSizeInMegaBytes = 15;
		public int LogSizeInMegaBytes
		{
			get
			{
				return this._logSizeInMegaBytes;
			}
			set
			{
				this._logSizeInMegaBytes = value;
			}
		}

		string _logName;
		public string LogName
		{
			get
			{
				return this._logName;
			}
		}

		IEnumerable<string> _sources;
		public IEnumerable<string> Sources
		{
			get
			{
				return this._sources;
			}
		}

		public EventLogDeployer(string logName, IEnumerable<string> sources)
		{
			_logName = logName;
			_sources = sources;
		}
		public EventLogDeployer(string logName, IEnumerable<string> sources, string machineName) : this(logName, sources)
		{
			_machineName = machineName;
		}

		private void DeleteSource(string source)
		{
			string sourceBelongsToLog = EventLog.LogNameFromSourceName(source, _machineName);
			if (sourceBelongsToLog != _logName)
			{
				Console.WriteLine("Skipping '{0}' - it belongs to log '{1}' rather than '{2}'.", source, sourceBelongsToLog, _logName);
				return;
			}

			EventLog.DeleteEventSource(source, _machineName);
		}
		public void Delete()
		{
			string builtInSourceName = string.Concat(_logName, "Instrumentation");
			bool sourcesContainedBuildIn = false;

			// create sources
			foreach (string source in _sources)
			{
				DeleteSource(source);
				if (source == builtInSourceName)
					sourcesContainedBuildIn = true;
			}

			if (!sourcesContainedBuildIn)
			{
				DeleteSource(builtInSourceName);
			}

			if (EventLog.Exists(_logName, _machineName))
			{
				EventLog.Delete(_logName, _machineName);
			}
		}

		private void CreateSource(string source)
		{
			string sourceBelongsToLog = EventLog.LogNameFromSourceName(source, _machineName);
			if (sourceBelongsToLog != string.Empty)
			{
				Console.WriteLine("Skipping '{0}' - source belongs to log '{1}' already.", source, sourceBelongsToLog);
				return;
			}

			EventLog.CreateEventSource(source, _logName, _machineName);
		}
		public void Create()
		{
			string builtInSourceName = string.Concat(_logName, "Instrumentation");
			bool sourcesContainedBuildIn = false;

			// create sources
			foreach (string source in _sources)
			{
				CreateSource(source);
				if (source == builtInSourceName)
					sourcesContainedBuildIn = true;
			}

			if (!sourcesContainedBuildIn)
			{
				CreateSource(builtInSourceName);
			}

			using (EventLog eventLog = new EventLog(_logName, _machineName, builtInSourceName))
			{
				eventLog.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 14);
				eventLog.MaximumKilobytes = _logSizeInMegaBytes * 1024;
				eventLog.WriteEntry("Log created.", EventLogEntryType.Information);
				eventLog.Dispose();
			}
		}
	}
}