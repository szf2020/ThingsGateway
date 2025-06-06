using MySqlConnector;

using System.Data;

namespace SqlSugar
{
    public partial class MySqlFastBuilder : FastBuilder, IFastBuilder
    {
        private async Task<int> MySqlConnectorBulkCopy(DataTable dt)
        {
            try
            {
                this.Context.Open();
                var tran = (MySqlTransaction)this.Context.Ado.Transaction;
                var connection = (MySqlConnection)this.Context.Ado.Connection;
                MySqlBulkCopy bulkCopy = new MySqlBulkCopy(connection, tran);
                bulkCopy.DestinationTableName = dt.TableName;
                await bulkCopy.WriteToServerAsync(dt).ConfigureAwait(false);
                return dt.Rows.Count;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                CloseDb();
            }
        }
    }
}