using Anantashesha.Decompiler.ProcedureAnalyzers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Anantashesha
{
    unsafe class Program
    {
        static void Main(string[] args)
        {
            string lname = @"C:\Program Files (x86)\GameON\RED STONE\RedStoneLocal.exe",
                     dname = @"C:\Users\daigo\Documents\redstone_new2\Red Stone_dump.exe";
            Procedure.ProcedureFinder(lname);
            //BayesianNetwork.Fit();
            Console.ReadLine();
        }
    }
}
