using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

using TDengine.Data.Client;
using TDengine.Driver;

namespace SqlSugar.TDengineAdo;

public class TDengineCommand : DbCommand
{
    private string commandText;
    private TDengineConnection connection;
    private TDengineParameterCollection _DbParameterCollection;

    public TDengineCommand()
    {
    }

    public TDengineCommand(string commandText, TDengineConnection connection)
    {
        this.CommandText = commandText;
        this.Connection = connection;
    }

    public override string CommandText
    {
        get => this.commandText;
        set => this.commandText = value;
    }

    public override int CommandTimeout { get; set; }

    public override CommandType CommandType { get; set; }

    public override bool DesignTimeVisible { get; set; }

    public override UpdateRowSource UpdatedRowSource { get; set; }

    protected override DbConnection DbConnection
    {
        get => (DbConnection)this.connection;
        set => this.connection = (TDengineConnection)value;
    }

    protected override DbParameterCollection DbParameterCollection
    {
        get
        {
            if (this._DbParameterCollection == null)
                this._DbParameterCollection = new TDengineParameterCollection();
            return (DbParameterCollection)this._DbParameterCollection;
        }
    }

    protected override DbTransaction DbTransaction { get; set; }

    public override void Cancel() => throw new NotImplementedException();

    public override int ExecuteNonQuery()
    {
        try
        {
            this.connection.Open();
            long num = this.connection.connection.Exec(this.GetNoParameterSql(this.commandText));
            this.connection.Close();
            return num > (long)int.MaxValue ? int.MaxValue : Convert.ToInt32(num);
        }
        catch
        {
            this.connection.Close();
            throw;
        }
    }

    public override object ExecuteScalar()
    {
        try
        {
            this.connection.Open();
            IRows irows = this.connection.connection.Query(this.GetNoParameterSql(this.commandText));
            using (irows)
            {
                irows.Read();
                this.connection.Close();
                return irows.GetValue(0);
            }
        }
        catch
        {
            this.connection.Close();
            throw;
        }
    }

    public new DbDataReader ExecuteReader()
    {
        try
        {
            this.connection.Open();
            TDengineDataReader tdengineDataReader = new TDengineDataReader(this.connection.connection.Query(this.GetNoParameterSql(this.commandText)));
            this.connection.Close();
            return (DbDataReader)tdengineDataReader;
        }
        catch
        {
            this.connection.Close();
            throw;
        }
    }

    public override void Prepare() => throw new NotImplementedException();

    protected override void Dispose(bool disposing) => base.Dispose(disposing);

    protected override DbParameter CreateDbParameter() => throw new NotImplementedException();

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        return this.ExecuteReader();
    }

    private string GetNoParameterSql(string sql)
    {
        foreach (TDengineParameter tdengineParameter in (IEnumerable<TDengineParameter>)this.Parameters.Cast<TDengineParameter>().OrderByDescending<TDengineParameter, int>((Func<TDengineParameter, int>)(it => it.parameterName.Length)))
        {
            if (!tdengineParameter.parameterName.Contains('@'))
                tdengineParameter.parameterName = "@" + tdengineParameter.parameterName;
            object obj = tdengineParameter.value;
            if (tdengineParameter.value == null || tdengineParameter.value == DBNull.Value)
                sql = Regex.Replace(sql, tdengineParameter.parameterName, "null", RegexOptions.IgnoreCase);
            else if (tdengineParameter.value is DateTime)
            {
                DateTime dateTime = (DateTime)tdengineParameter.value;
                sql = !tdengineParameter.IsMicrosecond ? (!tdengineParameter.IsNanosecond ? Regex.Replace(sql, tdengineParameter.parameterName, Helper.ToUnixTimestamp(dateTime).ToString() ?? "", RegexOptions.IgnoreCase) : Regex.Replace(sql, tdengineParameter.parameterName, Helper.DateTimeToLong19(dateTime).ToString() ?? "", RegexOptions.IgnoreCase)) : Regex.Replace(sql, tdengineParameter.parameterName, Helper.DateTimeToLong16(dateTime).ToString() ?? "", RegexOptions.IgnoreCase);
            }
            else
                sql = tdengineParameter.value is string || tdengineParameter.value != null ? Regex.Replace(sql, tdengineParameter.parameterName, "'" + tdengineParameter.value.ToString().Replace("'", "''") + "'", RegexOptions.IgnoreCase) : Regex.Replace(sql, tdengineParameter.parameterName, "'" + tdengineParameter.value?.ToString() + "'", RegexOptions.IgnoreCase);
        }
        return sql;
    }
}
