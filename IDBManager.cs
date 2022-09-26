using System.Collections.Generic;
using System.Data;

namespace Shell.WRFM.Global.Web.DataAccess
{
    /// <summary>
    /// IDBManager
    /// </summary>
    public interface IDBManager
    {
        /// <summary>
        /// Gets or sets the type of the provider.
        /// </summary>
        /// <value>
        /// The type of the provider.
        /// </value>
        DataProvider ProviderType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        string ConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        IDbConnection Connection
        {
            get;
        }
        /// <summary>
        /// Gets the transaction.
        /// </summary>
        IDbTransaction Transaction
        {
            get;
        }

        /// <summary>
        /// Gets the data reader.
        /// </summary>
        IDataReader DataReader
        {
            get;
        }
        /// <summary>
        /// Gets the data adapter.
        /// </summary>
        IDataAdapter DataAdapter
        {
            get;
        }
        /// <summary>
        /// Gets the command.
        /// </summary>
        IDbCommand Command
        {
            get;
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        List<IDbDataParameter> Parameters
        {
            get;
        }
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        IDBManager Clone();

        /// <summary>
        /// Opens this instance.
        /// </summary>
        void Open();
        /// <summary>
        /// Begins the transaction.
        /// </summary>
        void BeginTransaction();
        /// <summary>
        /// Commits the transaction.
        /// </summary>
        void CommitTransaction();
        /// <summary>
        /// Adds the parameters.
        /// </summary>
        /// <param name="paramName">Name of the param.</param>
        /// <param name="objValue">The obj value.</param>
        void AddParameters(string paramName, object objValue);
        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns></returns>
        IDataReader ExecuteReader(CommandType commandType, string commandText);
        /// <summary>
        /// Executes the data set.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns></returns>
        DataSet ExecuteDataSet(CommandType commandType, string commandText);
        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns></returns>
        object ExecuteScalar(CommandType commandType, string commandText);
        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns></returns>
        int ExecuteNonQuery(CommandType commandType, string commandText);
        /// <summary>
        /// Closes the reader.
        /// </summary>
        void CloseReader();
        /// <summary>
        /// Closes this instance.
        /// </summary>
        void Close();
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        void Dispose();
    }
}
