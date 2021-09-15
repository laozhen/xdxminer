using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace xdxminer_lib
{
    public static class Helpers { 
    
        [Conditional("DEBUG")]
        public static void CheckForUndisposed(bool disposed, object o)
        {
            Debug.Assert(disposed, "An object of type " + o.GetType() + " was not disposed prior to finalization.");
        }

        public static void Dispose<T>(ref T disposable) where T : IDisposable
        { 
            if (disposable != null)
                disposable.Dispose();
            disposable = default(T);
        }

    }
}
