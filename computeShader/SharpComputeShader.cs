using computeShader.Extension;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

using Buffer11 = SharpDX.Direct3D11.Buffer;

namespace computeShader
{
    public class SharpComputeShader<T> where T : struct
    {

        private Device _d3dDevice;
        private DeviceContext _d3dContext;
        private UnorderedAccessView _accessView;
        private ComputeShader _shader;

        private Buffer11 _buffer;
        private Buffer11 _resultBuffer;

        public SharpComputeShader(Microsoft.Xna.Framework.Graphics.GraphicsDevice MNGDevice, string filename, string functionName, int count)
        {
            _d3dDevice = (MNGDevice.Handle as SharpDX.Direct3D11.Device);
            _d3dContext = (MNGDevice.Handle as SharpDX.Direct3D11.Device).ImmediateContext;
            

            _accessView = CreateUAV(count, out _buffer);
            _resultBuffer = CreateStaging(count);


            var computeShaderByteCode = ShaderBytecode.CompileFromFile(filename, functionName, "cs_5_0");
            _shader = new ComputeShader(_d3dDevice, computeShaderByteCode);
        }

        private UnorderedAccessView CreateUAV(int count, out Buffer11 buffer)
        {
            int size = SharpDX.Utilities.SizeOf<T>();
            BufferDescription bufferDescription = new BufferDescription()
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = size,
                SizeInBytes = size * count
            };

            buffer = new Buffer11(_d3dDevice, bufferDescription);


            UnorderedAccessViewDescription uavDescription = new UnorderedAccessViewDescription()
            {
                Buffer = new UnorderedAccessViewDescription.BufferResource() { FirstElement = 0, Flags = UnorderedAccessViewBufferFlags.None, ElementCount = count },
                Format = SharpDX.DXGI.Format.Unknown,
                Dimension = UnorderedAccessViewDimension.Buffer
            };

            return new UnorderedAccessView(_d3dDevice, buffer, uavDescription);

        }
        private Buffer11 CreateStaging(int count)
        {
            int size = SharpDX.Utilities.SizeOf<T>() * count;
            BufferDescription bufferDescription = new BufferDescription()
            {
                SizeInBytes = size,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write,
                Usage = ResourceUsage.Staging,
                OptionFlags = ResourceOptionFlags.None,
            };

            return new Buffer11(_d3dDevice, bufferDescription);
        }

        public void Begin()
        {
            _d3dContext.ComputeShader.SetUnorderedAccessView(0, _accessView);
            _d3dContext.ComputeShader.Set(_shader);
        }
        public void Start(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
        {
            _d3dContext.Dispatch(threadGroupCountX, threadGroupCountY, threadGroupCountZ);
        }

        public void End()
        {
            _d3dContext.CopyResource(_buffer, _resultBuffer);
            _d3dContext.Flush();
            _d3dContext.ComputeShader.SetUnorderedAccessView(0, null);
            _d3dContext.ComputeShader.Set(null);
        }

        public T[] ReadData(int count)
        {
            SharpDX.DataStream stream;
            SharpDX.DataBox box = _d3dContext.MapSubresource(_resultBuffer, 0, MapMode.Read, MapFlags.None, out stream);
            T[] result = stream.ReadRange<T>(count);
            _d3dContext.UnmapSubresource(_buffer, 0);
            return result;
        }
    }
}
