using System;
using System.Data;
using MySql.Data.MySqlClient;

using IHI.Server.Useful;

namespace IHI.Server.Database
{
    public class WrappedMySqlConnection : IDisposable
    {
        #region Field: _connection
        /// <summary>
        /// 
        /// </summary>
        private readonly MySqlConnection _connection;
        #endregion

        public WrappedMySqlConnection(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
        }

        #region Method: GetCachedStatement
        public WrappedMySqlCommand GetCommand(string queryString)
        {
            return new WrappedMySqlCommand(queryString, _connection);
        }
        #endregion

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
