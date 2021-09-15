using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xdxminer_lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;


namespace xdxminer_test
{
    [TestClass]
    public class GPUComputeTest
    {
        private static string seedHash_1 = "290DECD9548B62A8D60345A988386FC84BA6BC95484008F6362F93160EF3E563";



        [TestMethod]
        public void test_ligthCache ()
        {
            Ethash ethash = new Ethash();
            uint epoch = ethash.getEpochNumber(seedHash_1);

            //TODO
            DagItem[] lightCache = ethash.getLightCacheUsingEpochAndSeeHash(epoch,seedHash_1);
            Assert.AreEqual(lightCache[0].data[0], (uint) 0x7d45419e);
            Assert.AreEqual(lightCache[0].data[1], (uint)0xc9f23f82);
            Assert.AreEqual(lightCache.Length, 264179);
        }

        [TestMethod]
        public void test_generateDag()
        {
            Ethash ethash = new Ethash();
            uint epoch = ethash.getEpochNumber(seedHash_1);

            GPUCompute compute = new GPUCompute();
            compute.initComputeDevice();
            compute.generateDagDataInGPU(ethash.getLightCacheUsingEpochAndSeeHash(epoch, seedHash_1), ethash.getFullDataCacheBytesUsingSeedHash(seedHash_1));
            DagItem[][] dag = Enumerable.Range(0, 2).ToArray().Select( index => { return compute.getDag(index); }).ToArray();

            Assert.AreEqual(dag[0][0].data[0], (uint)3018019943);
            Assert.AreEqual(dag[0][0].data[1], (uint)2926838499);

            Assert.AreEqual(dag[0][1].data[0], (uint)1519522744);
            Assert.AreEqual(dag[0][1].data[1], (uint)3024848009);

          

        }


        [TestMethod]
        public void test_GPUCompute()
        {
            Ethash ethash = new Ethash();
            uint epoch = ethash.getEpochNumber(seedHash_1);

            DagItem[] lightCache = ethash.getLightCacheUsingEpochAndSeeHash(epoch, seedHash_1);
            ulong fullDatasetBytesCount = ethash.getFullDataCacheBytesUsingSeedHash(seedHash_1);
            //TODO
            GPUCompute compute = new GPUCompute();
            compute.initComputeDevice();
            compute.generateDagDataInGPU(lightCache, fullDatasetBytesCount);
            Stopwatch watch = new Stopwatch();
            compute.initComputeShader();
            watch.Start();
            watch.Stop();
            //MineResult result = compute.getMineResults();
            //DebugMsg[] debugMsgs = compute.getDebugData();
            // nounce = 0  result - >43 b8 9a 9d  e2
            //Console.WriteLine("Elapsed={0}", watch.Elapsed);
            //Console.WriteLine("Elapsed={0}", debugMsgs.Length);
            //Assert.AreEqual(result.dpad1, result.dpad1);
            compute.stop();
        }



        [TestMethod]
        public void testGPUComputeEpoch1Result()
        {
            Ethash ethash = new Ethash();
            uint epoch = ethash.getEpochNumber(seedHash_1);

            DagItem[] lightCache = ethash.getLightCacheUsingEpochAndSeeHash(epoch, seedHash_1);
            ulong count = ethash.getFullDataCacheBytesUsingSeedHash(seedHash_1);
            GPUCompute compute = new GPUCompute(1);
            compute.initComputeDevice();
            compute.generateDagDataInGPU(lightCache, count);
            compute.initComputeShader();

            MineParams mineParam_data = new MineParams
            {
                target0 = 0xFFFFFFFF,
                target1 = 0xFFFFFFFF,
                target2 = 0xFFFFFFFF,
                target3 = 0xFFFFFFFF,
                target4 = 0xFFFFFFFF,
                target5 = 0xFFFFFFFF,
                target6 = 0xFFFFFFFF,
                target7 = 0xFFFFFFFF,
                header0 = 0x00,
                header1 = 0x00,
                header2 = 0x00,
                header3 = 0x00,
                startNonce = 0X00,
                numDatasetElementsDivdeByTwo = 0,
                init = 0
            };

            compute.updateMineParams(mineParam_data);
            compute.flush();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            compute.compute(1);
            compute.flush();
            watch.Stop();
            MineResult result = compute.getMineResults();

            Assert.AreEqual(0xD0, result.hashResultByteAt(0));
            Assert.AreEqual(0x3A, result.hashResultByteAt(1));
            Assert.AreEqual(0x9B, result.hashResultByteAt(2));
            Assert.AreEqual(0xC8, result.hashResultByteAt(3));
            Assert.AreEqual(0x6E, result.hashResultByteAt(4));
            Assert.AreEqual(0xE2, result.hashResultByteAt(5));
            Assert.AreEqual(0x4C, result.hashResultByteAt(6));
            Assert.AreEqual(0x9A, result.hashResultByteAt(7));
            compute.stop();
        }


