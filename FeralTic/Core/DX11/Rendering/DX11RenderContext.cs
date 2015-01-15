using FeralTic.DX11.Geometry;
using FeralTic.DX11.Resources;
using System;


namespace FeralTic.DX11
{
    public partial class DX11RenderContext : IDisposable
    {
        public SlimDX.Direct3D11.Device Device { get; private set; }

        public DefaultTextures DefaultTextures { get; private set; }
        public DX11ResourcePoolManager ResourcePool { get; private set; }
        public DX11RenderTargetStack RenderTargetStack { get; private set; }
        public DX11PrimitivesManager Primitives { get; private set; }
        public DX11RenderStateStack RenderStateStack { get; private set; }
        public DX11ResourceScheduler ResourceScheduler { get; private set; }

        private SlimDX.Direct3D11.ShaderResourceView[] nullsrvs = new SlimDX.Direct3D11.ShaderResourceView[128];
        private SlimDX.Direct3D11.UnorderedAccessView[] nulluavs = new SlimDX.Direct3D11.UnorderedAccessView[8];

        /// <summary>
        /// create from primary adapter
        /// </summary>
        /// <param name="flags"></param>
        public DX11RenderContext(SlimDX.Direct3D11.DeviceCreationFlags flags = SlimDX.Direct3D11.DeviceCreationFlags.None)
        {
            this.Device = new SlimDX.Direct3D11.Device(SlimDX.Direct3D11.DriverType.Hardware, flags);
        }

        /// <summary>
        /// create from specified adapter
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="flags"></param>
        public DX11RenderContext(SlimDX.DXGI.Adapter adapter, SlimDX.Direct3D11.DeviceCreationFlags flags = SlimDX.Direct3D11.DeviceCreationFlags.None)
        {
            this.Device = new SlimDX.Direct3D11.Device(adapter, flags);
        }

        /// <summary>
        /// create from exist device
        /// who should dispose ?
        /// </summary>
        /// <param name="device"></param>
        public DX11RenderContext(SlimDX.Direct3D11.Device device)
        {
            this.Device = device;
        }

        public SlimDX.DXGI.Adapter Adapter
        {
            get
            {
                var factory = this.Device.Factory;
                using (var dxgiDevice = new SlimDX.DXGI.Device(this.Device))
                {
                    return dxgiDevice.Adapter;
                }
            }
        }

        public void Initialize(int schedulerthreadcount = 1)
        {
            this.ResourcePool = new DX11ResourcePoolManager(this);
            this.RenderTargetStack = new DX11RenderTargetStack(this);
            this.DefaultTextures = new DefaultTextures(this);
            this.Primitives = new DX11PrimitivesManager(this);
            this.RenderStateStack = new DX11RenderStateStack(this);
            this.ResourceScheduler = new DX11ResourceScheduler(this, schedulerthreadcount);
            this.ResourceScheduler.Initialize();
            this.CheckBufferSupport();
            this.BuildFormatSampling();
        }

        private void CheckBufferSupport()
        {
            try
            {
                DX11RawBuffer raw = new DX11RawBuffer(this.Device, 16);
                raw.Dispose();
                this.computesupport = true;
            }
            catch
            {
                this.computesupport = false;
            }
        }

        public void BeginFrame()
        {
            this.ResourcePool.BeginFrame();
        }

        public void EndFrame()
        {
        }

        public void CleanUp()
        {
            var context = Device.ImmediateContext;
            context.PixelShader.SetShaderResources(nullsrvs, 0, 128);
            context.DomainShader.SetShaderResources(nullsrvs, 0, 128);
            context.HullShader.SetShaderResources(nullsrvs, 0, 128);
            context.GeometryShader.SetShaderResources(nullsrvs, 0, 128);
            context.VertexShader.SetShaderResources(nullsrvs, 0, 128);
            this.CleanUpCS();
        }

        public void CleanUpPS()
        {
            Device.ImmediateContext.PixelShader.SetShaderResources(nullsrvs, 0, 128);
        }

        public void CleanUpCS()
        {
            Device.ImmediateContext.ComputeShader.SetShaderResources(nullsrvs, 0, 128);

            if (this.IsFeatureLevel11)
            {
                this.Device.ImmediateContext.ComputeShader.SetUnorderedAccessViews(nulluavs, 0, 8);
            }
            else
            {
                this.Device.ImmediateContext.ComputeShader.SetUnorderedAccessViews(nulluavs, 0, 1);
            }
        }

        public void CleanShaderStages()
        {
            var context = Device.ImmediateContext;
            context.HullShader.Set(null);
            context.DomainShader.Set(null);
            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
            context.GeometryShader.Set(null);
            context.ComputeShader.Set(null);
        }

        public void Dispose()
        {
            Device.ImmediateContext.ClearState();

            this.ResourcePool.Dispose();
            this.DefaultTextures.Dispose();
            this.Primitives.Dispose();
            this.ResourceScheduler.Dispose();

            this.Device.Dispose();
        }

        public SlimDX.Direct3D11.FeatureLevel FeatureLevel { get { return this.Device.FeatureLevel; } }

        public bool IsFeatureLevel11 { get { return this.Device.FeatureLevel >= SlimDX.Direct3D11.FeatureLevel.Level_11_0; } }
        public bool IsAtLeast101 { get { return this.Device.FeatureLevel >= SlimDX.Direct3D11.FeatureLevel.Level_10_1; } }

        private bool computesupport;

        public bool ComputeShaderSupport
        {
            get
            {
                if (this.IsFeatureLevel11) return true;

                return this.computesupport;
            }
        }
    }
}
