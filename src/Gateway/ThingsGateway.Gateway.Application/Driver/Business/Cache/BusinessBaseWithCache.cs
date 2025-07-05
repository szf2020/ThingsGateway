//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;

using ThingsGateway.Extension;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件
/// </summary>
public abstract class BusinessBaseWithCache : BusinessBase
{

    #region 条件

    protected abstract bool AlarmModelEnable { get; }
    protected abstract bool DevModelEnable { get; }
    protected abstract bool VarModelEnable { get; }
    protected internal override Task InitChannelAsync(IChannel? channel, CancellationToken cancellationToken)
    {
        if (AlarmModelEnable)
            DBCacheAlarm = LocalDBCacheAlarmModel();

        if (DevModelEnable)
            DBCacheDev = LocalDBCacheDevModel();

        if (VarModelEnable)
        {
            DBCacheVar = LocalDBCacheVarModel();
            DBCacheVars = LocalDBCacheVarModels();
        }

        return base.InitChannelAsync(channel, cancellationToken);
    }
    protected override async Task ProtectedExecuteAsync(object? state, CancellationToken cancellationToken)
    {
        await Update(cancellationToken).ConfigureAwait(false);
    }
    protected virtual async Task Update(CancellationToken cancellationToken)
    {
        if (VarModelEnable)
        {
            await UpdateVarModelMemory(cancellationToken).ConfigureAwait(false);
            await UpdateVarModelsMemory(cancellationToken).ConfigureAwait(false);
        }

        if (DevModelEnable)
        {
            await UpdateDevModelMemory(cancellationToken).ConfigureAwait(false);
        }

        if (AlarmModelEnable)
        {
            await UpdateAlarmModelMemory(cancellationToken).ConfigureAwait(false);
        }

        if (VarModelEnable)
        {

            await UpdateVarModelCache(cancellationToken).ConfigureAwait(false);
            await UpdateVarModelsCache(cancellationToken).ConfigureAwait(false);
        }

        if (DevModelEnable)
        {
            await UpdateDevModelCache(cancellationToken).ConfigureAwait(false);
        }

        if (AlarmModelEnable)
        {
            await UpdateAlarmModelCache(cancellationToken).ConfigureAwait(false);
        }


    }
    #endregion


    #region alarm



    protected ConcurrentQueue<CacheDBItem<AlarmVariable>> _memoryAlarmModelQueue = new();

    private volatile bool LocalDBCacheAlarmModelInited;
    private CacheDB DBCacheAlarm;


    /// <summary>
    /// 入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddCache(List<CacheDBItem<AlarmVariable>> data)
    {

        if (_businessPropertyWithCache.CacheEnable && data?.Count > 0)
        {
            try
            {
                LogMessage?.LogInformation($"Add {typeof(DeviceBasicData).Name} data to file cache, count {data.Count}");
                foreach (var item in data)
                {
                    item.Id = CommonUtils.GetSingleId();
                }
                var dir = CacheDBUtil.GetCacheFilePath(CurrentDevice.Name.ToString());
                var fileStart = CacheDBUtil.GetFileName($"{CurrentDevice.PluginName}_{typeof(AlarmVariable).FullName}_{nameof(AlarmVariable)}");
                var fullName = dir.CombinePathWithOs($"{fileStart}{CacheDBUtil.EX}");

                lock (fullName)
                {
                    bool s = false;
                    while (!s)
                    {
                        s = CacheDBUtil.DeleteCache(_businessPropertyWithCache.CacheFileMaxLength, fullName);
                    }
                    using var cache = LocalDBCacheAlarmModel();
                    cache.DBProvider.Fastest<CacheDBItem<AlarmVariable>>().PageSize(50000).BulkCopy(data);
                }
            }
            catch
            {
                try
                {
                    using var cache = LocalDBCacheAlarmModel();
                    lock (cache.CacheDBOption.FileFullName)
                    {
                        cache.DBProvider.Fastest<CacheDBItem<AlarmVariable>>().PageSize(50000).BulkCopy(data);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex, "Add cache fail");
                }
            }
        }
    }

