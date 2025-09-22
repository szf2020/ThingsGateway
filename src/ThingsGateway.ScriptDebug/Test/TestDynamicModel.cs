////------------------------------------------------------------------------------
////此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//// 此代码版权（除特别声明外的代码）归作者本人Diego所有
//// 源代码使用协议遵循本仓库的开源协议及附加协议
//// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//// 使用文档：https://thingsgateway.cn/
//// QQ群：605534569
//// ------------------------------------------------------------------------------

using System.Dynamic;

using ThingsGateway.Foundation;
using ThingsGateway.Gateway.Application;
public class TestDynamicModel : IDynamicModel
{
    public IEnumerable<dynamic> GetList(IEnumerable<object> datas)
    {
        if (datas == null) return null;
        List<ExpandoObject> deviceObjs = new List<ExpandoObject>();
        //按设备名称分组
        var groups = datas.Where(a => !string.IsNullOrEmpty(((AlarmVariable)a).DeviceName)).GroupBy(a => ((AlarmVariable)a).DeviceName, a => ((AlarmVariable)a));
        foreach (var group in groups)
        {
            //按采集时间分组
            var data = group.GroupBy(a => a.AlarmTime.DateTimeToUnixTimestamp());
            var deviceObj = new ExpandoObject();
            List<ExpandoObject> expandos = new List<ExpandoObject>();
            foreach (var item in data)
            {
                var expando = new ExpandoObject();
                expando.TryAdd("ts", item.Key);
                var variableObj = new ExpandoObject();
                foreach (var tag in item)
                {
                    var alarmObj = new ExpandoObject();
                    alarmObj.TryAdd(nameof(tag.AlarmCode), tag.AlarmCode);
                    alarmObj.TryAdd(nameof(tag.AlarmText), tag.AlarmText);
                    alarmObj.TryAdd(nameof(tag.AlarmType), tag.AlarmType);
                    alarmObj.TryAdd(nameof(tag.AlarmLimit), tag.AlarmLimit);
                    alarmObj.TryAdd(nameof(tag.EventTime), tag.EventTime);
                    alarmObj.TryAdd(nameof(tag.EventType), tag.EventType);

                    variableObj.TryAdd(tag.Name, alarmObj);
                }
                expando.TryAdd("alarms", variableObj);

                expandos.Add(expando);
            }
            deviceObj.TryAdd(group.Key, expandos);
            deviceObjs.Add(deviceObj);
        }

        return deviceObjs;
    }
}