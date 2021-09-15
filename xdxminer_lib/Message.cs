using Deveel.Math;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using xdxminer_lib.util;
using BigInteger = System.Numerics.BigInteger;
using BigIntegerDeveel = Deveel.Math.BigInteger;

namespace xdxminer_lib
{


    public static class Message
    {
        static Logger logger = new Logger("msg");
        static NewJob latestJob;
        static readonly ConcurrentQueue<Solution> solutions = new ConcurrentQueue<Solution>();

        public static void addLatestJob (NewJob newJob)
        {
                Interlocked.Exchange(ref latestJob, newJob);      
        }


        public static bool tryGetLatestJob(out NewJob newJob)
        {
            newJob = null;

            if (latestJob == null)
            {
                return false;
            }

            Interlocked.Exchange(ref newJob, latestJob);
            Interlocked.Exchange(ref latestJob, null);

            return true;
        }

        public static void addSolution (Solution solution)
        {          
            solutions.Enqueue(solution);
        }

        public static bool tryGetSolution (out Solution solution)
        {
            return solutions.TryDequeue(out solution);
        }
    }

    public struct ConnectToStratrum
    {
        public string server;
        public int port;
        public string username;
        public string password;
    }

    public struct MiningResult
    {

    }

    public struct Solution
    {
        public string jobId;
        public ulong nounce;
        public byte[] mixHash;
        public string header;
    }

    public class NewJob
    {
        private static string DOUBLE_FORMAT = "0." + new string('#', 339);
        private static BigInteger TARGET_BASE =  BigInteger.Parse("00000000ffff0000000000000000000000000000000000000000000000000000", System.Globalization.NumberStyles.HexNumber);
        private static BigIntegerDeveel bigIntegerDeveel = BigIntegerDeveel.Parse("00000000ffff0000000000000000000000000000000000000000000000000000", 16);
        private static BigDecimal TARGET_BASE_D = new BigDecimal(bigIntegerDeveel);

        public MineParams mineParams {  get; }
        public static Random rand = new Random();
        public string jobId { get; }
        public string headerHash { get;}
        public string seedHash { get;}
        public string target { get; }
        public NewJob(StratumCommand stratumCommand, Protocol protocol, string difficulty = null, ulong extraNounce = 0)
        {

            object[] arr = stratumCommand.parameters.ToArray();

            jobId = (string)arr[0];


            switch (protocol)
            {
                case Protocol.Stratum:
                    headerHash = (string)arr[1];
                    seedHash = (string)arr[2];
                    target = (string)arr[3];
                    break;
                case Protocol.EthereumStratum_1:
                    seedHash = (string)arr[1];
                    headerHash = (string)arr[2];
                    target = difficulty;
                    break;
            }


            byte[] headerBytes = StringToByteArray(headerHash);
            byte[] targtBytes = StringToByteArray(target);
            byte[] data = new byte[Marshal.SizeOf(typeof(MineParams))];
            Array.Copy(targtBytes, data, targtBytes.Length);
            Array.Copy(headerBytes, 0,data,targtBytes.Length, headerBytes.Length);
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            MineParams temp = Marshal.PtrToStructure<MineParams>(handle.AddrOfPinnedObject());
            handle.Free();
            
            temp.target0 = SwapBytes(temp.target0);
            temp.target1 = SwapBytes(temp.target1);
            temp.target2 = SwapBytes(temp.target2);
            temp.target3 = SwapBytes(temp.target3);
            temp.target4 = SwapBytes(temp.target4);
            temp.target5 = SwapBytes(temp.target5);
            temp.target6 = SwapBytes(temp.target6);
            temp.target7 = SwapBytes(temp.target7);

            if (protocol == Protocol.EthereumStratum_1)
            {
                temp.startNonce = extraNounce;
            } else
            {
                //temp.startNonce = 0UL;
                temp.startNonce = ulongRandom();
            }

            this.mineParams = temp;
        }

     

        ulong ulongRandom()
        {
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            return BitConverter.ToUInt64(buf, 0);
        }
        public static string getTargetFromDiff(double diff)
        {
            BigDecimal bigDecimal = new BigDecimal(diff);
            BigDecimal result =BigMath.Divide(TARGET_BASE_D, bigDecimal, RoundingMode.HalfUp);
            string resultStr = result.ToBigInteger().ToString(16).PadLeft(64,'0');
            return resultStr;
        }
        public static string getTargetFromDiff_V1 (decimal diffDe)
        {
            double diff = Convert.ToDouble(diffDe);
            string result= "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";
            BigInteger product;
            if (diff == 0)
            {
                result = "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";
            } else
            {
                diff = 1 / diff;

                BigInteger idiff = new BigInteger(diff);
                product = TARGET_BASE*idiff;
                string sdiff = new BigDecimal(diff).ToString();
                if(sdiff.ToLower().Contains("e"))
                {
                    sdiff = diff.ToString(DOUBLE_FORMAT);
                }
                int ldiff = sdiff.Length;
                int offset = sdiff.IndexOf('.');
                if(offset >=0)
                {
                    int precision = (ldiff - 1) - offset;

                    // Effective sequence of decimal places
                    //https://github.com/ethereum-mining/ethminer/blob/ce52c74021b6fbaaddea3c3c52f64f24e39ea3e9/libdevcore/CommonData.cpp
                    string decimals = sdiff.Substring(offset + 1);
                    decimals = decimals.Remove(0, FindFirstNotOf(decimals, "0"));

                    string decimalDivisor = "1";
                    decimalDivisor = decimalDivisor.PadRight(precision + 1, '0');
                    BigInteger multiplier = BigInteger.Parse(decimals);
                    BigInteger divisor = BigInteger.Parse(decimalDivisor);
                    BigInteger decimalproduct;
                    decimalproduct = TARGET_BASE * multiplier;
                    decimalproduct /= divisor;
                    product += decimalproduct;
                    result = product.ToString("x");
                }
            }
            result = result.PadLeft(64, '0');

            return result;

        }

        public static int FindFirstNotOf(string source, string chars)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (chars == null) throw new ArgumentNullException("chars");
            if (source.Length == 0) return -1;
            if (chars.Length == 0) return 0;

            for (int i = 0; i < source.Length; i++)
            {
                if (chars.IndexOf(source[i]) == -1) return i;
            }
            return -1;
        }

        public static uint SwapBytes(uint x)
        {
            return ((x & 0x000000ff) << 24) +
                   ((x & 0x0000ff00) << 8) +
                   ((x & 0x00ff0000) >> 8) +
                   ((x & 0xff000000) >> 24);
        }

        public static byte[] StringToByteArray(string hex)
        {
            hex = hex.ToLower().Replace("0x", "");
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static ulong StringToULong(String hex)
        {
            return BitConverter.ToUInt64(StringToByteArray(hex), 0);
        }

    }

    public struct NewDifficult
    {
    }

}
