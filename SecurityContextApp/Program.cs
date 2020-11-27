using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecurityContextApp
{
    public delegate WindowsIdentity AuthDelegate(string p1, string p2, string p3);
    class Program
    {
        static void Main(string[] args)
        {
            var getIdentityFunc = ConfigHelper.GetValue("AuthType") == "9" ? 
                new AuthDelegate(Logon.GetRemoteWindowsIdentity) : new AuthDelegate(Logon.GetLocalWindowsIdentity);

            Console.WriteLine("Before impersonating: {0}", WindowsIdentity.GetCurrent().Name);
            //using (Logon.GetLocalWindowsIdentity(@"CarsonMax", "1913").Impersonate())
            using (getIdentityFunc(
                ConfigHelper.GetValue("WinUserName"), 
                ConfigHelper.GetValue("WinUserPwd"),
                ConfigHelper.GetValue("WinComputerName")).Impersonate())
            //using (Logon.GetRemoteWindowsIdentity(@"Administrator", "191", "CarsonMax").Impersonate())
            {
                //SecurityContext.SuppressFlowWindowsIdentity();
                Console.WriteLine("Within Impersonation context: {0}", WindowsIdentity.GetCurrent().Name);
                PrintPathFiles(@"C:\");
                new Thread(() =>{
                    Console.WriteLine("Async thread: {0}", WindowsIdentity.GetCurrent().Name);
                    PrintPathFiles(@"C:\");
                }).Start();
            }
            Console.WriteLine("Undo impersonation: {0}", WindowsIdentity.GetCurrent().Name);
            Console.Read();
        }

        static void PrintPathFiles(string path)
        {
            foreach(var info in new System.IO.DirectoryInfo(path).GetFileSystemInfos())
            {
                Console.WriteLine(info.FullName);
            }
        }
    }
}
