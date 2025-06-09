using System.Data;
using System.Data.Common;

namespace SqlSugar.TDengineAdo;

public class TDengineParameter : DbParameter
{
    public string parameterName;
    private int size;
    private System.Data.DbType dbType;

    public object value { get; set; }

    public bool IsMicrosecond { get; set; }

    public bool IsNanosecond { get; set; }

    public TDengineParameter(string parameterName, object value, System.Data.DbType dbType = System.Data.DbType.Object, int size = 0)
    {
        this.parameterName = parameterName;
        this.value = value;
        this.size = size;
        this.dbType = dbType;
    }

    public override System.Data.DbType DbType
    {
        get => this.dbType;
        set => this.dbType = value;
    }

    public override int Size
    {
        get => this.size;
        set => this.size = value;
    }

    public override string ParameterName
    {
        get => this.parameterName;
        set => this.parameterName = value;
    }

    public override object Value
    {
        get => this.value;
        set => this.value = value;
    }

    public override void ResetDbType() => throw new NotImplementedException();

    public override string SourceColumn
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override bool IsNullable
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override DataRowVersion SourceVersion
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override ParameterDirection Direction
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override bool SourceColumnNullMapping
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
}
