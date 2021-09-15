using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using xdxminer_lib.util;
using xdxminer_lib.settings;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace xdxminer_lib
{


    public class GPUCompute
    {
        private bool isStopped = false;

        private Logger logger = new Logger("GPU");
        private Device device;
        private DeviceContext context;

        private StructuredBuffer<DagItem>[] dag =new StructuredBuffer<DagItem>[] { };

        private StructuredBuffer<DagItem> lightCacheGPU;
        private StructuredBuffer<MineResult> mineResult;
        private StructuredBuffer<DebugMsg> debugData;
        Buffer mineResultOut;
        Buffer bufferOut { get; set; }
        ConstantsBuffer<Params> parameter;
        ConstantsBuffer<MineParams> mineParam;
        bool codeCompiled = false;
        private DagItem[] lightCache;

        private uint fullDatasetDagItemCount;
        private static uint ADDITIONAL_ITEM_COUNT = 2;

        private static uint PART_1_COUNT = 20537500;
        private static uint PART_2_COUNT = 20537500;
        private static uint PART_3_COUNT = 20537500;
        private static uint PART_4_COUNT = 20537500;
        
        private uint dispatchX = 1;
        private uint dispatchY = 1;


        private ShaderMacro MACRO_PART_1_COUNT;
        private ShaderMacro MACRO_PART_2_COUNT;
        private ShaderMacro MACRO_PART_3_COUNT;
        private ShaderMacro MACRO_PART_4_COUNT;
        private ShaderMacro SEARCH_NUM_THREADS_X;
        private ShaderMacro SEARCH_NUM_THREADS_Y;
        

        private static int GENERATE_FULL_DATASET_NUM_THREADS_X = 64;
        ShaderBytecode mineBytecode;
        ComputeShader mineShader;
        ShaderBytecode genDagByteCode;
        ComputeShader genDagShader;

        Params parameter_data;
        MineParams mineParam_data;
        MineResult mineResult_data;
        class MyInclude : Include
        {
            IDisposable disposable;

            public IDisposable Shadow { get => disposable; set => this.disposable= value; }

            public void Close(Stream stream)
            {
                stream.Close();
            }

            public void Dispose()
            {
                disposable.Dispose();
            }
            public Stream Open(IncludeType type, string fileName, Stream parentStream)
            {
                Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(@"xdxminer_lib." + fileName);
                disposable = s;
                return s;

            }
        }

        // init device
        public GPUCompute(uint dispatchX = 32)
        {

            this.dispatchX = dispatchX;
            switch (Coin.selectedCoin.coinType)
            {
                case CoinType.ETC:
                    PART_1_COUNT = PART_2_COUNT = PART_3_COUNT = PART_4_COUNT = 15007500;
                    break;
                case CoinType.ETH:
                    PART_1_COUNT = PART_2_COUNT = PART_3_COUNT = PART_4_COUNT = 20007500;
                    break;
            }

            SEARCH_NUM_THREADS_X = new ShaderMacro { Name = "SEARCH_NUM_THREADS_X_VALUE", Definition = (this.dispatchX).ToString() };

            MACRO_PART_1_COUNT = new ShaderMacro { Name = "PART_1_VALUE", Definition = PART_1_COUNT.ToString() };
            MACRO_PART_2_COUNT = new ShaderMacro { Name = "PART_2_VALUE", Definition = (PART_1_COUNT + PART_2_COUNT).ToString() };
            MACRO_PART_3_COUNT = new ShaderMacro { Name = "PART_3_VALUE", Definition = (PART_1_COUNT + PART_2_COUNT + PART_3_COUNT).ToString() };
            MACRO_PART_4_COUNT = new ShaderMacro { Name = "PART_4_VALUE", Definition = (PART_1_COUNT + PART_2_COUNT + PART_3_COUNT + PART_4_COUNT).ToString() };

        mineResult_data = new MineResult
            {
                count = 0,
                dpad1 = 999,
                dpad2 = 999,
                dpad3 = 999,
                nonce = new UInt64[6],
                hashResult = new byte[32]
            };


        }

        public void initComputeDevice()
        {

            this.device = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.DisableGpuTimeout);
            device.DebugName = "The Device";
            this.context = device.ImmediateContext;
            logger.info("DirectX 11, feature level " + device.FeatureLevel);
            if (!codeCompiled)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(@"xdxminer_lib.ethdag.hlsl"))
                using (var reader = new StreamReader(stream))
                {
                    string value = reader.ReadToEnd();
                    genDagByteCode = new ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.Compile(
                        value, "main", "cs_5_0", ShaderFlags.None, EffectFlags.None, defines: new ShaderMacro[] { 
                            MACRO_PART_1_COUNT, 
                            MACRO_PART_2_COUNT, 
                            MACRO_PART_3_COUNT, 
                            MACRO_PART_4_COUNT,
                            SEARCH_NUM_THREADS_X,
                            SEARCH_NUM_THREADS_Y}, include: new MyInclude()));
                }

                logger.info("Compiling DirectX shader");
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(@"xdxminer_lib.ethsearch.hlsl"))
                using (var reader = new StreamReader(stream))
                {
                    string value = reader.ReadToEnd();
                    mineBytecode = new ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.Compile(value, "main", "cs_5_0",
                        ShaderFlags.None, EffectFlags.None, defines: new ShaderMacro[] { 
                            MACRO_PART_1_COUNT,
                            MACRO_PART_2_COUNT, 
                            MACRO_PART_3_COUNT, 
                            MACRO_PART_4_COUNT,
                            SEARCH_NUM_THREADS_X,
                            SEARCH_NUM_THREADS_Y}, include: new MyInclude()));

                }


                logger.info("DirectX shader compiled."); 
                codeCompiled = true;
            }
            genDagShader = new ComputeShader(device, genDagByteCode);
            mineShader = new ComputeShader(device, mineBytecode);

        }

        // create dag
        private void initEpochInfo(DagItem[] lightCache, ulong fullDatasetBytes)
        {

            this.lightCache = lightCache;
            this.fullDatasetDagItemCount = (uint)(fullDatasetBytes / 64);  // one item is 64 bytes

            createBuffer();

            initData();
            resetMineResultCount();
        }

        private unsafe void createBuffer()
        {
             dag = new StructuredBuffer<DagItem>[4];
            context.Flush();
            logger.info("creating buffer in gpu.");
            dag[0] = new StructuredBuffer<DagItem>(device, (int)(PART_1_COUNT + ADDITIONAL_ITEM_COUNT), "dag1");
            context.Flush();
            System.Threading.Thread.Sleep(200);
            dag[1] = new StructuredBuffer<DagItem>(device, (int)(PART_2_COUNT + ADDITIONAL_ITEM_COUNT), "dag2");
            context.Flush();
            System.Threading.Thread.Sleep(200);
            dag[2] = new StructuredBuffer<DagItem>(device, (int)(PART_3_COUNT + ADDITIONAL_ITEM_COUNT), "dag3");
            context.Flush();
            System.Threading.Thread.Sleep(200);
            dag[3] = new StructuredBuffer<DagItem>(device, (int)(PART_4_COUNT + ADDITIONAL_ITEM_COUNT), "dag4");
            context.Flush();
            System.Threading.Thread.Sleep(1000);

            mineResult = new StructuredBuffer<MineResult>(device, 1, "mineResult");
            mineResultOut = mineResult.CreateStaging();

            //TODO use dynamic value 1130471 is dag at 4Gbytes
            lightCacheGPU = new StructuredBuffer<DagItem>(device, 1130471*2, "lightCache");

            mineParam = new ConstantsBuffer<MineParams>(device);
            parameter = new ConstantsBuffer<Params>(device);
            logger.info("finish creating graphic buffer");
            context.Flush();
            System.Threading.Thread.Sleep(1000);

        }

        private void initData()
        {
            lightCacheGPU.Update(context, lightCache);


            parameter_data = new Params
            {
                datasetGenerationOffset = 0,
                numDatasetElements = fullDatasetDagItemCount,
                numCacheElements = (uint)lightCache.Length
            };

            parameter.Update(context, ref parameter_data);


            mineParam_data = new MineParams
            {
                target0 = 0x00000000, target1 = 0x00FFFFFF,
                target2 = 0xFFFFFFFF, target3 = 0xFFFFFFFF,
                target4 = 0xFFFFFFFF, target5 = 0xFFFFFFFF,
                target6 = 0xFFFFFFFF, target7 = 0xFFFFFFFF,
                header0 = 0x00, header1 = 0x00,
                header2 = 0x00, header3 = 0x00,
                startNonce = 0X00,
                numDatasetElementsDivdeByTwo = fullDatasetDagItemCount / 2,
                init = 0
            };

            mineParam.Update(context, ref mineParam_data);


        }

        public void generateDagDataInGPU(DagItem[] lightCache, ulong fullDatasetBytes)
        {
            isStopped = false;
            initEpochInfo(lightCache, fullDatasetBytes);
       
            context.ComputeShader.Set(genDagShader);
            context.ComputeShader.SetUnorderedAccessViews(0, dag.Select(d => d.UAV).ToArray());
            context.ComputeShader.SetUnorderedAccessView(4, lightCacheGPU.UAV);
            context.ComputeShader.SetConstantBuffer(1, parameter.Buffer);
            int i = 0;


            for (   i = 0; !isStopped && (i * 256 * GENERATE_FULL_DATASET_NUM_THREADS_X <= fullDatasetDagItemCount + 1); i++)
            {
                context.Dispatch(256, 1, 1);
                context.Flush();
                parameter_data.datasetGenerationOffset = (uint)(i * 256 * GENERATE_FULL_DATASET_NUM_THREADS_X);
                parameter.Update(context, ref parameter_data);
                if(i%1000==0)
                {
                    System.Threading.Thread.Sleep(1000);
                    logger.info("creating DAG step {0}/{1} " , i * 256 * GENERATE_FULL_DATASET_NUM_THREADS_X, fullDatasetDagItemCount);
                }
            }
            System.Threading.Thread.Sleep(3000);
            

            logger.info("DAG creation completed.", i);

        }
        public void initComputeShader()
        {
            context.Flush();
            context.ComputeShader.Set(null);
            context.ComputeShader.SetUnorderedAccessView(0, null);
            context.ComputeShader.SetUnorderedAccessView(1, null);
            context.ComputeShader.SetUnorderedAccessView(2, null);
            context.ComputeShader.SetUnorderedAccessView(3, null);
            context.ComputeShader.SetConstantBuffer(0, null);
            context.ComputeShader.SetConstantBuffer(1, null);
            context.Flush();
            context.ComputeShader.Set(mineShader);
            context.ComputeShader.SetShaderResources(0, dag.Select(da => da.SRV).ToArray());

            context.ComputeShader.SetUnorderedAccessView(4, mineResult.UAV);
            context.ComputeShader.SetConstantBuffer(1, mineParam.Buffer);
            context.Flush();
        }
        public void resetMineResultCount()
        {
            mineResult_data.count = 0;
            mineResult.Update(context, new MineResult[] { mineResult_data });

        }

        public void compute (uint steps)
        {
            context.Dispatch((int)steps, 1, 1);
        }

        public void flush()
        {
            context.Flush();
        }


        public void updateMineParams (MineParams mineParamsInput)
        {
            mineParamsInput.numDatasetElementsDivdeByTwo = fullDatasetDagItemCount / 2;
            mineParam.Update(context, ref mineParamsInput);
        }

        public void stop()
        {
            isStopped = true;
            mineShader?.Dispose();
            genDagShader?.Dispose();
            dag.ToList().ForEach( da => da.Dispose());
            mineResult?.Dispose();
            mineResultOut?.Dispose();
            debugData?.Dispose();
            //TODO use dynamic value 1130471 is dag at 4Gbytes
            lightCacheGPU?.Dispose();

            mineParam?.Dispose();
            parameter?.Dispose();
            device?.Dispose();
            context?.Dispose();

        }

        ~GPUCompute()
        {
            stop();
        }

        public MineResult getMineResults()
        {
            context.CopyResource(mineResult.Buffer, mineResultOut);
            DataStream stream;
            DataBox box = context.MapSubresource(mineResultOut, 0, MapMode.Read, MapFlags.None, out stream);
            IntPtr ptr = box.DataPointer;
            MineResult result = Marshal.PtrToStructure<MineResult>(ptr);
            context.UnmapSubresource(mineResultOut, 0);
            return result;

        }

        public DagItem[] getDag(int index)
        {
            StructuredBuffer<DagItem> structuredBuffer = dag[index];
            bufferOut = structuredBuffer.CreateStaging();

            // data will be shard across 4 buffer which means single buffer length = total/4
            context.CopyResource(structuredBuffer.Buffer, bufferOut);
            context.Flush();

            DataStream stream;
            DataBox box = context.MapSubresource(bufferOut, 0, MapMode.Read, MapFlags.None, out stream);
            IntPtr ptr = box.DataPointer;
            int copyOutBuffer = 100;
            DagItem[] resultArr = new DagItem[copyOutBuffer];
            for (int i = 0; i < copyOutBuffer; i++)
            {
                resultArr[i] = Marshal.PtrToStructure<DagItem>(ptr + structuredBuffer.Stride * i);
            }
            context.UnmapSubresource(bufferOut, 0);

            return resultArr;
        }

        public DebugMsg[] getDebugData(   )
        {
            StructuredBuffer<DebugMsg> structuredBuffer = debugData;
            bufferOut = structuredBuffer.CreateStaging();

            // data will be shard across 4 buffer which means single buffer length = total/4
            context.CopyResource(structuredBuffer.Buffer, bufferOut);
            context.Flush();

            DataStream stream;
            DataBox box = context.MapSubresource(bufferOut, 0, MapMode.Read, MapFlags.None, out stream);
            IntPtr ptr = box.DataPointer;
            DebugMsg[] resultArr = new DebugMsg[structuredBuffer.Capacity];
            for (int i = 0; i < structuredBuffer.Capacity; i++)
            {
                resultArr[i] = Marshal.PtrToStructure<DebugMsg>(ptr + structuredBuffer.Stride * i);
            }

            context.UnmapSubresource(bufferOut, 0);
            return resultArr;
        }


    }
}
