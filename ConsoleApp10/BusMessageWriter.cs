using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp10
{
    public class BusMessageWriter
    {
        readonly IBusConnection _connection = new();
        readonly MemoryStream _buffer = new();
        private static readonly SemaphoreLocker _locker = new SemaphoreLocker();

        public async Task SendMessageAsync(byte[] nextMessage)
        {
            //var result = await _locker.LockAsync(async () =>
            await _locker.LockAsync(async () =>
            {
                _buffer.Write(nextMessage, 0, nextMessage.Length);
                if (_buffer.Length > 1000)
                {
                    await _connection.PublishAsync(_buffer.ToArray());
                    _buffer.SetLength(0);
                }
            });
        }
    }
    interface IConnection
    {
        Task PublishAsync(byte[] a);
    }

    class IBusConnection : IConnection
    {
        public Task PublishAsync(byte[] a)
        {
            throw new NotImplementedException();
        }
    }

    public class SemaphoreLocker
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

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
    }

}
