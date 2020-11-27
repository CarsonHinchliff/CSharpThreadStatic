using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityContextApp
{
    class ConfigHelper
    {
        public static string GetValue(string key) => System.Configuration.ConfigurationManager.AppSettings[key] ?? "";
    }
}
