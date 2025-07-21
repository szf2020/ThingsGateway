namespace ThingsGateway.SqlSugar
{
    public class SugarAsyncLock : IDisposable
    {
        static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1);

        public SugarAsyncLock(SqlSugarProvider db)
        {
        }

        public async Task<SugarAsyncLock> AsyncLock(int timeOutSeconds)
        {
            TimeSpan timeout = TimeSpan.FromSeconds(timeOutSeconds);
            await SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(false);
            return this;
        }

        public void Dispose()
        {
            SemaphoreSlim.Release();
        }
    }
}
