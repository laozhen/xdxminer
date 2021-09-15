using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace xdxminer_lib
{
    public class StructuredBuffer<T> : IDisposable where T : struct
    {
        private Buffer buffer;
        private ShaderResourceView srv;
        private UnorderedAccessView uav;
        private Device device;

        public Buffer Buffer
        {
            get { return buffer; }
        }

        public ShaderResourceView SRV
        {
            get { return srv; }
        }

        public UnorderedAccessView UAV
        {
            get { return uav; }
        }

        public int Capacity { get; private set; }

        private string debugName;
        public string DebugName
        {
            get { return debugName; }
            set
            {
                debugName = value;
                buffer.DebugName = debugName;
                srv.DebugName = debugName + " SRV";
                uav.DebugName = debugName + " UAV";
            }
        }
        public int Stride { get; private set; }
        public StructuredBuffer(Device device, int initialCapacity, string debugName = "UNNAMED")
        {
            this.device = device;
            this.debugName = debugName;
            Stride = Marshal.SizeOf(typeof(T));
            CreateBufferForSize(initialCapacity, out buffer, out srv, out uav);
            Capacity = initialCapacity;
        }

        public Buffer CreateStaging()
        {
            BufferDescription bufferDescription = new BufferDescription()
            {
                SizeInBytes = Stride * Capacity,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write,
                Usage = ResourceUsage.Staging,
                OptionFlags = ResourceOptionFlags.None,
            };

            return new Buffer(device, bufferDescription);
        }


        private void CreateBufferForSize(int size, out Buffer newBuffer, out ShaderResourceView newSRV, out UnorderedAccessView newUAV)
        {
            newBuffer = new Buffer(device, new BufferDescription
            {
                BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                SizeInBytes = size * Stride,
                StructureByteStride = Stride,
                Usage = ResourceUsage.Default
            });
            newBuffer.DebugName = debugName;
            newSRV = new ShaderResourceView(device, newBuffer);
            newSRV.DebugName = debugName + " SRV";
            newUAV = new UnorderedAccessView(device, newBuffer);
            newUAV.DebugName = debugName + " UAV";
        }
        public void SetCapacityWithCopy(DeviceContext context, int newCapacity)
        {
            CreateBufferForSize(newCapacity, out Buffer newBuffer, out ShaderResourceView newSRV, out UnorderedAccessView newUAV);

            //Copies data from the previous buffer to the new buffer, truncating anything which does not fit.
            context.CopySubresourceRegion(buffer, 0, new ResourceRegion(0, 0, 0, Math.Min(Capacity, newCapacity) * Stride, 1, 1), newBuffer, 0);
            buffer.Dispose();
            srv.Dispose();
            uav.Dispose();
            buffer = newBuffer;
            srv = newSRV;
            uav = newUAV;
            Capacity = newCapacity;
        }

        public void SetCapacityWithoutCopy(int newCapacity)
        {
            buffer.Dispose();
            srv.Dispose();
            uav.Dispose();
            CreateBufferForSize(newCapacity, out buffer, out srv, out uav);
            Capacity = newCapacity;
        }

        public unsafe void Update (DeviceContext context, T[] newValues,int sourceOffset = 0, int destinationOffset = 0)
        {


            int count = newValues.Length;
            int strideInBytes = Marshal.SizeOf(typeof(T));
            IntPtr arrPtr = Marshal.AllocHGlobal(count * strideInBytes);

            for (int i = 0; i < newValues.Length; i++)
            {
                Marshal.StructureToPtr(newValues[i], arrPtr + i * strideInBytes, true);
            }
            //Marshal.Copy(arrPtr, arrBytes, 0, strideInBytes);
            context.UpdateSubresource(new DataBox(arrPtr), buffer, 0,
                    new ResourceRegion(destinationOffset * strideInBytes, 0, 0,
                        (destinationOffset + (count)) * strideInBytes, 1, 1));

        }


        private bool disposed;
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                uav.Dispose();
                srv.Dispose();
                buffer.Dispose();
            }
        }

#if DEBUG
        ~StructuredBuffer()
        {
            Helpers.CheckForUndisposed(disposed, this);
        }
#endif
    }
}
