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
    public class EthashTest
    {
        [TestMethod]
        public void test()
        {
            Ethash ethash = new Ethash();
            System.Console.Out.WriteLine("running test now");
            //Assert.That("123123", Is.EqualTo("Valueyouexpect it to return"));

        }

        [TestMethod]
        public void getFullDataCacheItemCount()
        {

            Ethash ethash = new Ethash();
            Assert.AreEqual((ulong)1082130304, ethash.getFullDataCacheBytesUsingSeedHash("290DECD9548B62A8D60345A988386FC84BA6BC95484008F6362F93160EF3E563"));
            Assert.AreEqual((ulong)1090514816, ethash.getFullDataCacheBytesUsingSeedHash("510E4E770828DDBF7F7B00AB00A9F6ADAF81C0DC9CC85F1F8249C256942D61D9"));
            Assert.AreEqual((ulong)1098906752, ethash.getFullDataCacheBytesUsingSeedHash("356E5A2CC1EBA076E650AC7473FCCC37952B46BC2E419A200CEC0C451DCE2336"));
            Assert.AreEqual((ulong)4613734016, ethash.getFullDataCacheBytesUsingSeedHash("01133cf3aa688c45d3cca045da5f1b1d8fb7e322cb40bd72011a03fc32780030"));
        }

        [TestMethod]
        public void initSeedHash()
        {

            Ethash ethash = new Ethash();
            //ethash.getLightCacheUsingEpochAndSeeHash("290DECD9548B62A8D60345A988386FC84BA6BC95484008F6362F93160EF3E563");
            //ethash.getLightCacheUsingEpochAndSeeHash("510E4E770828DDBF7F7B00AB00A9F6ADAF81C0DC9CC85F1F8249C256942D61D9");
            //ethash.getLightCacheUsingEpochAndSeeHash("356E5A2CC1EBA076E650AC7473FCCC37952B46BC2E419A200CEC0C451DCE2336");
        }

        [TestMethod]
        public void epochTuple()
        {

            Ethash ethash = new Ethash();
            //var t = ethash.getEpochTuple("cd8cf77a2b4c6ed3472e1161861d300d052cc5f4793cb675c45215fea72bdcec");
            //Assert.AreEqual(0xCD, t.Item1[0]);
            //Assert.AreEqual((uint)421, t.Item2);
        }


    }
}
