using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SecurityContextApp
{
    class Logon
    {
        // logon types
        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_LOGON_NETWORK = 3;
        const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        [DllImport("advapi32.dll")]
        public static extern int LogonUser(string lpszUserName,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        public static WindowsIdentity GetRemoteWindowsIdentity(string username, string password, string machine = "")
        {
            if (string.IsNullOrEmpty(machine)) machine = Environment.MachineName;

            IntPtr token = IntPtr.Zero;
            var status = LogonUser(username, machine, password, LOGON32_LOGON_NEW_CREDENTIALS, 0, ref token);
            if (status != 0)
            {
                return new WindowsIdentity(token);
            }
            throw new InvalidProgramException("Invalid user name or password");
        }

        public static WindowsIdentity GetLocalWindowsIdentity(string username, string password, string machine = "")
        {
            if (string.IsNullOrEmpty(machine)) machine = Environment.MachineName;

            IntPtr token = IntPtr.Zero;
            var status = LogonUser(username, machine, password, LOGON32_LOGON_INTERACTIVE, 0, ref token);
            if (status != 0)
            {
                return new WindowsIdentity(token);
            }
            throw new InvalidProgramException("Invalid user name or password");
        }
    }
}
