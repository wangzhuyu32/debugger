using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace LeEcoDebugger
{
    class misc
    {

    }

    public class ShortTask
    {
       public static T Execute<T>(int timeout, T defaultValue, Func<T> a)
        {
            try
            {
                CancellationTokenSource source = new CancellationTokenSource();
                Task<T> t = Task.Factory.StartNew<T>(() =>
                {
                    return a.Invoke();
                }, source.Token);
                if (t.Wait(timeout))
                    return t.Result;
                source.Cancel();
            }
            catch (Exception e) { }
            return defaultValue;
        }
    }

    public struct T32_Info_type
    {
        public string dumpPath { get; set;}
        public string elfPath { get; set; }
        public string CPU_name { get; set;}
        public string subSystem { get; set;}
        public string targetName { get; set;}
        public string loadBinaryCommand { get; set; }
    }

    public class RelocableType
    {
        public uint startAddr;
        public uint size;
        public RelocableType(uint startAddr, uint size)
        {
            this.startAddr = startAddr;
            this.size = size;
        }
    }
}
