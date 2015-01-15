﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;

using FeralTic.DX11.Utils;



namespace FeralTic.DX11.Resources
{
    public class DX11DepthStencil : DX11Texture2D, IDX11DepthStencil
    {
        public DepthStencilView DSV { get; protected set; }
        public DepthStencilView ReadOnlyDSV { get; protected set; }

        public DX11Texture2D Stencil { get; protected set; }
        private ShaderResourceView stencilview;


        public DX11DepthStencil(DX11RenderContext context, int w, int h, SampleDescription sd)
            : this(context, w, h, sd, Format.D32_Float)
        {
        }

        public DX11DepthStencil(DX11RenderContext context, int w, int h, SampleDescription sd, Format format)
        {
            this.context = context;
            var depthBufferDesc = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = DepthFormatsHelper.GetGenericTextureFormat(format),
                Height = h,
                Width = w,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = sd,
                Usage = ResourceUsage.Default
            };

            this.Resource = new Texture2D(context.Device, depthBufferDesc);

            ShaderResourceViewDescription srvd = new ShaderResourceViewDescription()
            {
                ArraySize = 1,
                Format = DepthFormatsHelper.GetSRVFormat(format),
                Dimension = sd.Count == 1 ? ShaderResourceViewDimension.Texture2D : ShaderResourceViewDimension.Texture2DMultisampled,
                MipLevels = 1,
                MostDetailedMip = 0
            };

            this.SRV = new ShaderResourceView(context.Device, this.Resource, srvd);


            if (format == Format.D24_UNorm_S8_UInt)
            {
                ShaderResourceViewDescription stencild = new ShaderResourceViewDescription()
                {
                    ArraySize = 1,
                    Format = SlimDX.DXGI.Format.X24_Typeless_G8_UInt,
                    Dimension = sd.Count == 1 ? ShaderResourceViewDimension.Texture2D : ShaderResourceViewDimension.Texture2DMultisampled,
                    MipLevels = 1,
                    MostDetailedMip = 0
                };

                this.stencilview = new ShaderResourceView(this.context.Device, this.Resource, stencild);

                this.Stencil = DX11Texture2D.FromTextureAndSRV(this.context, this.Resource, this.stencilview);

            }
            else
            {
                //Just pass depth instead
                this.Stencil = DX11Texture2D.FromTextureAndSRV(this.context, this.Resource, this.SRV);
            }

            DepthStencilViewDescription dsvd = new DepthStencilViewDescription()
            {
                Format = DepthFormatsHelper.GetDepthFormat(format),
                Dimension = sd.Count == 1 ? DepthStencilViewDimension.Texture2D : DepthStencilViewDimension.Texture2DMultisampled,
                MipSlice = 0
            };

            this.DSV = new DepthStencilView(context.Device, this.Resource, dsvd);



            //Read only dsv only supported in dx11 minimum
            if (context.IsFeatureLevel11)
            {
                dsvd.Flags = DepthStencilViewFlags.ReadOnlyDepth;
                if (format == Format.D24_UNorm_S8_UInt) { dsvd.Flags |= DepthStencilViewFlags.ReadOnlyStencil; }

                this.ReadOnlyDSV = new DepthStencilView(context.Device, this.Resource, dsvd);
            }




            this.desc = depthBufferDesc;
            this.isowner = true;
        }

        public void Clear(bool cleardepth = true, bool clearstencil = true, float depth = 1.0f, byte stencil = 0)
        {
            if (cleardepth || clearstencil)
            {
                DepthStencilClearFlags flags = (DepthStencilClearFlags)0;
                if (cleardepth) { flags = DepthStencilClearFlags.Depth; }
                if (clearstencil) { flags |= DepthStencilClearFlags.Stencil; }

                this.context.Device.ImmediateContext.ClearDepthStencilView(this.DSV, flags, depth, stencil);
            }

            
        }

        public override void Dispose()
        {
            if (this.stencilview != null) { this.stencilview.Dispose(); }
            if (this.DSV != null) { this.DSV.Dispose(); }
            if (this.ReadOnlyDSV != null) { this.ReadOnlyDSV.Dispose(); }
            base.Dispose();
        }
    }
}
