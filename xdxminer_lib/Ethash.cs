using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Linq;
using xdxminer_lib.util;

namespace xdxminer_lib { 

    public class Ethash
    {
        Logger logger = new Logger("ethash_etc");
        private static ulong DATASET_BYTES_INIT = 1073741824; //2 ^ 30;   //      # bytes in dataset at genesis
        private static ulong DATASET_BYTES_GROWTH = 8388608; //2 ^ 23;   //   # dataset growth per epoch
        private static uint CACHE_BYTES_INIT = 16777216; // 2 ^ 24;      // # bytes in cache at genesis
        private static uint CACHE_BYTES_GROWTH = 131072;  // 2^17     //# cache growth per epoch
        private static ulong MIX_BYTES = 128;         //# width of mix
        private static uint HASH_BYTES = 64;          //# hash length in bytes
        private static uint CACHE_ROUNDS = 3;          //# number of rounds in cache production
        private uint epochFactor = 1;

        private Dictionary<string, Tuple<byte[], uint>> seedHashTable =new Dictionary<string, Tuple<byte[], uint>>();

        public Ethash()
        {
            switch (settings.Coin.selectedCoin.coinType) {
                case settings.CoinType.ETC:
                    epochFactor = 2;
                    break;
                case settings.CoinType.ETH:
                    epochFactor = 1;
                    break;
              
            }
            initSeedHash();
        }


        protected void initSeedHash()
        {

            foreach (uint i in Enumerable.Range(1, 10).Concat(Enumerable.Range(420, 600)))
            {
                byte[] seedHash = getSeedHash(i);
                string hash = BitConverter.ToString(seedHash).Replace("-", "").ToLower();
                Tuple<byte[], uint> t = new Tuple<byte[], uint>(seedHash, i/epochFactor);
                seedHashTable.Add(hash, t);
            }
        }

        private bool isprime(ulong number) {
            if (number < 2) return false;
            if (number % 2 == 0) return (number == 2);
            ulong root = (ulong)Math.Sqrt((double)number);
            for (ulong i = 3; i <= root; i += 2)
            {
                if (number % i == 0) return false;
            }
            return true;
        }

        public DagItem[] getLightCacheUsingEpochAndSeeHash(uint epoch,string seedHashStr)
        {
            var result = getEpochTuple(seedHashStr);
            byte[] seedhash =result.Item1;
            return getLightCache(epoch, seedhash);
        }

        private Tuple<byte[],uint> getEpochTuple(string seedHashSstr)
        {
            try
            {
                seedHashSstr = seedHashSstr.Trim().ToLower().Replace("0x", "");
                Tuple<byte[], uint> result;
                if (seedHashTable.TryGetValue(seedHashSstr.ToLower(), out result))
                {
                    //var newTuple = new Tuple<byte[],uint>(result.Item1, 1);
                    return result;
                }
                else
                {
                    logAndExit();
                }
            } catch (Exception)
            {
                logAndExit();
            }
            return null;
        }

        private void logAndExit ()
        {
            logger.error("epoch not supported, please upgrade to new version");
            System.Threading.Thread.Sleep(30000);
            logger.error("exiting now");
            System.Threading.Thread.Sleep(3000);
            Environment.Exit(-1);
        }

        private DagItem[] getLightCache(uint epochNumber , byte[] seedHash)
        {
            uint lightCacheSize = getCacheSizeInBytes(epochNumber);
            byte[] [] lightCache  = createCacheBytes(lightCacheSize, seedHash);
            return getDagItemFromLightCacheBytes(lightCache);
        }

