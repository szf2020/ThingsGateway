// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components.Forms;

#if !Management
namespace ThingsGateway.Gateway.Application;
#else
namespace ThingsGateway.Management.Application;
#endif

public interface IVariablePageService
{
    Task<bool> BatchEditVariableAsync(List<Variable> models, Variable oldModel, Variable model, bool restart);
    Task<bool> DeleteVariableAsync(List<long> ids, bool restart);
    Task<bool> ClearVariableAsync(bool restart);
    Task InsertTestDataAsync(int testVariableCount, int testDeviceCount, string slaveUrl, bool businessEnable, bool restart);

    Task<bool> BatchSaveVariableAsync(List<Variable> input, ItemChangedType type, bool restart);

    Task<bool> SaveVariableAsync(Variable input, ItemChangedType type, bool restart);
    Task CopyVariableAsync(List<Variable> Model, int CopyCount, string CopyVariableNamePrefix, int CopyVariableNameSuffixNumber, bool AutoRestartThread);
    Task<QueryData<VariableRuntime>> OnVariableQueryAsync(QueryPageOptions options);
    Task<List<Variable>> GetVariableListAsync(QueryPageOptions option, int v);
    Task<USheetDatas> ExportVariableAsync(List<Variable> models, string? sortName, SortOrder sortOrder);
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableUSheetDatasAsync(USheetDatas data, bool restart);

    Task<string> ExportVariableFileAsync(GatewayExportFilter exportFilter);

    Task<OperResult<object>> OnWriteVariableAsync(long id, string writeData);
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableAsync(IBrowserFile a, bool restart);
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableFileAsync(string filePath, bool restart);
    Task InsertTestDtuDataAsync(int deviceCount, string slaveUrl, bool restart);
}