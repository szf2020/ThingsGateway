// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait
using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

using ThingsGateway.Common;

namespace ThingsGateway.Plugin.Mqtt
{
    public partial class MqttPropertyRazor : IPropertyUIBase
    {
        [Parameter, EditorRequired]
        public string Id { get; set; }
        [Parameter, EditorRequired]
        public bool CanWrite { get; set; }
        [Parameter, EditorRequired]
        public ModelValueValidateForm Model { get; set; }

        [Parameter, EditorRequired]
        public IEnumerable<IEditorItem> PluginPropertyEditorItems { get; set; }

        IStringLocalizer Localizer { get; set; }

        protected override Task OnParametersSetAsync()
        {
            Localizer = App.CreateLocalizerByType(Model.Value.GetType());

            return base.OnParametersSetAsync();
        }
        private async Task OnCAFileChange(UploadFile file)
        {
            var mqttClientProperty = (MqttClientProperty)Model.Value;

            if (mqttClientProperty.TLS == true)
            {
                {
                    var filePath = Path.Combine("PluginFile", Id, nameof(mqttClientProperty.CAFile));
                    if (!Directory.Exists(filePath))//如果不存在就创建文件夹
                        Directory.CreateDirectory(filePath);
                    //var fileSuffix = Path.GetExtension(file.Name).ToLower();// 文件后缀
                    var fileObjectName = $"{file.File.Name}";//存储后的文件名
                    var fileName = Path.Combine(filePath, fileObjectName);//获取文件全路径
                    fileName = fileName.Replace("\\", "/");//格式化一系

                    using (var stream = File.Create(Path.Combine(filePath, fileObjectName)))
                    {
                        using var fs = file.File.OpenReadStream(1024 * 1024 * 500);
                        await fs.CopyToAsync(stream).ConfigureAwait(false);
                    }

                    mqttClientProperty.CAFile = fileName;
                }
            }
        }
        private async Task OnClientCertificateFileChange(UploadFile file)
        {
            var mqttClientProperty = (MqttClientProperty)Model.Value;

            if (mqttClientProperty.TLS == true)
            {
                {
                    var filePath = Path.Combine("PluginFile", Id, nameof(mqttClientProperty.ClientCertificateFile));
                    if (!Directory.Exists(filePath))//如果不存在就创建文件夹
                        Directory.CreateDirectory(filePath);
                    //var fileSuffix = Path.GetExtension(file.Name).ToLower();// 文件后缀
                    var fileObjectName = $"{file.File.Name}";//存储后的文件名
                    var fileName = Path.Combine(filePath, fileObjectName);//获取文件全路径
                    fileName = fileName.Replace("\\", "/");//格式化一系

                    using (var stream = File.Create(Path.Combine(filePath, fileObjectName)))
                    {
                        using var fs = file.File.OpenReadStream(1024 * 1024 * 500);
                        await fs.CopyToAsync(stream).ConfigureAwait(false);
                    }

                    mqttClientProperty.ClientCertificateFile = fileName;
                }

            }
        }
        private async Task OnClientKeyFileChange(UploadFile file)
        {
            var mqttClientProperty = (MqttClientProperty)Model.Value;

            if (mqttClientProperty.TLS == true)
            {

                {
                    var filePath = Path.Combine("PluginFile", Id, nameof(mqttClientProperty.ClientKeyFile));
                    if (!Directory.Exists(filePath))//如果不存在就创建文件夹
                        Directory.CreateDirectory(filePath);
                    //var fileSuffix = Path.GetExtension(file.Name).ToLower();// 文件后缀
                    var fileObjectName = $"{file.File.Name}";//存储后的文件名
                    var fileName = Path.Combine(filePath, fileObjectName);//获取文件全路径
                    fileName = fileName.Replace("\\", "/");//格式化一系

                    using (var stream = File.Create(Path.Combine(filePath, fileObjectName)))
                    {
                        using var fs = file.File.OpenReadStream(1024 * 1024 * 500);
                        await fs.CopyToAsync(stream).ConfigureAwait(false);
                    }

                    mqttClientProperty.ClientKeyFile = fileName;
                }
            }
        }
        [Inject]
        private DownloadService DownloadService { get; set; }
        [Inject]
        private ToastService ToastService { get; set; }

        [Inject]
        private DialogService DialogService { get; set; }
    }
}