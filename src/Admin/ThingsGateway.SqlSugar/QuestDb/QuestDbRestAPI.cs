using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

using Newtonsoft.Json;

using System.Collections;
using System.Data.Common;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// QuestDB REST API 客户端
    /// </summary>
    public class QuestDbRestAPI
    {
        internal string url = string.Empty;
        internal string authorization = string.Empty;
        internal static Random random = new Random();
        // 可修改的数据库客户端
        ISqlSugarClient db;

        /// <summary>
        /// 初始化 QuestDbRestAPI 实例
        /// </summary>
        /// <param name="db">SqlSugar 数据库客户端</param>
        /// <param name="httpPort">restApi端口</param>
        public QuestDbRestAPI(ISqlSugarClient db, int httpPort = 9000)
        {
            var builder = new DbConnectionStringBuilder();
            builder.ConnectionString = db.CurrentConnectionConfig.ConnectionString;
            this.db = db;
            string host = String.Empty;
            string username = String.Empty;
            string password = String.Empty;
            QuestDbRestAPHelper.SetRestApiInfo(builder, ref host, ref username, ref password);
            BindHost(host, httpPort, username, password);
        }

        /// <summary>
        /// 异步执行SQL命令
        /// </summary>
        /// <param name="sql">要执行的SQL语句</param>
        /// <returns>执行结果</returns>
        public async Task<string> ExecuteCommandAsync(string sql)
        {
            // HTTP GET 请求执行SQL
            var result = string.Empty;
            var url = $"{this.url}/exec?query={HttpUtility.UrlEncode(sql)}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrWhiteSpace(authorization))
            {
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorization);
            }

            using var httpResponseMessage = await client.SendAsync(request).ConfigureAwait(false);
            result = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// 同步执行SQL命令
        /// </summary>
        /// <param name="sql">要执行的SQL语句</param>
        /// <returns>执行结果</returns>
        public string ExecuteCommand(string sql)
        {
            return ExecuteCommandAsync(sql).GetAwaiter().GetResult();
        }

        ///// <summary>
        ///// 异步批量插入单条数据
        ///// </summary>
        ///// <typeparam name="T">数据类型</typeparam>
        ///// <param name="insertData">要插入的数据</param>
        ///// <param name="dateFormat">日期格式字符串</param>
        ///// <returns>影响的行数</returns>
        //public async Task<int> BulkCopyAsync<T>(T insertData, string dateFormat = "yyyy/M/d H:mm:ss") where T : class, new()
        //{
        //    if (db.CurrentConnectionConfig.MoreSettings == null)
        //        db.CurrentConnectionConfig.MoreSettings = new ConnMoreSettings();
        //    db.CurrentConnectionConfig.MoreSettings.DisableNvarchar = true;
        //    var sql = db.InsertableT(insertData).ToSqlString();
        //    var result = await ExecuteCommandAsync(sql).ConfigureAwait(false);
        //    return result.Contains("OK", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        //}

        ///// <summary>
        ///// 同步批量插入单条数据
        ///// </summary>
        ///// <typeparam name="T">数据类型</typeparam>
        ///// <param name="insertData">要插入的数据</param>
        ///// <param name="dateFormat">日期格式字符串</param>
        ///// <returns>影响的行数</returns>
        //public int BulkCopy<T>(T insertData, string dateFormat = "yyyy/M/d H:mm:ss") where T : class, new()
        //{
        //    return BulkCopyAsync(insertData, dateFormat).GetAwaiter().GetResult();
        //}

        /// <summary>
        /// 创建分页批量插入器
        /// </summary>
        /// <param name="pageSize">每页大小</param>
        /// <returns>分页批量插入器实例</returns>
        public QuestDbPageSizeBulkCopy PageSize(int pageSize)
        {
            QuestDbPageSizeBulkCopy result = new QuestDbPageSizeBulkCopy(this, pageSize, db);
            return result;
        }

        private readonly HttpClient client = new HttpClient();

        /// <summary>
        /// 异步批量快速插入数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="insertList">要插入的数据列表</param>
        /// <param name="tableName">表名称</param>
        /// <param name="dateFormat">日期格式字符串</param>
        /// <returns>插入的记录数</returns>
        public async Task<int> BulkCopyAsync<T>(List<T> insertList, string tableName = null, string dateFormat = "yyyy/M/d H:mm:ss") where T : class, new()
        {
            var result = 0;
            var fileName = $"{Guid.NewGuid()}.csv";
            var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            try
            {
                // 准备多部分表单数据
                var boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
                var list = new List<Hashtable>();
                tableName ??= db.EntityMaintenance.GetEntityInfo<T>().DbTableName;

                // 获取或创建列信息缓存
                var key = "QuestDbBulkCopy" + typeof(T).FullName + typeof(T).GetHashCode();
                var columns = ReflectionInoCacheService.Instance.GetOrCreate(key, () =>
                 db.CopyNew().DbMaintenance.GetColumnInfosByTableName(tableName));

                // 构建schema信息
                columns.ForEach(d =>
                {
                    if (d.DataType == "TIMESTAMP")
                    {
                        list.Add(new Hashtable()
                        {
                            { "name", d.DbColumnName },
                            { "type", d.DataType },
                            { "pattern", dateFormat}
                        });
                    }
                    else
                    {
                        list.Add(new Hashtable()
                        {
                            { "name", d.DbColumnName },
                            { "type", d.DataType }
                        });
                    }
                });
                var schema = JsonConvert.SerializeObject(list);

                // 写入CSV文件
                using (var writer = new StreamWriter(filePath))
                using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
                {
                    var options = new TypeConverterOptions { Formats = new[] { GetDefaultFormat() } };
                    csv.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                    CsvCreating<T>(csv);
                    await csv.WriteRecordsAsync(insertList).ConfigureAwait(false);
                }

                // 准备HTTP请求内容
                using var httpContent = new MultipartFormDataContent(boundary);
                using var fileStream = File.OpenRead(filePath);
                //if (!string.IsNullOrWhiteSpace(this.authorization))
                //    client.DefaultRequestHeaders.Add("Authorization", this.authorization);
                httpContent.Add(new StringContent(schema), "schema");
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                httpContent.Add(streamContent, "data", Path.GetFileName(filePath));

                // 处理boundary带双引号可能导致服务器错误的情况
                httpContent.Headers.Remove("Content-Type");
                httpContent.Headers.TryAddWithoutValidation("Content-Type",
                    "multipart/form-data; boundary=" + boundary);

                // 发送请求并处理响应
              using  var httpResponseMessage =
                    await Post(client, tableName, httpContent).ConfigureAwait(false);
                var readAsStringAsync = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                var splitByLine = QuestDbRestAPHelper.SplitByLine(readAsStringAsync);

                // 解析响应结果
                foreach (var s in splitByLine)
                {
                    if (s.Contains("Rows"))
                    {
                        var strings = s.Split('|');
                        if (strings[1].Trim() == "Rows imported")
                        {
                            result = Convert.ToInt32(strings[2].Trim());
                        }
                    }
                }
            }
            finally
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    // 忽略删除文件时的异常
                }
            }
            return result;
        }

        /// <summary>
        /// 配置CSV写入器
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="csv">CSV写入器</param>
        private void CsvCreating<T>(CsvWriter csv) where T : class, new()
        {
            var entityColumns = db.EntityMaintenance.GetEntityInfo<T>().Columns;
            if (entityColumns.Any(it => it.IsIgnore || it.UnderType?.IsEnum == true))
            {
                var customMap = new DefaultClassMap<T>();
                foreach (var item in entityColumns.Where(it => !it.IsIgnore))
                {
                    var memberMap = customMap.Map(typeof(T), item.PropertyInfo).Name(item.PropertyName);
                    if (item.UnderType?.IsEnum == true
                        && item.SqlParameterDbType == null
                        && db.CurrentConnectionConfig?.MoreSettings?.TableEnumIsString != true)
                    {
                        memberMap.TypeConverter<CsvHelperEnumToIntConverter>();
                    }
                }
                csv.Context.RegisterClassMap(customMap);
            }
        }

        /// <summary>
        /// 获取默认日期格式
        /// </summary>
        /// <returns>日期格式字符串</returns>
        private static string GetDefaultFormat()
        {
            return "yyyy-MM-ddTHH:mm:ss.fffffff";
        }

        /// <summary>
        /// 发送POST请求
        /// </summary>
        /// <param name="client">HTTP客户端</param>
        /// <param name="name">表名</param>
        /// <param name="httpContent">请求内容</param>
        /// <returns>HTTP响应消息</returns>
        private Task<HttpResponseMessage> Post(HttpClient client, string name, MultipartFormDataContent httpContent)
        {
            return client.PostAsync($"{this.url}/imp?name={name}", httpContent);
        }

        /// <summary>
        /// 同步批量快速插入数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="insertList">要插入的数据列表</param>
        /// <param name="tableName">表名称</param>
        /// <param name="dateFormat">日期格式字符串</param>
        /// <returns>插入的记录数</returns>
        public int BulkCopy<T>(List<T> insertList, string tableName = null, string dateFormat = "yyyy/M/d H:mm:ss") where T : class, new()
        {
            return BulkCopyAsync(insertList, tableName, dateFormat).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 绑定主机信息
        /// </summary>
        /// <param name="host">主机地址</param>
        /// <param name="httpPort">HTTP端口</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        private void BindHost(string host, int httpPort, string username, string password)
        {
            url = host;
            if (url.EndsWith('/'))
                url = url.Remove(url.Length - 1);

            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                url = $"http://{url}";

            url = $"{url}:{httpPort}";

            // 生成Basic Auth Token
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                authorization = $"Basic {base64}";
            }
        }
    }
}