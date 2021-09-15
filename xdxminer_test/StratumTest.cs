using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Net.Security;
using xdxminer_lib;
using System.IO;

namespace xdxminer_test
{
    public class StratumToTest : Stratum
    {
        private SslStream sslStream1;
        public void setSslStream (SslStream sslStream)
        {
            this.sslStream1 = sslStream;
        }

        override protected Stream createNetworkStream ()
        {
            return sslStream1;
        }
    }


    [TestClass]
    public class StratumTest
    {

        [TestMethod]
        public void testConnect()
        {
            Stratum clientStratum = new Stratum();
            clientStratum.setDetails("eu1.ethermine.org", 5555, "0x159fc14f2d6464891cdb8f70d042cc58815b21b9",false);
            clientStratum.start();
            System.Threading.Thread.Sleep(300000);
            NewJob newJob;
            Message.tryGetLatestJob(out newJob);
        }


        [TestMethod]
        public void testConnectSuccess()
        {
            Stratum clientStratum = new Stratum();
            clientStratum.setDetails("eu1.ethermine.org", 5555, "0x159fc14f2d6464891cdb8f70d042cc58815b21b9",false);
            clientStratum.start();
            System.Threading.Thread.Sleep(3000);
            NewJob newJob;
            Message.tryGetLatestJob(out newJob);
        }


        [TestMethod]
        public void testConnectStartStop()
        {
            Stratum clientStratum = new Stratum();
            clientStratum.setDetails("eu1.ethermine.org", 5555, "0x159fc14f2d6464891cdb8f70d042cc58815b21b9",false);
            clientStratum.start();
            System.Threading.Thread.Sleep(3000);
            NewJob newJob;
            Message.tryGetLatestJob(out newJob);
            clientStratum.stop();
            System.Threading.Thread.Sleep(3000);
            Message.tryGetLatestJob(out newJob);
            NewJob lastJob;
            Message.tryGetLatestJob(out lastJob);
            Assert.IsTrue(lastJob == null);

        }

        [TestMethod]
        public void testConnectError()
        {
            var mockSSLStream = new Moq.Mock<SslStream>();
            mockSSLStream.Setup(stream => stream.ReadByte()).Returns(0);
            Stratum clientStratum = new Stratum();
            clientStratum.setDetails("eth.f2pool.com", 5555, "0x159fc14f2d6464891cdb8f70d042cc58815b21b9" ,false);
            clientStratum.start();
            System.Threading.Thread.Sleep(3000);
            NewJob newJob;
            Message.tryGetLatestJob(out newJob);
            clientStratum.stop();
            System.Threading.Thread.Sleep(3000);
            Message.tryGetLatestJob(out newJob);
            NewJob lastJob;
            Message.tryGetLatestJob(out lastJob);
            Assert.IsTrue(lastJob == null);

        }

        [TestMethod]
        public void testConnect_f2Pool()
        {
            var mockSSLStream = new Moq.Mock<SslStream>();
            mockSSLStream.Setup(stream => stream.ReadByte()).Returns(0);
            Stratum clientStratum = new Stratum();
            clientStratum.setDetails("eth.f2pool.com", 6688, "zleth123.xwaveminer", false,"123");
            clientStratum.start();
            System.Threading.Thread.Sleep(3000000);
            NewJob newJob;
            Message.tryGetLatestJob(out newJob);
            clientStratum.stop();
            System.Threading.Thread.Sleep(3000);
            Message.tryGetLatestJob(out newJob);
            NewJob lastJob;
            Message.tryGetLatestJob(out lastJob);
            Assert.IsTrue(lastJob == null);

        }


        [TestMethod]
        public void testConnect_flexpool()
        {
            var mockSSLStream = new Moq.Mock<SslStream>();
            mockSSLStream.Setup(stream => stream.ReadByte()).Returns(0);
            Stratum clientStratum = new Stratum();
            clientStratum.setDetails("eth-us-east.flexpool.io", 5555, "0x6ebfa411F54272C4F5c696e0E98a3Ed45Ae2b265", true, "123");
            clientStratum.start();
            System.Threading.Thread.Sleep(3000000);
            NewJob newJob;
            Message.tryGetLatestJob(out newJob);
            clientStratum.stop();
            System.Threading.Thread.Sleep(3000);
            Message.tryGetLatestJob(out newJob);
            NewJob lastJob;
            Message.tryGetLatestJob(out lastJob);
            Assert.IsTrue(lastJob == null);

        }


        [TestMethod]
        public void testConnect_sparkpool()
        {
            var mockSSLStream = new Moq.Mock<SslStream>();
            mockSSLStream.Setup(stream => stream.ReadByte()).Returns(0);
            Stratum clientStratum = new Stratum();
            clientStratum.setDetails("eth-eu.sparkpool.com", 3333, "0x6ebfa411F54272C4F5c696e0E98a3Ed45Ae2b265", false, null);
            clientStratum.start();
            System.Threading.Thread.Sleep(3000);
            NewJob newJob;
            Message.tryGetLatestJob(out newJob);
            clientStratum.stop();
            System.Threading.Thread.Sleep(3000);
            Message.tryGetLatestJob(out newJob);
            NewJob lastJob;
            Message.tryGetLatestJob(out lastJob);
            Assert.IsTrue(lastJob == null);

        }

        [TestMethod]
        public void testNounceToHexString()
        {
            Solution solution = new Solution();
            solution.nounce = ulong.MaxValue-1;
            string result = string.Format("{0:X16}", solution.nounce).ToLower();
            Debug.WriteLine("result is " + result);

        }

        [TestMethod]
        public void testStratumFormat ()
        {
            Solution solution = new Solution();
            solution.mixHash = new byte[32];
            solution.mixHash[2] = 0x3e;
            string output = StratumUtil.ByteArrayToHexString(solution.mixHash);
            string CurrentString = "{\"id\":3,\"jsonrpc\": \"2.0\",\"result\":false,\"error\":\"Miner provided invalid solution package!\"}";
            StratumResponse Response = StratumUtil.JsonDeserialize<StratumResponse>(CurrentString);

        }
    }
}
