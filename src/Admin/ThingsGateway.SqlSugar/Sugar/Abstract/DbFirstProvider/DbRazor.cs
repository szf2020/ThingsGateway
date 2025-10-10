using System.Text;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// Razor优先代码生成类
    /// </summary>
    public class RazorFirst
    {
        /// <summary>
        /// 类字符串列表
        /// </summary>
        internal List<KeyValuePair<string, string>> ClassStringList { get; set; }
        /// <summary>
        /// 文件名格式化函数
        /// </summary>
        internal Func<string, string> FormatFileNameFunc { get; set; }

        /// <summary>
        /// 默认Razor类模板
        /// </summary>
        public const string DefaultRazorClassTemplate =
@"using System;
using System.Linq;
using System.Text;
using ThingsGateway.SqlSugar;
namespace @Model.Namespace 
{
    ///<summary>
    ///
    ///</summary>
    public partial class @Model.ClassName
    {
           public @(Model.ClassName)(){


           }
 @foreach (var item in @Model.Columns)
   {
      if(item.IsPrimarykey&&item.IsIdentity){
         @:/// <summary>
         @:/// Desc:@item.ColumnDescription
         @:/// Default:@item.DefaultValue
         @:/// Nullable:@item.IsNullable
         @:/// </summary>     
         @:[ThingsGateway.SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = true)]      
         @:public @item.DataType @item.DbColumnName {get;set;}
         }
        else if(item.IsPrimarykey)
        {
         @:/// <summary>
         @:/// Desc:@item.ColumnDescription
         @:/// Default:@item.DefaultValue
         @:/// Nullable:@item.IsNullable
         @:/// </summary>    
         @:[ThingsGateway.SqlSugar.SugarColumn(IsPrimaryKey = true)]       
         @:public @item.DataType @item.DbColumnName {get;set;}
         } 
        else if(item.IsIdentity)
        {
         @:/// <summary>
         @:/// Desc:@item.ColumnDescription
         @:/// Default:@item.DefaultValue
         @:/// Nullable:@item.IsNullable
         @:/// </summary>       
         @:[ThingsGateway.SqlSugar.SugarColumn(IsIdentity = true)]    
         @:public @item.DataType @item.DbColumnName {get;set;}
         }
         else
         {
         @:/// <summary>
         @:/// Desc:@item.ColumnDescription
         @:/// Default:@item.DefaultValue
         @:/// Nullable:@item.IsNullable
         @:/// </summary>           
         @:public @item.DataType @item.DbColumnName {get;set;}
         }
       }

    }
}";

        /// <summary>
        /// 创建类文件
        /// </summary>
        public void CreateClassFile(string directoryPath)
        {
            var seChar = Path.DirectorySeparatorChar.ToString();
            if (ClassStringList.HasValue())
            {
                foreach (var item in ClassStringList)
                {
                    var fileName = item.Key;
                    if (this.FormatFileNameFunc != null)
                    {
                        fileName = this.FormatFileNameFunc(fileName);
                    }
                    var filePath = directoryPath.TrimEnd('\\').TrimEnd('/') + string.Format(seChar + "{0}.cs", fileName);
                    FileHelper.CreateFile(filePath, item.Value, Encoding.UTF8);
                }
            }
        }

        /// <summary>
        /// 获取类字符串列表
        /// </summary>
        public List<KeyValuePair<string, string>> GetClassStringList()
        {
            return ClassStringList;
        }
    }
}