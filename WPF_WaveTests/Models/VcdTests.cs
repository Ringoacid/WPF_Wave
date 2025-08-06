using Microsoft.VisualStudio.TestTools.UnitTesting;
using WPF_Wave.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Wave.Models.Tests
{
    [TestClass()]
    public class VcdTests
    {
        [TestMethod()]
        public void LoadFromFileTest()
        {
            Vcd vcd = new Vcd();

            vcd.LoadFromFile(@"Assets/SampleVcd/MCS4Test.vcd");

            vcd.LoadFromFile(@"Assets/SampleVcd/RomTest.vcd");

        }
    }
}