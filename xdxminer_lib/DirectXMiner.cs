using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.System.Profile;
using xdxminer_lib.util;


namespace xdxminer_lib
{
    public class DirectXMiner
    {
        Logger logger = new Logger("x");
        private readonly uint ETH_SEARCH_NUM_THREADS;
        private  readonly uint DISPATCH_NUM;
        private  bool isStopped = true;
        private GPUCompute compute;
        private uint currentEpoch = 0;
        private long hashRateCount;
        private Ethash ethash;
        private Stopwatch stopwatch = new Stopwatch();

        private NewJob currentJob;
        private MineParams mineParams;
        private Task gpuTask = null;
        public DirectXMiner()
        {
            string device = AnalyticsInfo.VersionInfo.DeviceFamily;
            if(device.ToLower().Contains("xbox"))
            {
                ETH_SEARCH_NUM_THREADS = 64;
                DISPATCH_NUM = 256;
            } else
            {
                ETH_SEARCH_NUM_THREADS = 512;
                DISPATCH_NUM = 1024;
            }
            ethash = new Ethash();
            compute = new GPUCompute(ETH_SEARCH_NUM_THREADS);
        }
        public void start()
        {
            isStopped = true;

            gpuTask?.Wait();

            isStopped = false;

            gpuTask =Task.Run(() => {
                loop();
            });


            stopwatch.Start();

        }

        public void stop()
        {
            isStopped = true;
            compute?.stop();
            gpuTask?.Wait();
        }


        private void loop()
        {
            uint epoch = uint.MaxValue;
            while (!isStopped)
            {
               
                NewJob newJob;
                if (Message.tryGetLatestJob(out newJob))
                {
                    logger.debug("new job " + newJob.jobId.PadRight(6, ' ').Substring(0, 5) + "...");
                    epoch = ethash.getEpochNumber(newJob.seedHash);
                    if (epoch != currentEpoch)
                    {
                        try
                        {
                            compute.stop();
                            compute.initComputeDevice();

                            ulong totalBytes = ethash.getFullDataCacheBytes(epoch);
                            logger.info("New Epoch found. Creating new dag for epoch " + epoch + ". Total bytes " + totalBytes);

                            compute.generateDagDataInGPU(ethash.getLightCacheUsingEpochAndSeeHash(epoch, newJob.seedHash), totalBytes);
                            System.Threading.Thread.Sleep(3000);
                            logger.info("dag generation completed.");
                            System.Threading.Thread.Sleep(3000);
                            compute.initComputeShader();
                            currentEpoch = epoch;
                        } catch (Exception ex)
                        {
                            logger.info("stopped.");
                        }
                    }
                    currentJob = newJob;
                    mineParams = currentJob.mineParams;
                    compute.updateMineParams(currentJob.mineParams);
                    computeLoop();
                }
                else if (currentJob != null)
                {
                    computeLoop();
                }
            }
        }


        private void computeLoop ()
        {


            if (stopwatch.ElapsedMilliseconds > 20000)
            {
                double hashrate = (hashRateCount * ((DISPATCH_NUM * 1.0) / 1000) * ETH_SEARCH_NUM_THREADS) / stopwatch.ElapsedMilliseconds;
                logger.info("hash rate {0:0.00} MH/s", hashrate);
                hashRateCount = 0;
                stopwatch.Restart();
            }

            for (int i = 0; i < 1; i++)
            {
                compute.compute(DISPATCH_NUM);
                mineParams.startNonce += DISPATCH_NUM * ETH_SEARCH_NUM_THREADS;
                compute.updateMineParams(mineParams);
                hashRateCount++;
            }
            MineResult result = compute.getMineResults();

            if (result.count >0)
            {
                for (int i=0; i < 1; i ++)
                {
                    Solution s = new Solution();
                    s.jobId = currentJob.jobId;
                    s.nounce = result.nonce[i];
                    s.mixHash = result.hashResult;
                    s.header = currentJob.headerHash;
                    Message.addSolution(s);
                }
                currentJob = null;
                compute.resetMineResultCount();
            }

        }
    }
}
