using SlimDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.DX11.Lib.Devices
{
    public class DXGIScreen
    {
        public DXGIScreen()
        {
            this.AdapterId = -1;
            this.Adapter = null;
            this.MonitorId = -1;
            this.Monitor = null;
        }

        public DXGIScreen(int adapterId, Adapter adapter, int monitorId, Output monitor)
        {
            this.AdapterId = adapterId;
            this.Adapter = adapter;
            this.MonitorId = monitorId;
            this.Monitor = monitor;
        }

        public int AdapterId { get; private set; }
        public Adapter Adapter { get; private set; }
        public int MonitorId { get; private set; }
        public Output Monitor { get; private set; }
    }
}
