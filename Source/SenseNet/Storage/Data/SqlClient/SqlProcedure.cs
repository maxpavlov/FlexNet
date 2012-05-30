using System;
using System.Data;
using System.Data.SqlClient;

namespace SenseNet.ContentRepository.Storage.Data.SqlClient
{
    internal class SqlProcedure : IDataProcedure, IDisposable
    {
        private bool _disposed;
        private SqlCommand _cmd;
        private SqlConnection _conn;
        private bool _useTransaction;

        public CommandType CommandType
        {
            get
            {
                if (_cmd == null)
                    _cmd = CreateCommand();
                return _cmd.CommandType;
            }
            set
            {
                if (_cmd == null)
                    _cmd = CreateCommand();
                _cmd.CommandType = value;
            }
        }
        public string CommandText
        {
            get
            {
                if (_cmd == null)
                    _cmd = CreateCommand();
                return _cmd.CommandText;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (_cmd == null)
                    _cmd = CreateCommand();
                _cmd.CommandText = value;
            }
        }
        public SqlParameterCollection Parameters
        {
            get
            {
                if (_cmd == null)
                    _cmd = CreateCommand();
                return _cmd.Parameters;
            }
        }

        internal SqlProcedure() { }
        [Obsolete("Use DataProvider.CreateDataProcedure static method instead", true)]
        public SqlProcedure(string commandText)
        {
            if (commandText == null)
                throw new ArgumentNullException("commandText");
            _cmd = CreateCommand();
            _cmd.CommandText = commandText;
            // Get active Connection object from TransactionProvider or create a non-opened local instance
        }
        private SqlCommand CreateCommand()
        {
            SqlTransaction tran = null;
            Transaction provider = (Transaction)TransactionScope.Provider;
            SqlConnection tranConn = (provider != null) ? provider.Connection : null;
            if (tranConn != null)
            {
                _conn = tranConn;
                tran = provider.Tran;
                _useTransaction = true;
            }
            else
            {
                _conn = new SqlConnection(RepositoryConfiguration.ConnectionString);
            }

            var cmd = new SqlCommand();
            cmd.Connection = _conn;
            cmd.Transaction = tran;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = RepositoryConfiguration.SqlCommandTimeout;

            return cmd;
        }

        private void StartConnection()
        {
            if (_conn == null)
                throw new InvalidOperationException("SqlProcedure has been closed.");

            if (_conn.State == ConnectionState.Closed)
                _conn.Open();
        }
        public void DeriveParameters()
        {
            StartConnection();
            SqlCommandBuilder.DeriveParameters(_cmd);
        }
        public SqlDataReader ExecuteReader()
        {
            StartConnection();
            return _cmd.ExecuteReader();
        }
        public SqlDataReader ExecuteReader(CommandBehavior behavior)
        {
            StartConnection();
            return _cmd.ExecuteReader(behavior);
        }
        public object ExecuteScalar()
        {
            StartConnection();
            return _cmd.ExecuteScalar();
        }
        public int ExecuteNonQuery()
        {
            StartConnection();
            return _cmd.ExecuteNonQuery();
        }
        private void Close()
        {
            if (!_useTransaction)
            {
                if (_conn != null && _conn.State == ConnectionState.Open)
                    _conn.Close();
                _conn = null;
            }
            _cmd = null;
        }

        //====================================================================== IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
                if (disposing)
                    this.Close();
            _disposed = true;
        }
        ~SqlProcedure()
        {
            Dispose(false);
        }

        //====================================================================== IDataProcedure Members

        CommandType IDataProcedure.CommandType
        {
            get { return this.CommandType; }
            set { this.CommandType = value; }
        }
        string IDataProcedure.CommandText
        {
            get { return this.CommandText; }
            set { this.CommandText = value; }
        }
        System.Data.Common.DbParameterCollection IDataProcedure.Parameters
        {
            get { return this.Parameters; }
        }
        void IDataProcedure.DeriveParameters()
        {
            DeriveParameters();
        }
        System.Data.Common.DbDataReader IDataProcedure.ExecuteReader()
        {
            return ExecuteReader();
        }
        System.Data.Common.DbDataReader IDataProcedure.ExecuteReader(CommandBehavior behavior)
        {
            return ExecuteReader(behavior);
        }
        object IDataProcedure.ExecuteScalar()
        {
            return ExecuteScalar();
        }
        int IDataProcedure.ExecuteNonQuery()
        {
            return ExecuteNonQuery();
        }
    }
}