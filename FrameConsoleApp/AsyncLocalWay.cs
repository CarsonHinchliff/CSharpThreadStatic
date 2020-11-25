using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrameConsoleApp
{
    class AsyncLocalWay
    {
        public class CallStackContext : Dictionary<string, object>
        {
            private static int _traceId = 0;

            //方法二
            //也可以使用ThreaLocal泛型类型, 即TLS的方式
            private static AsyncLocal<CallStackContext> _current = new AsyncLocal<CallStackContext>();
            public static CallStackContext Current { get => _current.Value; set => _current.Value = value; }

            public long TraceId { get; } = Interlocked.Increment(ref _traceId);
        }

        public class CallStack : IDisposable
        {
            public CallStack() => CallStackContext.Current = new CallStackContext();
            public void Dispose()
            {
                CallStackContext.Current = null;
            }
        }

        public static void Test()
        {
            for (int i = 0; i < 5; i++)
            {
                ThreadPool.QueueUserWorkItem(_ => Call());
            }

            Console.Read();
        }

        static void Call()
        {
            using (new CallStack())
            {
                CallStackContext.Current["argument"] = Guid.NewGuid();
                Foo();//Cross thread call
            }
        }

        static void Foo()
        {
            Trace();
            new Thread(Bar).Start();
        }
        static void Bar()
        {
            Trace();
            new Thread(Baz).Start();
        }
        static void Baz() => Trace();

        static void Trace([CallerMemberName] string methodName = null)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var traceId = CallStackContext.Current?.TraceId;
            var argument = CallStackContext.Current?["argument"];

            Console.WriteLine($"Thread: {threadId}; TraceId: {traceId}; Method: {methodName}; Argument: {argument}");
        }
    }
}
