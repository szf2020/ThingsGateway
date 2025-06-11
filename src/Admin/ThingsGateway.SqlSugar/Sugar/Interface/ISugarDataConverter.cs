using System.Data;

namespace ThingsGateway.SqlSugar
{
    public interface ISugarDataConverter
    {
        SugarParameter ParameterConverter<T>(object columnValue, int columnIndex);

        T QueryConverter<T>(IDataRecord dataRecord, int dataRecordIndex);
    }
}
