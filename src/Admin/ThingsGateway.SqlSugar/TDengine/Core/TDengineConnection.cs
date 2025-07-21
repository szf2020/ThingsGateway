using System.Data;
using System.Data.Common;

using TDengine.Driver;
using TDengine.Driver.Client;
using TDengine.Driver.Client.Websocket;
namespace TDengineAdo
{
    public class TDengineConnection : DbConnection
    {
        internal ITDengineClient connection;

        private ConnectionStringBuilder connectionStringBuilder;
        public TDengineConnection(string connectionString)
        {
            connectionStringBuilder = new ConnectionStringBuilder(connectionString);
            this.connection = DbDriver.Open(connectionStringBuilder);
        }

        public override string ConnectionString
        {
            get
            {
                return connectionStringBuilder.ConnectionString;
            }
            set => throw new NotSupportedException();
        }

        public override string Database => connectionStringBuilder.Database;

        public override string DataSource => connectionStringBuilder.Host;

        public override string ServerVersion => "Unknown";

        public override ConnectionState State
        {
            get => this.connection != null ? ConnectionState.Open : ConnectionState.Closed;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotSupportedException(nameof(BeginDbTransaction));
        }

        public override void Close()
        {
            if (connection != null && connection is not WSClient)
            {
                connection.Dispose();
                connection = null;
            }
        }
        public override void Open()
        {
            if (this.connection == null)
                this.connection = DbDriver.Open(connectionStringBuilder);
            else if (this.connection == null)
                this.connection = DbDriver.Open(connectionStringBuilder);
            if (string.IsNullOrEmpty(this.Database))
                return;
            this.connection.Exec("use " + this.Database);
        }

        protected override DbCommand CreateDbCommand()
        {
            return new TDengineCommand();
        }
        protected DbCommand CreateDbCommand(string commandText)
        {
            return new TDengineCommand(commandText, this);
        }

        protected override void Dispose(bool disposing)
        {
            connection?.Dispose();
        }

        public override void ChangeDatabase(string databaseName) => connectionStringBuilder.Database = databaseName;
    }
}