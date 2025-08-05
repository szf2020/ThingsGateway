using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace ThingsGateway.Upgrade;

public partial class FileRpcServer : SingletonRpcServer, IFileRpcServer
{
    private readonly ILog _logger;

    public FileRpcServer(ILog logger)
    {
        _logger = logger;
    }

    [DmtpRpc]
    public List<UpdateZipFile> GetList(UpdateZipFileInput input)
    {
        return App.GetService<IUpdateZipFileHostedService>().GetList(input);
    }
}