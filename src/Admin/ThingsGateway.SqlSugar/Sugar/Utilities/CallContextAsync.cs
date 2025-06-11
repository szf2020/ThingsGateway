using System.Collections.Concurrent;

namespace ThingsGateway.SqlSugar
{
    public static class CallContextAsync<T>
    {
        static ConcurrentDictionary<string, AsyncLocal<T>> state = new ConcurrentDictionary<string, AsyncLocal<T>>();
        public static void SetData(string name, T data) =>
            state.GetOrAdd(name, _ => new AsyncLocal<T>()).Value = data;
        public static T GetData(string name) =>
            state.TryGetValue(name, out AsyncLocal<T> data) ? data.Value : default(T);
    }

    public static class CallContextThread<T>
    {
        static ConcurrentDictionary<string, ThreadLocal<T>> state = new ConcurrentDictionary<string, ThreadLocal<T>>();
        public static void SetData(string name, T data) =>
            state.GetOrAdd(name, _ => new ThreadLocal<T>()).Value = data;
        public static T GetData(string name) =>
            state.TryGetValue(name, out ThreadLocal<T> data) ? data.Value : default(T);
    }
}
