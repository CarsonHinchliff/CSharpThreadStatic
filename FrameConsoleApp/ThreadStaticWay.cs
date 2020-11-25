using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrameConsoleApp
{
    /// <summary>
    /// 说明：线程间传递数据的三种方式（不通过Thread.Start(arguments)传递）
    /// 方式一:[ThreadStatic]特性标注要传递或共享的数据
    /// 方式二:通过ThreadLocal<T>泛型类型标注要传递或共享的数据
    /// 方式三:通过CallContext（Remoting.Message命名空间）的静态方法GetData/SetData/FreeNamedDataSlot
    /// </summary>
    class ThreadStaticWay
    {
        public class CallStackContext : Dictionary<string, object>
        {
            private static int _traceId = 0;

            //方法1
            //[ThreadStatic]//ThreadStatic特性進行標注
            //private static CallStackContext _current;
            //public static CallStackContext Current { get => _current; set => _current = value; }

            //方法二
            ////也可以使用ThreaLocal泛型类型, 即TLS的方式
            //private static ThreadLocal<CallStackContext> _current = new ThreadLocal<CallStackContext>();  
            //public static CallStackContext Current { get => _current.Value; set => _current.Value = value; }

            //方法三
            //使用CallContext
            public static CallStackContext Current => CallContext.GetData(nameof(CallStackContext)) as CallStackContext;

            public long TraceId { get; } = Interlocked.Increment(ref _traceId);
        }

        public class CallStack : IDisposable
        {
            //方法三
            //使用CallContext
            //public CallStack() => CallStackContext.Current = new CallStackContext();
            //public void Dispose()
            //{
            //    CallStackContext.Current = null;
            //}

            public CallStack() => CallContext.SetData(nameof(CallStackContext), new CallStackContext());
            public void Dispose()
            {
                CallContext.FreeNamedDataSlot(nameof(CallStackContext));
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
                Foo();
                Bar();
                Baz();
            }
        }

        static void Foo() => Trace();
        static void Bar() => Trace();
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
