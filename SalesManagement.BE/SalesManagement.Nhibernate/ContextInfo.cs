using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesManagement.Nhibernate
{
    public static class ContextInfo
    {
        public static string GetEnvironment()
        {
            var environment = System.Configuration.ConfigurationManager.AppSettings["ORMEnvironment"];

            if (environment != null)
            {
                return environment;
            }

            return "Web";
        }
    }
}
