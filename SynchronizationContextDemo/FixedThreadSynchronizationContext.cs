using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynchronizationContextDemo
{
    public class FixedThreadSynchronizationContext: SynchronizationContext
    {
        private readonly ConcurrentQueue<(SendOrPostCallback Callback, object State)> _workItems;

        public FixedThreadSynchronizationContext()
        {
            _workItems = new ConcurrentQueue<(SendOrPostCallback callback, object state)>();
            var thread = new Thread(StartLoop);
            Console.WriteLine("FixedThreadSynchronizationContext.ThreadId:{0}", thread.ManagedThreadId);
            thread.Start();

            void StartLoop()
            {
                while (true)
                {
                    if (_workItems.TryDequeue(out var workItem))
                    {
                        workItem.Callback(workItem.State);
                    }
                }
            }
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _workItems.Enqueue((d, state));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            base.Send(d, state);
        }
    }

    public class FixedThreadSynchronizationContextTest
    {
        public static async Task Test()
        {
            var synchronizationContext = new FixedThreadSynchronizationContext();
            for(int i=0; i< 5; i++)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                    Invoke();
                });
            }
            Console.Read();

            void Invoke()
            {
                var dispatchThreadId = Thread.CurrentThread.ManagedThreadId;
                SendOrPostCallback callback = _ => Console.WriteLine($"Pooled Thread: {dispatchThreadId}; Execution Thread: {Thread.CurrentThread.ManagedThreadId}");
                SynchronizationContext.Current.Post(callback, null);
            }
        }         
    }

    public class FixedThreadSynchronizationContextAwaitTest
    {
        public static async Task Test()
        {
            SynchronizationContext.SetSynchronizationContext(new FixedThreadSynchronizationContext());
            //await Task.Delay(100)
            await Task.Delay(100).ConfigureAwait(false);//将不在同步上下文中执行
            Console.WriteLine("Await Thread:{0}", Thread.CurrentThread.ManagedThreadId);
        }
    }
}
