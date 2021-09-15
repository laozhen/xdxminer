using SharpDX.Direct3D11;
using System;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace xdxminer_lib
{
    public class ConstantsBuffer<T> : IDisposable where T : struct
    {
        private Buffer buffer;
        public Buffer Buffer { get { return buffer; } }

        private bool mappable;

        public int Capacity { get; private set; }


        public ConstantsBuffer(Device device, bool mappable = true, string debugName = "UNNAMED")
        {
            this.mappable = mappable;

            var size = Marshal.SizeOf(typeof(T));
            var alignedSize = (size >> 4) << 4;
            if (alignedSize < size)
                alignedSize += 16;
            buffer = new Buffer(device, new BufferDescription
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = mappable ? CpuAccessFlags.Write : CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = alignedSize,
                StructureByteStride = alignedSize,
                Usage = mappable ? ResourceUsage.Dynamic : ResourceUsage.Default
            });
            buffer.DebugName = debugName;
        }


        public void Update(DeviceContext context, ref T bufferData)
        {
            if (mappable)
            {
                var dataBox = context.MapSubresource(Buffer, 0, MapMode.WriteDiscard, MapFlags.None);
                SharpDX.Utilities.Write(dataBox.DataPointer, ref bufferData);
                context.UnmapSubresource(buffer, 0);
            }
            else
            {
                context.UpdateSubresource(ref bufferData, Buffer);
            }
        }



        private bool disposed;
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                buffer.Dispose();
            }
        }

#if DEBUG
        ~ConstantsBuffer()
        {
            //Helpers.CheckForUndisposed(disposed, this);
        }
#endif
    }
}
