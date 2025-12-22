
using BootstrapBlazor.Components;

namespace ThingsGateway.Management.Application;

public interface IUpdateZipFileService
{
    void Delete(IEnumerable<UpdateZipFile> updateZipFiles);
    List<UpdateZipFile>? GetList(UpdateZipFileInput input);
    Task<QueryData<UpdateZipFile>>? Page(QueryPageOptions options);
    Task SaveUpdateZipFile(UpdateZipFileAddInput input);
    Task SaveUpdateZipFile(UpdateZipFileAddInput1 input);
}