        [TestMethod]
        public void testGPUComputeEpoch429Result()
        {
            string epoch429Seedhash = "F30E991BF8CCC9F0D9805E8876BA3E62300C575C73EF4AE52D7013FBCF7571AF";
            Ethash ethash = new Ethash();
            uint epoch = ethash.getEpochNumber(epoch429Seedhash);

            DagItem[] lightCache = ethash.getLightCacheUsingEpochAndSeeHash(epoch, epoch429Seedhash);
            ulong count = ethash.getFullDataCacheBytesUsingSeedHash(epoch429Seedhash);
            GPUCompute compute = new GPUCompute(1);
            compute.initComputeDevice();
            compute.generateDagDataInGPU(lightCache, count);
            compute.initComputeShader();

            MineParams mineParam_data = new MineParams
            {
                target0 = 0xFFFFFFFF,
                target1 = 0xFFFFFFFF,
                target2 = 0xFFFFFFFF, 
                target3 = 0xFFFFFFFF,
                target4 = 0xFFFFFFFF,
                target5 = 0xFFFFFFFF,
                target6 = 0xFFFFFFFF,
                target7 = 0xFFFFFFFF,
                header0 = 0x00,
                header1 = 0x00,
                header2 = 0x00,
                header3 = 0x00,
                startNonce = 0X00,
                numDatasetElementsDivdeByTwo = 0,
                init = 0
            };

            compute.updateMineParams(mineParam_data);
            compute.flush();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            compute.compute(1);
            compute.flush();
            watch.Stop();
            MineResult result = compute.getMineResults();

            Assert.AreEqual(0xb1, result.hashResultByteAt(0));
            Assert.AreEqual(0x3e, result.hashResultByteAt(1));
            Assert.AreEqual(0xa1, result.hashResultByteAt(2));
            Assert.AreEqual(0x54, result.hashResultByteAt(3));
            Assert.AreEqual(0x1a, result.hashResultByteAt(4));
            Assert.AreEqual(0xad, result.hashResultByteAt(5));
            Assert.AreEqual(0x41, result.hashResultByteAt(6));
            Assert.AreEqual(0x22, result.hashResultByteAt(7));
            compute.stop();
        }

        [TestMethod]
        public void test_GPUCompute_proof_of_work()
        {
            StratumCommand stratumCommand = new StratumCommand();
            stratumCommand.parameters =
                new System.Collections.ArrayList {
                    "3766097a40b1443b7761de9e513e354ed2021f26e62ae414d6e8782a2a0483ea",
                    "3766097a40b1443b7761de9e513e354ed2021f26e62ae414d6e8782a2a0483ea",
                    "cd8cf77a2b4c6ed3472e1161861d300d052cc5f4793cb675c45215fea72bdcec",
                    "0000000fffff00000000ffff00000000ffff00000000ffff00000000ffff0000",
                    true};
            // 00000000_ffff0000_0000ffff_00000000_ffff0000_0000ffff_00000000_ffff0000",
            NewJob newJob = new NewJob(stratumCommand, Protocol.Stratum);
            MineParams mineParams = newJob.mineParams;


            Ethash ethash = new Ethash();
            uint epoch = ethash.getEpochNumber(seedHash_1);

            DagItem[] lightCache = ethash.getLightCacheUsingEpochAndSeeHash(epoch,seedHash_1);
            ulong count = ethash.getFullDataCacheBytesUsingSeedHash(seedHash_1);
            GPUCompute compute = new GPUCompute();
            compute.generateDagDataInGPU(lightCache, count);
            compute.initComputeShader();
            Stopwatch watch = new Stopwatch();
            compute.updateMineParams(mineParams);

            watch.Start();
            for (int i = 0; i < 10000000; i++) {
                compute.compute(1024);
                MineResult result = compute.getMineResults();
                mineParams.startNonce += 16* 1024;
                compute.updateMineParams(mineParams);
                if(result.count >0)
                {
                    watch.Stop();
                    Debug.WriteLine("result time used:" + (watch.ElapsedMilliseconds/1000));
                    Assert.IsTrue(i > 5661);
                    break;
                }
            }
            Console.WriteLine("Elapsed={0}", watch.Elapsed);
            compute.stop();

        }


        [TestMethod]
        public void test_GPUCompute_proof_of_work_8G()
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
            MineParams mineParams = newJob.mineParams;

            Ethash ethash = new Ethash();
            uint epoch = ethash.getEpochNumber(seedHash_1);

            DagItem[] lightCache = ethash.getLightCacheUsingEpochAndSeeHash(epoch, seedHash_1);
            ulong count = ethash.getFullDataCacheBytesUsingSeedHash(seedHash_1);
            GPUCompute compute = new GPUCompute();
            compute.generateDagDataInGPU(lightCache, count);
            compute.initComputeShader();
            Stopwatch watch = new Stopwatch();
            compute.updateMineParams(mineParams);

            watch.Start();
            for (int i = 0; i < 10000000; i++)
            {
                compute.compute(1024);
                MineResult result = compute.getMineResults();
                mineParams.startNonce += 16 * 1024;
                compute.updateMineParams(mineParams);
                if (result.count > 0)
                {
                    watch.Stop();
                    Debug.WriteLine("result time used:" + (watch.ElapsedMilliseconds / 1000));
                    Assert.IsTrue(i > 5661);
                    break;
                }
            }
            compute.stop();

        }
    }
}
