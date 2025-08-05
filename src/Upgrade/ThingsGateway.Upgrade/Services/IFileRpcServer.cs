using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
namespace ThingsGateway.Upgrade;

[GeneratorRpcProxy(GeneratorFlag = GeneratorFlag.ExtensionAsync)]
public interface IFileRpcServer : IRpcServer
{
    [DmtpRpc]
    List<UpdateZipFile> GetList(UpdateZipFileInput input);
}