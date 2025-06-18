




namespace ThingsGateway.Upgrade;

public interface IUpdateZipFileHostedService
{
    void Delete(IEnumerable<UpdateZipFile> updateZipFiles);
    List<UpdateZipFile>? GetList(UpdateZipFileInput input);
    Task<QueryData<UpdateZipFile>>? Page(QueryPageOptions options);
    Task SaveUpdateZipFile(UpdateZipFileAddInput input);
    Task SaveUpdateZipFile(UpdateZipFileAddInput1 input);
}