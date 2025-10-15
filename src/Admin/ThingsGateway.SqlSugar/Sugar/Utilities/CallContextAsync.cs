using System.Collections.Concurrent;

namespace ThingsGateway.SqlSugar
{
    public static class CallContextAsync<T>
    {
        static NonBlockingDictionary<string, AsyncLocal<T>> state = new NonBlockingDictionary<string, AsyncLocal<T>>();
        public static void SetData(string name, T data) =>
            state.GetOrAdd(name, _ => new AsyncLocal<T>()).Value = data;
        public static T GetData(string name) =>
            state.TryGetValue(name, out AsyncLocal<T> data) ? data.Value : default(T);
    }

    public static class CallContextThread<T>
    {
        static NonBlockingDictionary<string, ThreadLocal<T>> state = new NonBlockingDictionary<string, ThreadLocal<T>>();
        public static void SetData(string name, T data) =>
            state.GetOrAdd(name, _ => new ThreadLocal<T>()).Value = data;
        public static T GetData(string name) =>
            state.TryGetValue(name, out ThreadLocal<T> data) ? data.Value : default(T);
    }
}
