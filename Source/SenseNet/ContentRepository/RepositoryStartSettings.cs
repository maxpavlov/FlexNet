using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SenseNet.ContentRepository
{
    /// <summary>
    /// Controls the startup sequence of the Repository. This class is used as a parameter of the Repository.Start(RepositoryStartSettings) method.
    /// The startup control information is also available (in read only way) in the RepositoryInstace which the Repository.Start method returns to.
    /// </summary>
    public class RepositoryStartSettings
    {
        private const string CONFIGRESTOREINDEXKEY = "RestoreIndex";
        public static bool? ConfigRestoreIndex
        {
            get
            {
                var setting = ConfigurationManager.AppSettings[CONFIGRESTOREINDEXKEY];
                if (string.IsNullOrEmpty(setting))
                    return null;
                
                bool value;
                if (Boolean.TryParse(setting, out value))
                    return value;

                return null;
            }
        }

        /// <summary>
        /// Provides the control information of the startup sequence. 
        /// The instance of this class is the clone of the RepositoryStartSettings that was passed the Repository.Start(RepositoryStartSettings) method.
        /// </summary>
        public class ImmutableRepositoryStartSettings : RepositoryStartSettings
        {
            private new bool _startLuceneManager;
            private new bool _backupIndexAtTheEnd;
            private new bool _restoreIndex;
            private new bool _startWorkflowEngine;
            private new string _pluginsPath;
            private new string _indexPath;
            private new System.IO.TextWriter _console;

            /// <summary>
            /// Gets a value that is 'true' if your tool uses the Content search and any modification features (e.g. save, move etc.). 'True' is the default.
            /// </summary>
            public new bool StartLuceneManager { get { return _startLuceneManager; } }
            /// <summary>
            /// Gets a value that is 'true' if the Lucene index will be backed up before your tool exits. Default: false
            /// </summary>
            public new bool BackupIndexAtTheEnd { get { return _backupIndexAtTheEnd; } }
            /// <summary>
            /// Gets a value that is 'true' if your tool needs a fresh index in the startup time. 'True' is the default. If StartLuceneManager = false, value of this property is irrelevant.
            /// </summary>
            public new bool RestoreIndex { get { return _restoreIndex; } }
            /// <summary>
            /// Gets or sets a value that is 'true' if your tool enables the running of workflow engine. 'True' is the default.
            /// </summary>
            public new bool StartWorkflowEngine { get { return _startWorkflowEngine; } }
            /// <summary>
            /// Gets a local directory path of plugins if it is different from your tool's path. Default is null that means the plugins are placed in the appdomain's working directory.
            /// </summary>
            public new string PluginsPath { get { return _pluginsPath; } }
            /// <summary>
            /// Gets a local directory path of index if it is different from configured path. Default is false that means the application uses the configured index path.
            /// </summary>
            public new string IndexPath { get { return _indexPath; } }
            /// <summary>
            /// Gets a System.IO.TextWriter instance. Can be null. If it is not null, the startup sequence will be traced to given writer.
            /// </summary>
            public new System.IO.TextWriter Console { get { return _console; } }

            internal ImmutableRepositoryStartSettings(RepositoryStartSettings settings)
            {
                _startLuceneManager = settings.StartLuceneManager;
                _backupIndexAtTheEnd = settings.BackupIndexAtTheEnd;
                var configRestoreIndex = RepositoryStartSettings.ConfigRestoreIndex;
                _restoreIndex = configRestoreIndex.HasValue ? configRestoreIndex.Value : settings.RestoreIndex;
                _startWorkflowEngine = settings.StartWorkflowEngine;
                _console = settings.Console;
                _pluginsPath = settings.PluginsPath;
                _indexPath = settings.IndexPath;
            }
        }

        internal static readonly RepositoryStartSettings Default = new RepositoryStartSettings();

        private bool _startLuceneManager = true;
        private bool _backupIndexAtTheEnd = false;
        private bool _restoreIndex = true;
        private bool _startWorkflowEngine = true;
        private string _pluginsPath;
        private string _indexPath;
        private System.IO.TextWriter _console;

        /// <summary>
        /// Gets or sets a value that is 'true' if your tool uses the Content search and any modification features (e.g. save, move etc.). 'True' is the default.
        /// </summary>
        /// <remarks>
        /// If your tool needs to run Lucene and its running is postponed (StartLuceneManager = false), call the RepositoryInstance.StartLucene() method.
        /// </remarks>
        public virtual bool StartLuceneManager
        {
            get { return _startLuceneManager; }
            set { _startLuceneManager = value; }
        }
        /// <summary>
        /// Gets or sets a value that is 'true' if the Lucene index will be backed up before your tool exits. Default: false
        /// </summary>
        public virtual bool BackupIndexAtTheEnd
        {
            get { return _backupIndexAtTheEnd; }
            set { _backupIndexAtTheEnd = value; }
        }
        /// <summary>
        /// Gets or sets a value that is 'true' if your tool needs a fresh index in the startup time. 'True' is the default. If StartLuceneManager = false, value of this property is irrelevant.
        /// </summary>
        public virtual bool RestoreIndex
        {
            get { return _restoreIndex; }
            set { _restoreIndex = value; }
        }
        /// <summary>
        /// Gets or sets a value that is 'true' if your tool enables the running of workflow engine. 'True' is the default.
        /// </summary>
        /// <remarks>
        /// If your tool needs to run the workflow engine and its running is postponed (StartWorkflowEngine = false), call the RepositoryInstance.StartWorkflowEngine() method.
        /// </remarks>
        public virtual bool StartWorkflowEngine
        {
            get { return _startWorkflowEngine; }
            set { _startWorkflowEngine = value; }
        }
        /// <summary>
        /// Gets or sets a local directory path of plugins if it is different from your tool's path. Default is null that means the plugins are placed in the appdomain's working directory.
        /// </summary>
        public virtual string PluginsPath
        {
            get { return _pluginsPath; }
            set { _pluginsPath = value; }
        }
        /// <summary>
        /// Gets or sets a local directory path of index if it is different from configured path. Default is false that means the application uses the configured index path.
        /// </summary>
        public virtual string IndexPath
        {
            get { return _indexPath; }
            set { _indexPath = value; }
        }
        /// <summary>
        /// Gets or set a System.IO.TextWriter instance. Can be null. If it is not null, the startup sequence will be traced to given writer.
        /// </summary>
        public virtual System.IO.TextWriter Console
        {
            get { return _console; }
            set { _console = value; }
        }
    }
}
