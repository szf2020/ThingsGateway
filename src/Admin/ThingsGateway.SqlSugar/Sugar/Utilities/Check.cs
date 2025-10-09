namespace ThingsGateway.SqlSugar
{
    public static class Check
    {
        public static void ThrowNotSupportedException(string message)
        {
            message = message.IsNullOrEmpty() ? new NotSupportedException().Message : message;
            throw new SqlSugarException($"SqlSugarException.NotSupportedException：{message}");
        }



        public static void ExceptionLang(string enMessage, string cnMessage)
        {
            throw new SqlSugarException(ErrorMessage.GetThrowMessage(enMessage, cnMessage));
        }


    }
}
