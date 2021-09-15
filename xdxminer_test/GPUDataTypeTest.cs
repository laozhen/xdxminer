using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;
using xdxminer_lib;

namespace xdxminer_test
{
    [TestClass]
    public class GPUDataTypeTest
    {
        [TestMethod]
        public void marshall ()
        {
            Marshal.SizeOf(typeof(DagItem));
            //Marshal.SizeOf(typeof(Uint4));
            Marshal.SizeOf(typeof(Uint2));
            Marshal.SizeOf(typeof(MineParams));
            Marshal.SizeOf(typeof(MineResult));

        }
    }
}