        private DagItem[] getDagItemFromLightCacheBytes(byte[][] lightCache)
        {
            DagItem[] dagItem = new DagItem[lightCache.Length];
            for (uint i = 0; i < lightCache.Length; i++)
            {
                var cacheItem = lightCache[i];
                dagItem[i] = new DagItem
                {
                    data = new uint[] {
                        getUintfromByte(cacheItem, 0),
                        getUintfromByte(cacheItem, 1),
                        getUintfromByte(cacheItem, 2),
                        getUintfromByte(cacheItem, 3),
                        getUintfromByte(cacheItem, 4),
                        getUintfromByte(cacheItem, 5),
                        getUintfromByte(cacheItem, 6),
                        getUintfromByte(cacheItem, 7),
                        getUintfromByte(cacheItem, 8),
                        getUintfromByte(cacheItem, 9),
                        getUintfromByte(cacheItem, 10),
                        getUintfromByte(cacheItem, 11),
                        getUintfromByte(cacheItem, 12),
                        getUintfromByte(cacheItem, 13),
                        getUintfromByte(cacheItem, 14),
                        getUintfromByte(cacheItem, 15)

                    }

                };
            }
            return dagItem;
        }

        private byte[][] createCacheBytes(uint cache_size , byte[] seedHash) {

            uint n = (uint)(cache_size / HASH_BYTES);
            byte[][] o = new byte[n][];
            o[0] = sha3_512(seedHash);
            for(uint i =1; i < n;i++)
            {
                o[i] = sha3_512(o[i - 1]);
            }
            for (uint cacheRound =0; cacheRound < CACHE_ROUNDS; cacheRound++)
            {
                for (uint i=0;i< n;i++)
                {
                    // first index
                    uint v = getUintfromByte(o[i]) % n;

                    //second index
                    uint w = (n + i - 1) % n;

                    var item1 = o[v];
                    var item2 = o[w];
                    var result = exclusiveOR(item1, item2);
                    o[i] = sha3_512(result);
                }
            }
            return o;

            
        }

        public static byte[] exclusiveOR(byte[] key, byte[] PAN)
        {
            if (key.Length == PAN.Length)
            {
                byte[] result = new byte[key.Length];
                for (int i = 0; i < key.Length; i++)
                {
                    result[i] = (byte)(key[i] ^ PAN[i]);
                }
                return result;
            }
            else
            {
                throw new ArgumentException();
            }
        }


        private uint getUintfromByte (byte [] input ,int wordOffset = 0 )
        {
            byte[] bytes = new byte[4];
            Buffer.BlockCopy(input, wordOffset * 4, bytes, 0, 4);

            // If the system architecture is not little-endian (that is, little end first),
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            uint i = BitConverter.ToUInt32(bytes, 0);
            return i;

        }


        private uint getCacheSizeInBytes(uint epochNumber)
        {
            uint sz = CACHE_BYTES_INIT + CACHE_BYTES_GROWTH * epochNumber;
            sz -= HASH_BYTES;
            while (!isprime(sz / HASH_BYTES))
            {
                sz = sz - (2 * HASH_BYTES);
            }
            return sz;
        }

        private byte[] getSeedHash ( uint epochNumber)
        {

            var digest = new KeccakDigest(256);
            
            byte[] s = new byte[32];
            for (int i=0; i < epochNumber; i ++)
            {
                s = sha3_256(s);
            }
            
            return s;
        }

        private byte [] sha3_256 (byte[] input)
        {
            byte[] output = new byte[32];
            var digest = new KeccakDigest(256);
            digest.BlockUpdate(input, 0, input.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        private byte[] sha3_512(byte[] input)
        {
            byte[] output = new byte[64];
            var digest = new KeccakDigest(512);
            digest.BlockUpdate(input, 0, input.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        public uint getEpochNumber(string seedHash)
        {
            return getEpochTuple(seedHash).Item2;
        }

        public ulong getFullDataCacheBytesUsingSeedHash(string seedhash)
        {
            return getFullDataCacheBytes(getEpochTuple(seedhash).Item2);
        }

        public ulong getFullDataCacheBytes(uint epochNumber)
        {
            ulong sz = DATASET_BYTES_INIT + DATASET_BYTES_GROWTH * (ulong)epochNumber;
            sz -= MIX_BYTES;
            while (!isprime(sz / MIX_BYTES))
            {
                sz -= 2 * MIX_BYTES;
            }
            return sz;
        }

    }
}
