using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp10
{
    public class BusMessageWriter
    {
        private static readonly SemaphoreLocker _locker = new();

        public MemoryStream Buffer { get; } = new();

        internal IBusConnection? Connection { get; }

        public async Task SendMessageAsync(byte[] nextMessage)
        {
            //var result = await _locker.LockAsync(async () =>
            await _locker.LockAsync(async () =>
            {
                Buffer.Write(nextMessage, 0, nextMessage.Length);
                if (Buffer.Length > 1000)
                {
                    await Connection.PublishAsync(Buffer.ToArray());
                    Buffer.SetLength(0);
                }
            });
        }
    }

    internal interface IConnection
    {
        Task PublishAsync(byte[] a);
    }

    internal class IBusConnection : IConnection
    {
        public Task PublishAsync(byte[] a)
        {
            throw new NotImplementedException();
        }
    }

    public class SemaphoreLocker : IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task LockAsync(Func<Task> worker)
        {
            await _semaphore.WaitAsync();
            try
            {
                await worker();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // overloading variant for non-void methods with return type (generic T)
        public async Task<T> LockAsync<T>(Func<Task<T>> worker)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await worker();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

}
