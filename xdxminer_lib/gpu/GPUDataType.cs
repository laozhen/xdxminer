using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace xdxminer_lib
{

    [StructLayout(LayoutKind.Sequential,Pack =1)]
    public struct Uint4
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] x;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Uint2
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] x;
    }

    //512 bit and 64 bytes for Uint 512 

    [StructLayout(LayoutKind.Sequential)]
    public struct DagItem
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public uint[] data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DagParts
    {
        public uint part1;
        public uint part2;
        public uint part3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Params
    {
        public uint numDatasetElements;
        public uint numCacheElements;
        public uint datasetGenerationOffset;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct MineParams
    {
        public uint target0;
        public uint target1;
        public uint target2;
        public uint target3;
        public uint target4;
        public uint target5;
        public uint target6;
        public uint target7;

        public UInt64 header0;
        public UInt64 header1;
        public UInt64 header2;
        public UInt64 header3;

        public ulong startNonce;
        public uint numDatasetElementsDivdeByTwo;
        public uint init;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MineResult
    {
        public uint count;    
        public uint dpad1;     //4 
        public uint dpad2;
        public uint dpad3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public UInt64[] nonce; 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] hashResult;

        public byte hashResultByteAt (int index)
        {
            return hashResult[index];
        }

    };


    [StructLayout(LayoutKind.Sequential)]
    public struct DebugMsg
    {
        public uint parentIndex;
        public uint fnvSecondMix;
    };

    class GPUDataType
    {
    }
}
