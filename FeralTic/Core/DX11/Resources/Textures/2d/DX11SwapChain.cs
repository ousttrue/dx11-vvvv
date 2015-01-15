﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using System.Windows.Forms;
using SlimDX.DXGI;
using SlimDX;

using Device = SlimDX.Direct3D11.Device;

namespace FeralTic.DX11.Resources
{
    public class DX11SwapChain : DX11Texture2D, IDX11RenderTargetView, IDX11RWResource
    {
        private IntPtr handle;
        private SwapChain swapchain;
        private bool allowuav = false;

        public RenderTargetView RTV { get; protected set; }

        public UnorderedAccessView UAV { get; protected set; }

        public IntPtr Handle { get { return this.handle; } }

        public SwapChain SwapChain
        {
            get { return this.swapchain; }
        }

        public DX11SwapChain(DX11RenderContext context, IntPtr handle, Format format, SampleDescription sampledesc,int rate,int bufferCount)
        {
            this.context = context;
            this.handle = handle;

            SwapChainDescription sd = new SwapChainDescription()
            {
                BufferCount = bufferCount,
                ModeDescription = new ModeDescription(0, 0, new Rational(rate, 1), format),
                IsWindowed = true,
                OutputHandle = handle,
                SampleDescription = sampledesc,
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput | Usage.ShaderInput,
                Flags = SwapChainFlags.None
            };

            if (sd.SampleDescription.Count == 1 && context.IsFeatureLevel11)
            {
                sd.Usage |= Usage.UnorderedAccess;
                this.allowuav = true;
            }

            this.swapchain = new SwapChain(context.Device.Factory, context.Device, sd);

            this.Resource = Texture2D.FromSwapChain<Texture2D>(this.swapchain, 0);

            this.context.Device.Factory.SetWindowAssociation(handle, WindowAssociationFlags.IgnoreAltEnter);

            this.RTV = new RenderTargetView(context.Device, this.Resource);
            this.SRV = new ShaderResourceView(context.Device, this.Resource);
            if (this.allowuav) { this.UAV = new UnorderedAccessView(context.Device, this.Resource); }

            this.desc = this.Resource.Description;
        }


        public void SetFullScreen(bool fullscreen)
        {
            bool fs;
            Output opt;
            this.swapchain.GetFullScreenState(out fs, out opt);

            if (this.swapchain.IsFullScreen != fullscreen)
            {
                this.swapchain.IsFullScreen = fullscreen;
            }
        }

        public bool IsFullScreen
        {
            get
            {
                bool fs;
                Output opt;
                this.swapchain.GetFullScreenState(out fs, out opt);
                return fs;
            }
        }


        public void Resize()
        {
            this.Resize(0, 0);
        }

        public void Resize(int w, int h)
        {
            if (this.RTV != null) { this.RTV.Dispose(); }
            if (this.SRV != null) { this.SRV.Dispose(); }
            if (this.UAV != null) { this.UAV.Dispose(); }

            this.Resource.Dispose();

            this.swapchain.ResizeBuffers(1,w, h, SlimDX.DXGI.Format.Unknown, SwapChainFlags.AllowModeSwitch);

            this.Resource = Texture2D.FromSwapChain<Texture2D>(this.swapchain, 0);
            this.RTV = new RenderTargetView(context.Device, this.Resource);
            this.SRV = new ShaderResourceView(context.Device, this.Resource);
            if (this.allowuav) { this.UAV = new UnorderedAccessView(context.Device, this.Resource); }
            this.desc = this.Resource.Description;
        }

        public override void Dispose()
        {
            if (this.RTV != null) { this.RTV.Dispose(); }
            if (this.SRV != null) { this.SRV.Dispose(); }
            if (this.UAV != null) { this.UAV.Dispose(); }

            this.Resource.Dispose();
  
            if (this.swapchain != null)
            {
                if (this.IsFullScreen) { this.SetFullScreen(false); }
                this.swapchain.Dispose();
            }
            
        }

        public void Present(int syncInterval,PresentFlags flags)
        {
            try
            {
                this.swapchain.Present(syncInterval, flags);
            }
            catch
            {
                
            }
        }  
    }
}
