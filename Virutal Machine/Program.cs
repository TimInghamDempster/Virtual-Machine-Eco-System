using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
	public struct StatsCounters
	{
		public int InstructionsExecuted;
		public int LoadWaits;
		public int StoreWaits;
		public int FetchWaits;
		public int InterruptWaits;
		public int ICacheHits;
		public int ICacheMisses;
	}

    public class Program
    {
       
        static void Main(string[] args)
        {
            var vm = new VirtualMachine();

            vm.Run();
        }
    }
}