    /// <summary>
    /// 添加队列，超限后会入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddQueueAlarmModel(CacheDBItem<AlarmVariable> data)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            //检测队列长度，超限存入缓存数据库
            if (_memoryAlarmModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
            {
                List<CacheDBItem<AlarmVariable>> list = null;
                lock (_memoryAlarmModelQueue)
                {
                    if (_memoryAlarmModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                    {
                        list = _memoryAlarmModelQueue.ToListWithDequeue();
                    }
                }
                AddCache(list);
            }
        }
        if (_memoryAlarmModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
        {
            lock (_memoryAlarmModelQueue)
            {
                if (_memoryAlarmModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                {
                    LogMessage?.LogWarning($"{typeof(AlarmVariable).Name} Queue exceeds limit, clear old data. If it doesn't work as expected, increase [QueueMaxCount] or Enable cache");
                    _memoryAlarmModelQueue.Clear();
                    _memoryAlarmModelQueue.Enqueue(data);
                    return;
                }
            }
        }
        else
        {
            _memoryAlarmModelQueue.Enqueue(data);
        }
    }

    /// <summary>
    /// 获取缓存对象，注意每次获取的对象可能不一样，如顺序操作，需固定引用
    /// </summary>
    protected virtual CacheDB LocalDBCacheAlarmModel()
    {
        var cacheDb = CacheDBUtil.GetCache(typeof(CacheDBItem<AlarmVariable>), CurrentDevice.Name.ToString(), $"{CurrentDevice.PluginName}_{typeof(AlarmVariable).Name}");

        if (!LocalDBCacheAlarmModelInited)
        {
            cacheDb.InitDb();
            LocalDBCacheAlarmModelInited = true;
        }
        return cacheDb;
    }



    /// <summary>
    /// 需实现上传到通道
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract ValueTask<OperResult> UpdateAlarmModel(IEnumerable<CacheDBItem<AlarmVariable>> item, CancellationToken cancellationToken);

    protected async Task UpdateAlarmModelCache(CancellationToken cancellationToken)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            #region //成功上传时，补上传缓存数据

            if (IsConnected())
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        //循环获取，固定读最大行数量，执行完成需删除行
                        var varList = await DBCacheAlarm.DBProvider.Queryable<CacheDBItem<AlarmVariable>>().Take(_businessPropertyWithCache.SplitSize).ToListAsync(cancellationToken).ConfigureAwait(false);
                        if (varList.Count > 0)
                        {
                            try
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    var result = await UpdateAlarmModel(varList, cancellationToken).ConfigureAwait(false);
                                    if (result.IsSuccess)
                                    {
                                        //删除缓存
                                        await DBCacheAlarm.DBProvider.Deleteable<CacheDBItem<AlarmVariable>>(varList).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false);
                                    }
                                    else
                                        break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (success)
                                    LogMessage?.LogWarning(ex);
                                success = false;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (success)
                        LogMessage?.LogWarning(ex);
                    success = false;
                }
            }

