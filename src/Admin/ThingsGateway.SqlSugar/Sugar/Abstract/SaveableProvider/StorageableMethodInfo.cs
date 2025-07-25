using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    public class StorageableMethodInfo
    {
        internal SqlSugarProvider Context { get; set; }
        internal MethodInfo MethodInfo { get; set; }
        internal object objectValue { get; set; }
        public int ExecuteCommand()
        {
            if (Context == null) return 0;
            object objectValue = null;
            MethodInfo method = GetSaveMethod(ref objectValue);
            if (method == null) return 0;
            return (int)method.Invoke(objectValue, Array.Empty<object>());
        }

        public StorageableAsMethodInfo AsInsertable
        {
            get
            {
                var type = "AsInsertable";
                return GetAs(type);
            }
        }
        public StorageableAsMethodInfo AsUpdateable
        {
            get
            {
                var type = "AsUpdateable";
                return GetAs(type);
            }
        }

        private StorageableAsMethodInfo GetAs(string type)
        {
            object objectValue = null;
            MethodInfo method = GetSaveMethod(ref objectValue);
            if (method == null) return new StorageableAsMethodInfo(null);
            method = objectValue.GetType().GetMethod(nameof(ToStorage));
            objectValue = method.Invoke(objectValue, Array.Empty<object>());
            StorageableAsMethodInfo result = new StorageableAsMethodInfo(type);
            result.ObjectValue = objectValue;
            result.Method = method;
            return result;
        }

        private MethodInfo GetSaveMethod(ref object callValue)
        {
            if (objectValue == null)
                return null;
            callValue = MethodInfo.Invoke(Context, new object[] { objectValue });
            return callValue.GetType().GetMyMethod(nameof(ExecuteCommand), 0);
        }

        public StorageableMethodInfo ToStorage()
        {
            return this;
        }

        public StorageableSplitTableMethodInfo SplitTable()
        {
            object objectValue = null;
            MethodInfo method = GetSaveMethod(ref objectValue);
            if (method == null) return new StorageableSplitTableMethodInfo(null);
            method = objectValue.GetType().GetMethod(nameof(SplitTable));
            objectValue = method.Invoke(objectValue, Array.Empty<object>());
            StorageableSplitTableMethodInfo result = new StorageableSplitTableMethodInfo(null);
            result.ObjectValue = objectValue;
            result.Method = method;
            return result;
        }

        public StorageableSplitTableMethodInfo AS(string tableName)
        {
            object objectValue = null;
            MethodInfo method = GetSaveMethod(ref objectValue);
            if (method == null) return new StorageableSplitTableMethodInfo(null);
            method = objectValue.GetType().GetMyMethod(nameof(QueryMethodInfo.AS), 1);
            objectValue = method.Invoke(objectValue, new object[] { tableName });
            StorageableSplitTableMethodInfo result = new StorageableSplitTableMethodInfo(null);
            result.ObjectValue = objectValue;
            result.Method = method;
            return result;
        }

        public StorageableSplitTableMethodInfo WhereColumns(string[] strings)
        {
            object objectValue = null;
            MethodInfo method = GetSaveMethod(ref objectValue);
            if (method == null) return new StorageableSplitTableMethodInfo(null);
            method = objectValue.GetType().GetMyMethod("WhereColumns", 1, typeof(string[]));
            objectValue = method.Invoke(objectValue, new object[] { strings });
            StorageableSplitTableMethodInfo result = new StorageableSplitTableMethodInfo(null);
            result.ObjectValue = objectValue;
            result.Method = method;
            return result;
        }
    }

    public class StorageableAsMethodInfo
    {
        private StorageableAsMethodInfo() { }
        private string type;
        public StorageableAsMethodInfo(string type)
        {
            this.type = type;
        }
        internal object ObjectValue { get; set; }
        internal MethodInfo Method { get; set; }
        public int ExecuteCommand()
        {
            if (type == null) return 0;
            PropertyInfo property = ObjectValue.GetType().GetProperty(type);
            var value = property.GetValue(ObjectValue);
            var newObj = value.GetType().GetMethod(nameof(ExecuteCommand)).Invoke(value, Array.Empty<object>());
            return (int)newObj;
        }
        public StorageableCommonMethodInfo IgnoreColumns(params string[] ignoreColumns)
        {
            PropertyInfo property = ObjectValue?.GetType().GetProperty(type);
            var value = property?.GetValue(ObjectValue);
            var newObj = value?.GetType().GetMyMethod(nameof(IgnoreColumns), 1, typeof(string[])).Invoke(value, new object[] { ignoreColumns });
            StorageableCommonMethodInfo result = new StorageableCommonMethodInfo();
            result.Value = newObj;
            return result;
        }
    }
    public class StorageableCommonMethodInfo
    {
        public object Value { get; set; }
        public int ExecuteCommand()
        {
            if (Value == null) return 0;
            var newObj = Value.GetType().GetMethod(nameof(ExecuteCommand)).Invoke(Value, Array.Empty<object>());
            return (int)newObj;
        }
    }

    public class StorageableSplitTableMethodInfo
    {
        private StorageableSplitTableMethodInfo() { }
        private string type;
        public StorageableSplitTableMethodInfo(string type)
        {
            this.type = type;
        }
        internal object ObjectValue { get; set; }
        internal MethodInfo Method { get; set; }
        public int ExecuteCommand()
        {
            var newObj = ObjectValue.GetType().GetMethod(nameof(ExecuteCommand)).Invoke(ObjectValue, Array.Empty<object>());
            return (int)newObj;
        }
    }
}
