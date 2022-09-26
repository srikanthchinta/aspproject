using System;using Shell.WRFM.Global.Utilities;
using System.Data;
using System.Collections.Generic;
using Shell.WRFM.Global.Web.Log;

namespace Shell.WRFM.Global.Web.DataAccess
{
    /// <summary>
    /// DBManager
    /// </summary>
    public sealed class DBManager : IDBManager, IDisposable
    {
        private IDbConnection idbConnection;
        private IDataReader idataReader;
        private IDataAdapter idataAdapter;
        private IDbCommand idbCommand;
        private DataProvider providerType;
        private IDbTransaction idbTransaction = null;
        private List<IDbDataParameter> idbParameters = null;
        private string strConnection;
        /// <summary>
        /// Logger
        /// </summary>
        public Log.Log DBLogger;

        DBManager(Log.Log log)
        {
            DBLogger = log;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DBManager"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="log">The log.</param>
        public DBManager(string provider, string connectionString, Log.Log log)
        {
            this.providerType = String.IsNullOrEmpty(provider) ? DataProvider.SqlServer : (DataProvider)Enum.Parse(typeof(DataProvider), provider);
            this.strConnection = connectionString;
            DBLogger = log;
        }
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public IDBManager Clone()
        {
            DBManager manager = new DBManager(DBLogger);
            manager.providerType = this.providerType;
            manager.strConnection = this.strConnection;
            return manager;
        }
        /// <summary>
        /// Gets the connection.
        /// </summary>
        public IDbConnection Connection
        {
            get
            {
                return idbConnection;
            }
        }

        /// <summary>
        /// Gets the data reader.
        /// </summary>
        public IDataReader DataReader
        {
            get
            {
                return idataReader;
            }
            set
            {
                idataReader = value;
            }
        }

        /// <summary>
        /// Gets the data adapter.
        /// </summary>
        public IDataAdapter DataAdapter
        {
            get
            {
                if (idataAdapter == null)
                    idataAdapter = DBManagerFactory.GetDataAdapter(this.ProviderType);

                return idataAdapter;
            }
            set
            {
                idataAdapter = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the provider.
        /// </summary>
        /// <value>
        /// The type of the provider.
        /// </value>
        public DataProvider ProviderType
        {
            get
            {
                return providerType;
            }
            set
            {
                providerType = value;
            }
        }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        public string ConnectionString
        {
            get
            {
                return strConnection;
            }
            set
            {
                strConnection = value;
            }
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        public IDbCommand Command
        {
            get
            {
                return idbCommand;
            }
        }

        /// <summary>
        /// Gets the transaction.
        /// </summary>
        public IDbTransaction Transaction
        {
            get
            {
                return idbTransaction;
            }
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public List<IDbDataParameter> Parameters
        {
            get
            {
                if (idbParameters == null)
                    return null;
                return idbParameters;
            }
        }

        /// <summary>
        /// Opens this instance.
        /// </summary>
        public void Open()
        {
            idbConnection =
            DBManagerFactory.GetConnection(this.providerType);
            idbConnection.ConnectionString = this.ConnectionString;
            if (idbConnection.State != ConnectionState.Open)
                idbConnection.Open();
            this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType);
            this.idbParameters = null;
            if(DBLogger!=null)
                DBLogger.Trace(DreamConstants.TRACEBEGIN + DreamConstants.DBEXECUTIONCONOPEN);
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            if (idbConnection != null && idbConnection.State != ConnectionState.Closed)
            {
                idbConnection.Close();
            }
            if (DBLogger != null)
                DBLogger.Trace(DreamConstants.TRACEEND + DreamConstants.DBEXECUTIONCONCLOSE);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            this.Close();
            this.idbCommand = null;
            this.idbTransaction = null;            
            this.idbParameters = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Adds the output parameters.
        /// </summary>
        /// <param name="paramName">Name of the param.</param>
        /// <param name="dbType">Type of the db.</param>
        /// <returns></returns>
        public IDbDataParameter AddOutputParameters(string paramName, DbType dbType)
        {
            IDbDataParameter param = (IDbDataParameter)DBManagerFactory.GetParameter(this.ProviderType);
            param.ParameterName = paramName;
            param.DbType = dbType;
            param.Direction = ParameterDirection.ReturnValue;

            if (idbParameters == null)
            {
                idbParameters = new List<IDbDataParameter>();
            }

            idbParameters.Add(param);
            return param;
        }
        /// <summary>
        /// Adds the parameters.
        /// </summary>
        /// <param name="paramName">Name of the param.</param>
        /// <param name="objValue">The obj value.</param>
        public void AddParameters(string paramName, object objValue)
        {
            AddParameters(paramName, objValue, ParameterDirection.Input);
        }
        /// <summary>
        /// Adds the parameters.
        /// </summary>
        /// <param name="paramName">Name of the param.</param>
        /// <param name="objValue">The obj value.</param>
        /// <param name="dir">The dir.</param>
        public void AddParameters(string paramName, object objValue, ParameterDirection dir)
        {
            IDbDataParameter param = (IDbDataParameter)DBManagerFactory.GetParameter(this.ProviderType);
            param.ParameterName = paramName;
            param.Value = objValue;
            param.Direction=dir;

            if (idbParameters == null)
            {
                idbParameters = new List<IDbDataParameter>();
            }

            idbParameters.Add(param);
        }
        /// <summary>
        /// Gets the parameter.
        /// </summary>
        /// <param name="paramName">Name of the param.</param>
        /// <returns></returns>
        public IDbDataParameter GetParameter(string paramName)
        {
            foreach (IDbDataParameter idbParameter in Parameters)
            {
                if (idbParameter.ParameterName == paramName)
                    return idbParameter;
            }
            return null;
        }

        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <param name="paramName">Name of the param.</param>
        /// <returns></returns>
        public object GetParameterValue(string paramName)
        {
            foreach (IDbDataParameter idbParameter in Parameters)
            {
                if (idbParameter.ParameterName == paramName)
                    return idbParameter.Value;
            }
            return null;
        }
        /// <summary>
        /// Begins the transaction.
        /// </summary>
        public void BeginTransaction()
        {
            if (this.idbTransaction == null)
                idbTransaction = DBManagerFactory.GetTransaction(this.ProviderType);
            this.idbCommand.Transaction = idbTransaction;
        }

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        public void CommitTransaction()
        {
            if (this.idbTransaction != null)
                this.idbTransaction.Commit();
            idbTransaction = null;
        }

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns></returns>
        public IDataReader ExecuteReader(CommandType commandType, string commandText)
        {
            using (TraceBlock tb = new TraceBlock(DBLogger,DreamConstants.DBEXECUTIONEXECUTEREADER, commandText))
            {
                using (this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType))
                {
                    idbCommand.Connection = this.Connection;
                    DBManager.PrepareCommand(idbCommand, this.Connection, this.Transaction, commandType, commandText, this.Parameters);
                    this.DataReader = idbCommand.ExecuteReader();
                    idbCommand.Parameters.Clear();
                }
            }
            return this.DataReader;
        }

        /// <summary>
        /// Closes the reader.
        /// </summary>
        public void CloseReader()
        {
            if (this.DataReader != null)
                this.DataReader.Close();
        }

        private static void AttachParameters(IDbCommand command, List<IDbDataParameter> commandParameters)
        {
            foreach (IDbDataParameter idbParameter in commandParameters)
            {
                if ((idbParameter.Direction == ParameterDirection.InputOutput) && (idbParameter.Value == null))
                {
                    idbParameter.Value = DBNull.Value;
                }

                command.Parameters.Add(idbParameter);
            }
        }

        
        private static void PrepareCommand(IDbCommand command, IDbConnection connection, IDbTransaction transaction, CommandType commandType, string commandText, List<IDbDataParameter> commandParameters)
        {
            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = commandType;

            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            if (commandParameters != null)
            {
                DBManager.AttachParameters(command, commandParameters);
            }
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(CommandType commandType, string commandText)
        {
            int returnValue;
            using (TraceBlock tb = new TraceBlock(DBLogger,DreamConstants.DBEXECUTIONEXECUTENONQUERY, commandText))
            {
                using (this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType))
                {
                    DBManager.PrepareCommand(idbCommand, this.Connection, this.Transaction, commandType, commandText, this.Parameters);
                    returnValue = idbCommand.ExecuteNonQuery();
                    idbCommand.Parameters.Clear();
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns></returns>
        public object ExecuteScalar(CommandType commandType, string commandText)
        {
            object returnValue = null;
            using (TraceBlock tb = new TraceBlock(DBLogger,DreamConstants.DBEXECUTIONEXECUTESCALAR, commandText))
            {
                using (this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType))
                {
                    DBManager.PrepareCommand(idbCommand, this.Connection, this.Transaction, commandType, commandText, this.Parameters);
                    returnValue = idbCommand.ExecuteScalar();
                    idbCommand.Parameters.Clear();
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Executes the command to create a data set.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns></returns>

        public DataSet ExecuteDataSet(CommandType commandType, string commandText)
        {
            if (this.Connection == null)
                this.Open();
            while (this.Connection.State == ConnectionState.Connecting) ;
            DataSet dataSet = new DataSet();

            using (TraceBlock tb = new TraceBlock(DBLogger, DreamConstants.DBEXECUTIONEXECUTEDATASET, commandText))
            {
                using (this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType))
                {
                    this.idbCommand.CommandTimeout = 180;
                    DBManager.PrepareCommand(idbCommand, this.Connection, this.Transaction, commandType, commandText, this.Parameters);
                    IDbDataAdapter dataAdapter = DBManagerFactory.GetDataAdapter(this.ProviderType);
                    try
                    {
                        dataAdapter.SelectCommand = idbCommand;
                        dataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                        dataAdapter.MissingMappingAction = MissingMappingAction.Passthrough;

                        dataAdapter.Fill(dataSet);
                    }
                    finally
                    {
                        DBManagerFactory.DisposeDataAdapter(dataAdapter);
                    }
                    idbCommand.Parameters.Clear();
                }
            }
            return dataSet;
        }

        /// <summary>
        /// Executes the command to create a data set.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns></returns>
        
        public DataSet ExecuteDataSetWithoutConstraints(CommandType commandType, string commandText)
        {
            if (this.Connection == null)
                this.Open();
            while (this.Connection.State == ConnectionState.Connecting) ;
            DataSet dataSet = new DataSet();
            dataSet.EnforceConstraints = false;

            using (TraceBlock tb = new TraceBlock(DBLogger, DreamConstants.DBEXECUTIONEXECUTEDATASET, commandText))
            {
                using (this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType))
                {

                    DBManager.PrepareCommand(idbCommand, this.Connection, this.Transaction, commandType, commandText, this.Parameters);
                    IDbDataAdapter dataAdapter = DBManagerFactory.GetDataAdapter(this.ProviderType);
                    try
                    {
                        dataAdapter.SelectCommand = idbCommand;
                        dataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                        dataAdapter.MissingMappingAction = MissingMappingAction.Passthrough;

                        dataAdapter.Fill(dataSet);
                    }
                    finally
                    {
                        DBManagerFactory.DisposeDataAdapter(dataAdapter);
                    }
                    idbCommand.Parameters.Clear();
                }
            }
            return dataSet;
        }
    }
}