            #endregion //成功上传时，补上传缓存数据
        }
    }

    protected async Task UpdateAlarmModelMemory(CancellationToken cancellationToken)
    {
        #region //上传设备内存队列中的数据

        try
        {
            var list = _memoryAlarmModelQueue.ToListWithDequeue().ChunkBetter(_businessPropertyWithCache.SplitSize);
            foreach (var item in list)
            {
                try
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        var result = await UpdateAlarmModel(item, cancellationToken).ConfigureAwait(false);
                        if (!result.IsSuccess)
                        {
                            AddCache(item.ToList());
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (success)
                        LogMessage?.LogWarning(ex);
                    success = false;
                }
            }
        }
        catch (Exception ex)
        {
            if (success)
                LogMessage?.LogWarning(ex);
            success = false;
        }

        #endregion //上传设备内存队列中的数据
    }

    #endregion



    #region device


    protected ConcurrentQueue<CacheDBItem<DeviceBasicData>> _memoryDevModelQueue = new();

    private volatile bool LocalDBCacheDevModelInited;

    private CacheDB DBCacheDev;


    /// <summary>
    /// 入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddCache(List<CacheDBItem<DeviceBasicData>> data)
    {

        if (_businessPropertyWithCache.CacheEnable && data?.Count > 0)
        {

            try
            {
                LogMessage?.LogInformation($"Add {typeof(DeviceBasicData).Name} data to file cache, count {data.Count}");
                foreach (var item in data)
                {
                    item.Id = CommonUtils.GetSingleId();
                }
                var dir = CacheDBUtil.GetCacheFilePath(CurrentDevice.Name.ToString());
                var fileStart = CacheDBUtil.GetFileName($"{CurrentDevice.PluginName}_{typeof(DeviceBasicData).FullName}_{nameof(DeviceBasicData)}");
                var fullName = dir.CombinePathWithOs($"{fileStart}{CacheDBUtil.EX}");

                lock (fullName)
                {
                    bool s = false;
                    while (!s)
                    {
                        s = CacheDBUtil.DeleteCache(_businessPropertyWithCache.CacheFileMaxLength, fullName);
                    }
                    using var cache = LocalDBCacheDevModel();
                    cache.DBProvider.Fastest<CacheDBItem<DeviceBasicData>>().PageSize(50000).BulkCopy(data);
                }
            }
            catch
            {
                try
                {
                    using var cache = LocalDBCacheDevModel();
                    lock (cache.CacheDBOption.FileFullName)
                    {
                        cache.DBProvider.Fastest<CacheDBItem<DeviceBasicData>>().PageSize(50000).BulkCopy(data);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex, "Add cache fail");
                }
            }
        }
    }

    /// <summary>
    /// 添加队列，超限后会入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddQueueDevModel(CacheDBItem<DeviceBasicData> data)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            //检测队列长度，超限存入缓存数据库
            if (_memoryDevModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
            {
                List<CacheDBItem<DeviceBasicData>> list = null;
                lock (_memoryDevModelQueue)
                {
                    if (_memoryDevModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                    {
                        list = _memoryDevModelQueue.ToListWithDequeue();
                    }
                }
                AddCache(list);
            }
        }
        if (_memoryDevModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
        {
            lock (_memoryDevModelQueue)
            {
                if (_memoryDevModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                {
                    LogMessage?.LogWarning($"{typeof(DeviceBasicData).Name} Queue exceeds limit, clear old data. If it doesn't work as expected, increase [QueueMaxCount] or Enable cache");
                    _memoryDevModelQueue.Clear();
                    _memoryDevModelQueue.Enqueue(data);
                    return;
                }
            }
        }
        else
        {
            _memoryDevModelQueue.Enqueue(data);
        }
    }

    /// <summary>
    /// 获取缓存对象，注意每次获取的对象可能不一样，如顺序操作，需固定引用
    /// </summary>
    protected virtual CacheDB LocalDBCacheDevModel()
    {
        var cacheDb = CacheDBUtil.GetCache(typeof(CacheDBItem<DeviceBasicData>), CurrentDevice.Name.ToString(), $"{CurrentDevice.PluginName}_{typeof(DeviceBasicData).Name}");
        if (!LocalDBCacheDevModelInited)
        {
            cacheDb.InitDb();
            LocalDBCacheDevModelInited = true;
        }
        return cacheDb;
    }



    /// <summary>
    /// 需实现上传到通道
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract ValueTask<OperResult> UpdateDevModel(IEnumerable<CacheDBItem<DeviceBasicData>> item, CancellationToken cancellationToken);

    protected async Task UpdateDevModelCache(CancellationToken cancellationToken)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            #region //成功上传时，补上传缓存数据

            if (IsConnected())
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {

                        //循环获取
                        var varList = await DBCacheDev.DBProvider.Queryable<CacheDBItem<DeviceBasicData>>().Take(_businessPropertyWithCache.SplitSize).ToListAsync(cancellationToken).ConfigureAwait(false);
                        if (varList.Count > 0)
                        {
                            try
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    var result = await UpdateDevModel(varList, cancellationToken).ConfigureAwait(false);
                                    if (result.IsSuccess)
                                    {
                                        //删除缓存
                                        await DBCacheDev.DBProvider.Deleteable<CacheDBItem<DeviceBasicData>>(varList).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false);
                                    }
                                    else
                                        break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (success)
                                    LogMessage?.LogWarning(ex);
                                success = false;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (success)
                        LogMessage?.LogWarning(ex);
                    success = false;
                }
            }

            #endregion //成功上传时，补上传缓存数据
        }
    }

    protected async Task UpdateDevModelMemory(CancellationToken cancellationToken)
    {
        #region //上传设备内存队列中的数据

        try
        {
            var list = _memoryDevModelQueue.ToListWithDequeue().ChunkBetter(_businessPropertyWithCache.SplitSize);
            foreach (var item in list)
            {
                try
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        var result = await UpdateDevModel(item, cancellationToken).ConfigureAwait(false);
                        if (!result.IsSuccess)
                        {
                            AddCache(item.ToList());
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (success)
                        LogMessage?.LogWarning(ex);
                    success = false;
                }
            }
        }
        catch (Exception ex)
        {
            if (success)
                LogMessage?.LogWarning(ex);
            success = false;
        }

        #endregion //上传设备内存队列中的数据
    }

    #endregion



    #region variable


    protected ConcurrentQueue<CacheDBItem<VariableBasicData>> _memoryVarModelQueue = new();
    protected ConcurrentQueue<CacheDBItem<List<VariableBasicData>>> _memoryVarModelsQueue = new();
    protected volatile bool success = true;
    private volatile bool LocalDBCacheVarModelInited;
    private volatile bool LocalDBCacheVarModelsInited;
    private CacheDB DBCacheVar;
    private CacheDB DBCacheVars;



    protected sealed override BusinessPropertyBase _businessPropertyBase => _businessPropertyWithCache;

    protected abstract BusinessPropertyWithCache _businessPropertyWithCache { get; }

    /// <summary>
    /// 入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddCache(List<CacheDBItem<VariableBasicData>> data)
    {
        if (_businessPropertyWithCache.CacheEnable && data?.Count > 0)
        {
            try
            {
                LogMessage?.LogInformation($"Add {typeof(VariableBasicData).Name} data to file cache, count {data.Count}");
                foreach (var item in data)
                {
                    item.Id = CommonUtils.GetSingleId();
                }
                var dir = CacheDBUtil.GetCacheFilePath(CurrentDevice.Name.ToString());
                var fileStart = CacheDBUtil.GetFileName($"{CurrentDevice.PluginName}_{typeof(VariableBasicData).Name}");
                var fullName = dir.CombinePathWithOs($"{fileStart}{CacheDBUtil.EX}");

                lock (this)
                {
                    bool s = false;
                    while (!s)
                    {
                        s = CacheDBUtil.DeleteCache(_businessPropertyWithCache.CacheFileMaxLength, fullName);
                    }
                    LocalDBCacheVarModelInited = false;
                    using var cache = LocalDBCacheVarModel();
                    cache.DBProvider.Fastest<CacheDBItem<VariableBasicData>>().PageSize(50000).BulkCopy(data);
                }
            }
            catch
            {
                try
                {
                    using var cache = LocalDBCacheVarModel();
                    lock (cache.CacheDBOption.FileFullName)
                        cache.DBProvider.Fastest<CacheDBItem<VariableBasicData>>().PageSize(50000).BulkCopy(data);
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex, "Add cache fail");
                }
            }
        }
    }
    /// <summary>
    /// 入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddCache(List<CacheDBItem<List<VariableBasicData>>> data)
    {
        if (_businessPropertyWithCache.CacheEnable && data?.Count > 0)
        {
            try
            {
                foreach (var item in data)
                {
                    item.Id = CommonUtils.GetSingleId();
                }
                var dir = CacheDBUtil.GetCacheFilePath(CurrentDevice.Name.ToString());
                var fileStart = CacheDBUtil.GetFileName($"{CurrentDevice.PluginName}_List_{typeof(VariableBasicData).Name}");
                var fullName = dir.CombinePathWithOs($"{fileStart}{CacheDBUtil.EX}");

                lock (this)
                {
                    bool s = false;
                    while (!s)
                    {
                        s = CacheDBUtil.DeleteCache(_businessPropertyWithCache.CacheFileMaxLength, fullName);
                    }
                    LocalDBCacheVarModelsInited = false;
                    using var cache = LocalDBCacheVarModels();
                    cache.DBProvider.Fastest<CacheDBItem<List<VariableBasicData>>>().PageSize(50000).BulkCopy(data);
                }
            }
            catch
            {
                try
                {
                    using var cache = LocalDBCacheVarModels();
                    lock (cache.CacheDBOption.FileFullName)
                        cache.DBProvider.Fastest<CacheDBItem<List<VariableBasicData>>>().PageSize(50000).BulkCopy(data);
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex, "Add cache fail");
                }
            }
        }
    }
    /// <summary>
    /// 添加队列，超限后会入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddQueueVarModel(CacheDBItem<VariableBasicData> data)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            //检测队列长度，超限存入缓存数据库
            if (_memoryVarModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
            {
                List<CacheDBItem<VariableBasicData>> list = null;
                lock (_memoryVarModelQueue)
                {
                    if (_memoryVarModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                    {
                        list = _memoryVarModelQueue.ToListWithDequeue();
                    }
                }
                AddCache(list);
            }
        }
        if (_memoryVarModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
        {
            lock (_memoryVarModelQueue)
            {
                if (_memoryVarModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                {
                    LogMessage?.LogWarning($"{typeof(VariableBasicData).Name} Queue exceeds limit, clear old data. If it doesn't work as expected, increase [QueueMaxCount] or Enable cache");
                    _memoryVarModelQueue.Clear();
                    _memoryVarModelQueue.Enqueue(data);
                    return;
                }
            }
        }
        else
        {
            _memoryVarModelQueue.Enqueue(data);
        }
    }

    /// <summary>
    /// 添加队列，超限后会入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddQueueVarModel(CacheDBItem<List<VariableBasicData>> data)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            //检测队列长度，超限存入缓存数据库
            if (_memoryVarModelsQueue.Count > _businessPropertyWithCache.QueueMaxCount)
            {
                List<CacheDBItem<List<VariableBasicData>>> list = null;
                lock (_memoryVarModelsQueue)
                {
                    if (_memoryVarModelsQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                    {
                        list = _memoryVarModelsQueue.ToListWithDequeue();
                    }
                }
                AddCache(list);
            }
        }
        if (_memoryVarModelsQueue.Count > _businessPropertyWithCache.QueueMaxCount)
        {
            lock (_memoryVarModelsQueue)
            {
                if (_memoryVarModelsQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                {
                    _memoryVarModelsQueue.Clear();
                    _memoryVarModelsQueue.Enqueue(data);
                    return;
                }
            }
        }
        else
        {
            _memoryVarModelsQueue.Enqueue(data);
        }
    }


    /// <summary>
    /// 获取缓存对象，注意using
    /// </summary>
    protected virtual CacheDB LocalDBCacheVarModel()
    {
        var cacheDb = CacheDBUtil.GetCache(typeof(CacheDBItem<VariableBasicData>), CurrentDevice.Name.ToString(), $"{CurrentDevice.PluginName}_{typeof(VariableBasicData).Name}");
        if (!LocalDBCacheVarModelInited)
        {
            cacheDb.InitDb();
            LocalDBCacheVarModelInited = true;
        }
        return cacheDb;
    }

    /// <summary>
    /// 获取缓存对象，注意using
    /// </summary>
    protected virtual CacheDB LocalDBCacheVarModels()
    {
        var cacheDb = CacheDBUtil.GetCache(typeof(CacheDBItem<List<VariableBasicData>>), CurrentDevice.Name.ToString(), $"{CurrentDevice.PluginName}_List_{typeof(VariableBasicData).Name}");
        if (!LocalDBCacheVarModelsInited)
        {
            cacheDb.InitDb();
            LocalDBCacheVarModelsInited = true;
        }
        return cacheDb;
    }


    /// <summary>
    /// 需实现上传到通道
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract ValueTask<OperResult> UpdateVarModel(IEnumerable<CacheDBItem<VariableBasicData>> item, CancellationToken cancellationToken);

    /// <summary>
    /// 需实现上传到通道
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract ValueTask<OperResult> UpdateVarModels(IEnumerable<VariableBasicData> item, CancellationToken cancellationToken);

    protected async Task UpdateVarModelCache(CancellationToken cancellationToken)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            #region //成功上传时，补上传缓存数据

            if (IsConnected())
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        //循环获取

                        var varList = await DBCacheVar.DBProvider.Queryable<CacheDBItem<VariableBasicData>>().Take(_businessPropertyWithCache.SplitSize).ToListAsync(cancellationToken).ConfigureAwait(false);
                        if (varList.Count > 0)
                        {
                            try
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    var result = await UpdateVarModel(varList, cancellationToken).ConfigureAwait(false);
                                    if (result.IsSuccess)
                                    {
                                        //删除缓存
                                        await DBCacheVar.DBProvider.Deleteable<CacheDBItem<VariableBasicData>>(varList).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false);
                                    }
                                    else
                                        break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (success)
                                    LogMessage?.LogWarning(ex);
                                success = false;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (success)
                        LogMessage?.LogWarning(ex);
                    success = false;
                }
            }

            #endregion //成功上传时，补上传缓存数据
        }
    }

    protected async Task UpdateVarModelsCache(CancellationToken cancellationToken)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            #region //成功上传时，补上传缓存数据

            if (IsConnected())
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        //循环获取

                        var varList = await DBCacheVars.DBProvider.Queryable<CacheDBItem<List<VariableBasicData>>>().FirstAsync(cancellationToken).ConfigureAwait(false);
                        if (varList?.Value?.Count > 0)
                        {
                            try
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    var result = await UpdateVarModels(varList.Value, cancellationToken).ConfigureAwait(false);
                                    if (result.IsSuccess)
                                    {
                                        //删除缓存
                                        await DBCacheVars.DBProvider.Deleteable<CacheDBItem<List<VariableBasicData>>>(varList).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false);
                                    }
                                    else
                                        break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (success)
                                    LogMessage?.LogWarning(ex);
                                success = false;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (success)
                        LogMessage?.LogWarning(ex);
                    success = false;
                }
            }

            #endregion //成功上传时，补上传缓存数据
        }
    }

    protected async Task UpdateVarModelMemory(CancellationToken cancellationToken)
    {
        #region //上传变量内存队列中的数据

        try
        {
            var list = _memoryVarModelQueue.ToListWithDequeue().ChunkBetter(_businessPropertyWithCache.SplitSize);
            foreach (var item in list)
            {
                try
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        var result = await UpdateVarModel(item, cancellationToken).ConfigureAwait(false);
                        if (!result.IsSuccess)
                        {
                            AddCache(item.ToList());
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (success)
                        LogMessage?.LogWarning(ex);
                    success = false;
                }
            }
        }
        catch (Exception ex)
        {
            if (success)
                LogMessage?.LogWarning(ex);
            success = false;
        }

        #endregion //上传变量内存队列中的数据
    }

    protected async Task UpdateVarModelsMemory(CancellationToken cancellationToken)
    {
        #region //上传变量内存队列中的数据

        try
        {
            var queues = _memoryVarModelsQueue.ToListWithDequeue();
            foreach (var cacheDBItem in queues)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                var list = cacheDBItem.Value;
                var data = list.ChunkBetter(_businessPropertyWithCache.SplitSize);
                foreach (var item in data)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            var result = await UpdateVarModels(item, cancellationToken).ConfigureAwait(false);
                            if (!result.IsSuccess)
                            {

                                AddCache(new List<CacheDBItem<List<VariableBasicData>>>() { new CacheDBItem<List<VariableBasicData>>(item.ToList()) });
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (success)
                            LogMessage?.LogWarning(ex);
                        success = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (success)
                LogMessage?.LogWarning(ex);
            success = false;
        }

        #endregion //上传变量内存队列中的数据
    }




    #endregion
}
