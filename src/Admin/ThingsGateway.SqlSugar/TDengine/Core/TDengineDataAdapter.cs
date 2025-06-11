using System.Data;
using System.Data.Common;

namespace ThingsGateway.SqlSugar.TDengineAdo;

public class TDengineDataAdapter : IDataAdapter
{
    private TDengineCommand command;
    private string sql;
    private TDengineConnection _TDengineConnection;

    public TDengineDataAdapter(TDengineCommand command) => this.command = command;

    public TDengineDataAdapter()
    {
    }

    public TDengineDataAdapter(string sql, TDengineConnection _TDengineConnection)
    {
        this.sql = sql;
        this._TDengineConnection = _TDengineConnection;
    }

    public TDengineCommand SelectCommand
    {
        get
        {
            if (this.command == null)
                this.command = new TDengineCommand(this.sql, this._TDengineConnection);
            return this.command;
        }
        set => this.command = value;
    }





    public ITableMappingCollection TableMappings => throw new NotImplementedException();

    public void Fill(DataTable dt)
    {
        if (dt == null)
            dt = new DataTable();
        DataColumnCollection columns = dt.Columns;
        DataRowCollection rows = dt.Rows;
        using (DbDataReader dbDataReader = this.command.ExecuteReader())
        {
            for (int ordinal = 0; ordinal < dbDataReader.FieldCount; ++ordinal)
            {
                string str = dbDataReader.GetName(ordinal).Trim();
                if (!columns.Contains(str))
                    columns.Add(new DataColumn(str, dbDataReader.GetFieldType(ordinal)));
                else
                    columns.Add(new DataColumn(str + ordinal.ToString(), dbDataReader.GetFieldType(ordinal)));
            }
            while (dbDataReader.Read())
            {
                DataRow row = dt.NewRow();
                for (int index = 0; index < columns.Count; ++index)
                    row[columns[index].ColumnName] = dbDataReader.GetValue(index) ?? (object)DBNull.Value;
                dt.Rows.Add(row);
            }
        }
        dt.AcceptChanges();
    }

    public void Fill(DataSet ds)
    {
        if (ds == null)
            ds = new DataSet();
        using (DbDataReader dbDataReader = this.command.ExecuteReader())
        {
            do
            {
                DataTable table = new DataTable();
                DataColumnCollection columns = table.Columns;
                DataRowCollection rows = table.Rows;
                for (int ordinal = 0; ordinal < dbDataReader.FieldCount; ++ordinal)
                {
                    string str = dbDataReader.GetName(ordinal).Trim();
                    if (!columns.Contains(str))
                        columns.Add(new DataColumn(str, dbDataReader.GetFieldType(ordinal)));
                    else
                        columns.Add(new DataColumn(str + ordinal.ToString(), dbDataReader.GetFieldType(ordinal)));
                }
                while (dbDataReader.Read())
                {
                    DataRow row = table.NewRow();
                    for (int index = 0; index < columns.Count; ++index)
                        row[columns[index].ColumnName] = dbDataReader.GetValue(index);
                    table.Rows.Add(row);
                }
                table.AcceptChanges();
                ds.Tables.Add(table);
            }
            while (dbDataReader.NextResult());
        }
    }


}
