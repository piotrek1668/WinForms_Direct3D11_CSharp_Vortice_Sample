using System.Diagnostics;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.DXGI.Debug;

namespace WinFormsDirect3D11Sample
{
    public class DxgiInfoManager
    {
        private ulong next;
        private IDXGIInfoQueue? infoQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DxgiInfoManager"/> class.
        /// </summary>
        public DxgiInfoManager()
        {
            if (D3D11.SdkLayersAvailable())
            {
                this.infoQueue = DXGI.DXGIGetDebugInterface1<IDXGIInfoQueue>();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.infoQueue?.Release();
        }

        /// <summary>
        /// Prints the messages from the info queue.
        /// </summary>
        /// <returns>Returns a list with messages.</returns>
        public void PrintMessages()
        {
            if (this.infoQueue == null) return;

            var end = this.infoQueue.GetNumStoredMessages(DXGI.DebugAll);

            if ((end > 0) && (this.next < end))
            {
                Debug.WriteLine("\n--- BEGIN DXGI INFO MANAGER MESSAGES ---");
                for (ulong i = this.next; i < end; i++)
                {
                    var message = this.infoQueue.GetMessage(DXGI.DebugAll, i);
                    Debug.WriteLine(message.Description);
                }

                Debug.WriteLine("--- END DXGI INFO MANAGER MESSAGES ---\n");
            }
        }

        /// <summary>
        /// Sets the point from which new messages are collected.
        /// </summary>
        public void Set()
        {
            if (this.infoQueue == null) return;
            this.next = this.infoQueue.GetNumStoredMessages(DXGI.DebugAll);
        }
    }
}
