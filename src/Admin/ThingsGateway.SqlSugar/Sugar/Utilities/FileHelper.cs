using System.Text;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 文件操作辅助类
    /// </summary>
    internal static class FileHelper
    {
        /// <summary>
        /// 创建文件并写入文本内容
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="text">要写入的文本内容</param>
        /// <param name="encoding">文本编码格式</param>
        /// <exception cref="Exception">文件操作过程中可能抛出的异常</exception>
        public static void CreateFile(string filePath, string text, Encoding encoding)
        {
            if (IsExistFile(filePath))
            {
                DeleteFile(filePath);
            }
            if (!IsExistFile(filePath))
            {
                string directoryPath = GetDirectoryFromFilePath(filePath);
                CreateDirectory(directoryPath);

                //Create File
                FileInfo file = new FileInfo(filePath);
                using (FileStream stream = file.Create())
                {
                    using (StreamWriter writer = new StreamWriter(stream, encoding))
                    {
                        writer.Write(text);
                        writer.Flush();
                    }
                }
            }
        }

        /// <summary>
        /// 检查目录是否存在
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <returns>如果目录存在返回true，否则返回false</returns>
        public static bool IsExistDirectory(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="directoryPath">要创建的目录路径</param>
        public static void CreateDirectory(string directoryPath)
        {
            if (!IsExistDirectory(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="filePath">要删除的文件路径</param>
        public static void DeleteFile(string filePath)
        {
            if (IsExistFile(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// 从文件路径获取所在目录
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件所在目录的完整路径</returns>
        public static string GetDirectoryFromFilePath(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            DirectoryInfo directory = file.Directory;
            return directory.FullName;
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>如果文件存在返回true，否则返回false</returns>
        public static bool IsExistFile(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}