using Odey.ReconciliationServices.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odey.ReconciliationServices.FMKeeleyReconciliationService.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            FMKeeleyReconcilationClient client = new FMKeeleyReconcilationClient();
            client.SendFMAdministratorDifferences();
        }
    }
}
