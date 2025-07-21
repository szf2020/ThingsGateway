using System.Data.Common;

namespace ThingsGateway.SqlSugar
{
    internal static class QuestDbRestAPHelper
    {
        /// <summary>
        /// 绑定RestAPI需要的信息
        /// </summary>
        public static void SetRestApiInfo(DbConnectionStringBuilder builder, ref string host, ref string httpPort, ref string username, ref string password)
        {
            if (builder.TryGetValue("Host", out object hostValue))
            {
                host = Convert.ToString(hostValue);
            }
            if (builder.TryGetValue("HttpPort", out object httpPortValue))
            {
                httpPort = Convert.ToString(httpPortValue);
            }
            if (builder.TryGetValue("Username", out object usernameValue))
            {
                username = Convert.ToString(usernameValue);
            }
            if (builder.TryGetValue("Password", out object passwordValue))
            {
                password = Convert.ToString(passwordValue);
            }
        }

        /// <summary>
        /// 逐行读取，包含空行
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<string> SplitByLine(string text)
        {
            List<string> lines = new List<string>();
            using (var sr = new StringReader(text))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    lines.Add(line);
                    line = sr.ReadLine();
                }
            }
            return lines;
        }
    }
}
