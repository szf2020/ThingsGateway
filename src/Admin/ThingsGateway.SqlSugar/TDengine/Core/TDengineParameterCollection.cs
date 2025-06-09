using System.Collections;
using System.Data.Common;

namespace SqlSugar.TDengineAdo;

public class TDengineParameterCollection : DbParameterCollection
{
    private List<TDengineParameter> parameters = new List<TDengineParameter>();

    public override int Count => this.parameters.Count;

    public override object SyncRoot => ((ICollection)this.parameters).SyncRoot;

    public override int Add(object value)
    {
        this.parameters.Add((TDengineParameter)value);
        return this.parameters.Count - 1;
    }

    public override void AddRange(Array values)
    {
        foreach (TDengineParameter tdengineParameter in values)
            this.parameters.Add(tdengineParameter);
    }

    public override void Clear() => this.parameters.Clear();

    public override bool Contains(string value)
    {
        foreach (DbParameter parameter in this.parameters)
        {
            if (parameter.ParameterName.Equals(value, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    public override bool Contains(object value)
    {
        return this.parameters.Contains((TDengineParameter)value);
    }

    public override void CopyTo(Array array, int index)
    {
        for (int index1 = 0; index1 < this.parameters.Count; ++index1)
            array.SetValue((object)this.parameters[index1], index + index1);
    }

    public override IEnumerator GetEnumerator() => (IEnumerator)this.parameters.GetEnumerator();

    public override int IndexOf(string parameterName)
    {
        for (int index = 0; index < this.parameters.Count; ++index)
        {
            if (this.parameters[index].ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
                return index;
        }
        return -1;
    }

    public override int IndexOf(object value) => this.parameters.IndexOf((TDengineParameter)value);

    public override void Insert(int index, object value)
    {
        this.parameters.Insert(index, (TDengineParameter)value);
    }

    public override void Remove(object value) => this.parameters.Remove((TDengineParameter)value);

    public override void RemoveAt(int index) => this.parameters.RemoveAt(index);

    public override void RemoveAt(string parameterName)
    {
        int index = this.IndexOf(parameterName);
        if (index < 0)
            return;
        this.parameters.RemoveAt(index);
    }

    protected override DbParameter GetParameter(int index) => (DbParameter)this.parameters[index];

    protected override DbParameter GetParameter(string parameterName)
    {
        int index = this.IndexOf(parameterName);
        return index >= 0 ? (DbParameter)this.parameters[index] : (DbParameter)null;
    }

    protected override void SetParameter(int index, DbParameter value)
    {
        if (index < 0 || index >= this.parameters.Count)
            throw new IndexOutOfRangeException("Index is out of range.");
        this.parameters[index] = value != null ? (TDengineParameter)value : throw new ArgumentNullException(nameof(value), "Parameter cannot be null.");
    }

    protected override void SetParameter(string parameterName, DbParameter value)
    {
        int index = this.IndexOf(parameterName);
        if (index == -1)
            throw new IndexOutOfRangeException("Parameter not found.");
        this.parameters[index] = value != null ? (TDengineParameter)value : throw new ArgumentNullException(nameof(value), "Parameter cannot be null.");
    }
}
