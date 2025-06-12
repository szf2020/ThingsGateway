// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using ThingsGateway.Extension.Generic;
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Gateway.Razor
{
    public partial class VariableWidget
    {
        [Inject]
        IStringLocalizer<ThingsGateway.Gateway.Razor._Imports> Localizer { get; set; }

        [Parameter]
        public VariableNode Node { get; set; }

        private static async Task<QueryData<SelectedItem>> OnRedundantDevicesQuery(VirtualizeQueryOption option)
        {
            var ret = new QueryData<SelectedItem>()
            {
                IsSorted = false,
                IsFiltered = false,
                IsAdvanceSearch = false,
                IsSearch = !option.SearchText.IsNullOrWhiteSpace()
            };

            var devices = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
            var items = devices.Where(a => a.IsCollect == true).WhereIf(!option.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(option.SearchText)).Take(20)
               .Select(a => new SelectedItem(a.Name, a.Name)).ToList();

            ret.TotalCount = items.Count;
            ret.Items = items;
            return ret;
        }

        private Task OnSelectedItemChanged(SelectedItem item)
        {
            return InvokeAsync(StateHasChanged);
        }
        private async Task<QueryData<SelectedItem>> OnRedundantVariablesQuery(VirtualizeQueryOption option)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            var ret = new QueryData<SelectedItem>()
            {
                IsSorted = false,
                IsFiltered = false,
                IsAdvanceSearch = false,
                IsSearch = !option.SearchText.IsNullOrWhiteSpace()
            };

            if ((!Node.DeviceText.IsNullOrWhiteSpace()) && GlobalData.ReadOnlyDevices.TryGetValue(Node.DeviceText, out var device))
            {
                var items = device.ReadOnlyVariableRuntimes.WhereIf(!option.SearchText.IsNullOrWhiteSpace(), a => a.Value.Name.Contains(option.SearchText)).Select(a => a.Value).Take(20)
                   .Select(a => new SelectedItem(a.Name, a.Name)).ToList();

                ret.TotalCount = items.Count;
                ret.Items = items;
                return ret;
            }
            else
            {
                ret.TotalCount = 0;
                ret.Items = new List<SelectedItem>();
                return ret;
            }
        }


    }
}