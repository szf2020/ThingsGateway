namespace SqlSugar.DistributedSystem.Snowflake
{
    public class InvalidSystemClock : Exception
    {
        public InvalidSystemClock(string message) : base(message) { }

        public InvalidSystemClock() : base()
        {
        }

        public InvalidSystemClock(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}