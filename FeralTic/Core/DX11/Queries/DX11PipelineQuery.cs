using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX.Direct3D11;

namespace FeralTic.DX11.Queries
{
    public class DX11PipelineQuery : IDX11Query
    {
        private DX11RenderContext context;

        private Query query;

        public bool hasrun = false;

        public PipelineStatistics Statistics { get; protected set; }

        public DX11PipelineQuery(DX11RenderContext context)
        {
            this.context = context;

            QueryDescription qd = new QueryDescription();
            qd.Flags = QueryFlags.None;
            qd.Type = QueryType.PipelineStatistics;

            this.query = new Query(context.Device, qd);
        }

        public void Start()
        {
            this.context.Device.ImmediateContext.Begin(query);
        }

        public void Stop()
        {
            this.context.Device.ImmediateContext.End(query);
            this.hasrun = true;
        }

        public void GetData()
        {
            if (this.hasrun == false) { return; }

            while (!this.context.Device.ImmediateContext.IsDataAvailable(this.query)) { }

            this.Statistics = this.context.Device.ImmediateContext.GetData<PipelineStatistics>(this.query);
        }

    }

}
