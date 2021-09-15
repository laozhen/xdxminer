using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xdxminer_lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace xdxminer_test
{
    [TestClass]
    public class MessageTest
    {
        [TestMethod]
        public void testNewJob ()
        {
            StratumCommand stratumCommand = new StratumCommand();
            stratumCommand.parameters =
                new System.Collections.ArrayList {
                    "3766097a40b1443b7761de9e513e354ed2021f26e62ae414d6e8782a2a0483ea",
                    "3766097a40b1443b7761de9e513e354ed2021f26e62ae414d6e8782a2a0483ea",
                    "cd8cf77a2b4c6ed3472e1161861d300d052cc5f4793cb675c45215fea72bdcec",
                    "00000000ffff00000000ffff00000000ffff00000000ffff00000000ffff0000",
                    true};
            // 00000000_ffff0000_0000ffff_00000000_ffff0000_0000ffff_00000000_ffff0000",

            NewJob newJob = new NewJob(stratumCommand, Protocol.Stratum);
            Assert.AreEqual((uint)0x00000000, newJob.mineParams.target0);
            Assert.AreEqual((uint)0xffff0000, newJob.mineParams.target1);
            Assert.AreEqual((uint)0x0000ffff, newJob.mineParams.target2);
            Assert.AreEqual((uint)0x00000000, newJob.mineParams.target3);
            Assert.AreEqual((uint)0xffff0000, newJob.mineParams.target4);
            Assert.AreEqual((uint)0x0000ffff, newJob.mineParams.target5);
            Assert.AreEqual((uint)0x00000000, newJob.mineParams.target6);
            Assert.AreEqual((uint)0xffff0000, newJob.mineParams.target7);
            Assert.AreEqual((UInt64)0x3b44b1407a096637, newJob.mineParams.header0);
            Assert.AreEqual((UInt64)0x4e353e519ede6177, newJob.mineParams.header1);
            Assert.AreEqual((UInt64)0x14e42ae6261f02d2, newJob.mineParams.header2);

        }

        [TestMethod]
        public void testNewJob_diff()
        {
            StratumCommand stratumCommand = new StratumCommand();
            stratumCommand.parameters =
                new System.Collections.ArrayList {
                    "3766097a40b1443b7761de9e513e354ed2021f26e62ae414d6e8782a2a0483ea",
                    "3766097a40b1443b7761de9e513e354ed2021f26e62ae414d6e8782a2a0483ea",
                    "cd8cf77a2b4c6ed3472e1161861d300d052cc5f4793cb675c45215fea72bdcec",
                    true,};
            // 00000000_ffff0000_0000ffff_00000000_ffff0000_0000ffff_00000000_ffff0000",
            // 00000001_12e0be82_6d683133_90000000_00000000_00000000_00000000_00000000
            NewJob newJob = new NewJob(stratumCommand, Protocol.EthereumStratum_1,  "0000000112e0be826d6831339000000000000000000000000000000000000000");
            Assert.AreEqual((uint)0x00000001, newJob.mineParams.target0);
            Assert.AreEqual((uint)0x12e0be82, newJob.mineParams.target1);
            Assert.AreEqual((uint)0x6d683133, newJob.mineParams.target2);
            Assert.AreEqual((uint)0x90000000, newJob.mineParams.target3);
            Assert.AreEqual((uint)0x00000000, newJob.mineParams.target4);
            Assert.AreEqual((uint)0x00000000, newJob.mineParams.target5);
            Assert.AreEqual((uint)0x00000000, newJob.mineParams.target6);
            Assert.AreEqual((uint)0x00000000, newJob.mineParams.target7);
            Assert.AreEqual((UInt64)0x3b44b1407a096637, newJob.mineParams.header0);
            Assert.AreEqual((UInt64)0x4e353e519ede6177, newJob.mineParams.header1);
            Assert.AreEqual((UInt64)0x14e42ae6261f02d2, newJob.mineParams.header2);

        }

        [TestMethod]
        public void staticSwitch ()
        {
            Message.addLatestJob(null);
            System.Threading.Thread.Sleep(10000000);
        }


        [TestMethod]

        public void testGetTarget ()
        {

            string result = NewJob.getTargetFromDiff(0.9313083637607666);
            Assert.AreEqual("0000000112e0be826d683679ced02f1c135163199dea15eb2093520b59181ce2", result);

            result = NewJob.getTargetFromDiff_V1(0.9313083637607666M);
            Assert.AreEqual("0000000112e0be826d6831339000000000000000000000000000000000000000", result);


            result = NewJob.getTargetFromDiff_V1(20);
            Assert.AreEqual("000000000cccc000000000333300000000000000000000000000000000000000", result);


            /*
            result = NewJob.getTargetFromDiff_V1(6695826.2826M);
            Assert.AreEqual("00000000000002816E0000000000000000000000000000000000000000000000", result.Substring(0,17).PadRight(64,'0'));

            */
        }
    }
}
