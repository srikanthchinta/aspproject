using System;using Shell.WRFM.Global.Utilities;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data.OracleClient;

namespace Shell.WRFM.Global.Web.DataAccess
{
    /// <summary>
    /// DBManagerFactory
    /// </summary>
    public sealed class DBManagerFactory
    {
        private DBManagerFactory() { }

        /// <summary>
        /// This method returns the connection object for the specified DataProvider
        /// </summary>
        /// <param name="providerType">enum value for DataProvidere</param>
        /// <returns>IDbConnection</returns>
        public static IDbConnection GetConnection(DataProvider providerType)
        {
            IDbConnection iDbConnection = null;
            switch (providerType)
            {
                case DataProvider.SqlServer:
                    iDbConnection = new SqlConnection();
                    break;
                case DataProvider.OleDb:
                    iDbConnection = new OleDbConnection();
                    break;
                case DataProvider.Odbc:
                    iDbConnection = new OdbcConnection();
                    break;
                case DataProvider.Oracle:
                    iDbConnection = new OracleConnection();
                    break;
                default:
                    return null;
            }

            return iDbConnection;
        }

        /// <summary>
        /// Returns the command object for the specified dataProvider
        /// </summary>
        /// <param name="providerType">enum value for DataProvider</param>
        /// <returns>DbCommand</returns>
        public static IDbCommand GetCommand(DataProvider providerType)
        {
            switch (providerType)
            {
                case DataProvider.SqlServer:
                    return new SqlCommand();
                case DataProvider.OleDb:
                    return new OleDbCommand();
                case DataProvider.Odbc:
                    return new OdbcCommand();
                case DataProvider.Oracle:
                    return new OracleCommand();
                default:
                    return null;
            }
        }

        /// <summary>
        ///  Returns the DataAdapter object for the specified dataProvider .
        /// </summary>
        /// <param name="providerType">enum value for DataProvider</param>
        /// <returns>DbDataAdapter</returns>
        
        public static IDbDataAdapter GetDataAdapter(DataProvider providerType)
        {
            switch (providerType)
            {
                case DataProvider.SqlServer:
                    return new SqlDataAdapter();
                case DataProvider.OleDb:
                    return new OleDbDataAdapter();
                case DataProvider.Odbc:
                    return new OdbcDataAdapter();
                case DataProvider.Oracle:
                    return new OracleDataAdapter();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Disposes the data adapter.
        /// </summary>
        /// <param name="adapter">The adapter.</param>
        public static void DisposeDataAdapter(IDbDataAdapter adapter)
        {
            IDisposable disposer = adapter as IDisposable;

            if (disposer != null)
            {
                disposer.Dispose();
            }
        }

        /// <summary>
        /// Returns the Transaction object for the specified dataProvider.
        /// </summary>
        /// <param name="providerType">enum value for DataProvider</param>
        /// <returns>DbTransaction object</returns>
        public static IDbTransaction GetTransaction(DataProvider providerType)
        {
            IDbConnection iDbConnection = GetConnection(providerType);
            IDbTransaction iDbTransaction = iDbConnection.BeginTransaction();
            return iDbTransaction;
        }

        /// <summary>
        /// Returns the Data Parameter object for the specified dataProvider.
        /// </summary>
        /// <param name="providerType">enum value for DataProvider</param>
        /// <returns>IDataParameter</returns>
        public static IDataParameter GetParameter(DataProvider providerType)
        {
            IDataParameter iDataParameter = null;
            switch (providerType)
            {
                case DataProvider.SqlServer:
                    iDataParameter = new SqlParameter();
                    break;
                case DataProvider.OleDb:
                    iDataParameter = new OleDbParameter();
                    break;
                case DataProvider.Odbc:
                    iDataParameter = new OdbcParameter();
                    break;

            }
            return iDataParameter;
        }

        /// <summary>
        /// Returns the array of DataParameters of size paramsCount. 
        /// </summary>
        /// <param name="providerType">enum value for DataProvider</param>
        /// <param name="paramsCount">Number of parameters</param>
        /// <returns>Array of IDbDataParameter</returns>
        public static IDbDataParameter[] GetParameters(DataProvider providerType, int paramsCount)
        {
            IDbDataParameter[] idbParams = new IDbDataParameter[paramsCount];

            switch (providerType)
            {
                case DataProvider.SqlServer:
                    for (int i = 0; i < paramsCount; ++i)
                    {
                        idbParams[i] = new SqlParameter();
                    }
                    break;
                case DataProvider.OleDb:
                    for (int i = 0; i < paramsCount; ++i)
                    {
                        idbParams[i] = new OleDbParameter();
                    }
                    break;
                case DataProvider.Odbc:
                    for (int i = 0; i < paramsCount; ++i)
                    {
                        idbParams[i] = new OdbcParameter();
                    }
                    break;
                default:
                    idbParams = null;
                    break;
            }
            return idbParams;
        }
    }
}
