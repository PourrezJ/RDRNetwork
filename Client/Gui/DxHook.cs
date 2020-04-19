using RDR2;
using RDRNetwork.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using SharpDX;
using SharpDX.Direct2D1;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using D3D12 = SharpDX.Direct3D12;
using RDRN_Shared;
using SharpDX.WIC;

namespace RDRNetwork.Gui.DirectXHook
{
    public class DxHook
    {
        public DxHook()
        {
            var address = GetProcAddress();
            
            hookPresent = new HookWrapper<PresentDelegate>(
                address[140], new PresentDelegate(PresentHook), this);
            /*
           hookDrawInstanced = new HookWrapper<DrawInstancedDelegate>(
               address[84], new DrawInstancedDelegate(DrawInstancedHook), this);

           hookDrawIndexedInstanced = new HookWrapper<DrawIndexedInstancedDelegate>(
               address[85], new DrawIndexedInstancedDelegate(DrawIndexedInstancedHook), this);
           */
            hookExecuteCommandLists = new HookWrapper<ExecuteCommandListsDelegate>(
                address[54], new ExecuteCommandListsDelegate(ExecuteCommandListsHook), this);
           /*
            hookSignalDelegate = new HookWrapper<SignalDelegate>(
                address[58], new SignalDelegate(SignalHook), this);*/
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate SharpDX.Result PresentDelegate(IntPtr swapChainPtr, int syncInterval, DXGI.PresentFlags flags);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate void DrawInstancedDelegate(IntPtr commandListPtr, int vertexCountPerInstance, int instanceCount, int startVertexLocation, int startInstanceLocation);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate void DrawIndexedInstancedDelegate(IntPtr commandListPtr, int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate void ExecuteCommandListsDelegate(IntPtr commandQueuePtr, uint numCommandLists, IntPtr commandListsOut);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate void SignalDelegate(IntPtr commandQueuePtr, IntPtr fenceRef, ulong value);

        private HookWrapper<PresentDelegate> hookPresent;
        private HookWrapper<DrawInstancedDelegate> hookDrawInstanced;
        private HookWrapper<DrawIndexedInstancedDelegate> hookDrawIndexedInstanced;
        private HookWrapper<ExecuteCommandListsDelegate> hookExecuteCommandLists;
        private HookWrapper<SignalDelegate> hookSignalDelegate;

        private bool init = false;
        private bool shutdown = false;

        public static DXGI.SwapChain3 SwapChain;
        public static D3D12.CommandQueue CommandQueue;
        public static D3D12.Device Device12;
        public static D3D12.Fence Fence;
        public static long FenceValue;
        public static D3D12.CommandAllocator CommandAllocator;
        public static D3D12.GraphicsCommandList CommandList;
        public static D3D12.PipelineState PipelineState;

        D3D12.DescriptorHeap descriptorHeapBackBuffers;
        D3D12.DescriptorHeap descriptorHeapImGuiRender;

        public static List<ImageElement> ImageElements 
            = new List<ImageElement>();

        struct FrameContext
        {
            public D3D12.CommandAllocator commandAllocator;
            public D3D12.Resource mainRanderTargetResource;
            public D3D12.CpuDescriptorHandle mainRanderTargetDescriptor;
        }

        Color[] colors = new Color[]
        {
            Color.Red,
            Color.Magenta,
            Color.Yellow,
            Color.YellowGreen,
            Color.Green,
            Color.Red
        };

        static int buffersCounts = -1;
        static FrameContext[] frameContext;

        //Direct3D11
        private static SharpDX.Direct3D11.Device Device11;
        public static D3D11.DeviceContext DeviceContext11 { get; private set; }
        public static D3D11.Device11On12 Device11on12 { get; private set; }

        static SharpDX.Direct3D11.Resource[] wrappedBackBuffers;

        //Direct2D
        static SharpDX.DirectWrite.TextFormat textFormat;
        static SharpDX.Direct2D1.SolidColorBrush textBrush;
        static SharpDX.Direct2D1.RenderTarget[] direct2DRenderTarget;

        static ImagingFactory2 WicFactory { get; set; }

        private static int frameIndex
        {
            get => SwapChain?.CurrentBackBufferIndex ?? 0;
        }

        public static RenderTarget CurrentRenderTarget
        {
            get
            {
                if (direct2DRenderTarget != null)
                    return direct2DRenderTarget[frameIndex];
                return null;
            }
        }

        public bool Started { get; private set; }
       

        public SharpDX.Result PresentHook(IntPtr swapChainPtr, int syncInterval, DXGI.PresentFlags flags)
        {
            if (CommandQueue == null)
                return hookPresent.Target(swapChainPtr, syncInterval, flags);

            if (SwapChain == null || SwapChain != null && SwapChain.NativePointer != swapChainPtr)
                SwapChain = DXGI.SwapChain3.FromPointer<DXGI.SwapChain3>(swapChainPtr);

            if (!init)
            {
                init = true;

                Device12 = SwapChain.GetDevice<D3D12.Device>();

                var sdesc = SwapChain.Description;
                sdesc.Flags = DXGI.SwapChainFlags.AllowModeSwitch;

                buffersCounts = sdesc.BufferCount;
                frameContext = new FrameContext[buffersCounts];

                var descriptorImGuiRender = new D3D12.DescriptorHeapDescription();
                descriptorImGuiRender.Type = D3D12.DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView;
                descriptorImGuiRender.DescriptorCount = buffersCounts;
                descriptorImGuiRender.Flags = D3D12.DescriptorHeapFlags.ShaderVisible;

                descriptorHeapImGuiRender = Device12.CreateDescriptorHeap(descriptorImGuiRender);

                CommandAllocator = Device12.CreateCommandAllocator(D3D12.CommandListType.Direct);

                for (int i = 0; i < buffersCounts; i++)
                {
                    frameContext[i].commandAllocator = CommandAllocator;
                }

                CommandList = Device12.CreateCommandList(0, D3D12.CommandListType.Direct, CommandAllocator, null);
                
                var descriptorBackBuffers = new D3D12.DescriptorHeapDescription();
                descriptorBackBuffers.Type = D3D12.DescriptorHeapType.RenderTargetView;
                descriptorBackBuffers.DescriptorCount = buffersCounts;
                descriptorBackBuffers.Flags = D3D12.DescriptorHeapFlags.None;
                descriptorBackBuffers.NodeMask = 1;

                descriptorHeapBackBuffers = Device12.CreateDescriptorHeap(descriptorBackBuffers);
               
                //Init Direct3D11 device from Direct3D12 device
                Device11 = SharpDX.Direct3D11.Device.CreateFromDirect3D12(Device12, SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport, null, null, CommandQueue);
                DeviceContext11 = Device11.ImmediateContext;
                Device11on12 = Device11.QueryInterface<SharpDX.Direct3D11.Device11On12>();

                var d2dFactory = new SharpDX.Direct2D1.Factory(SharpDX.Direct2D1.FactoryType.SingleThreaded);

                var rtvDescriptorSize = Device12.GetDescriptorHandleIncrementSize(D3D12.DescriptorHeapType.RenderTargetView);
                var rtvHandle = descriptorHeapBackBuffers.CPUDescriptorHandleForHeapStart;
                
                wrappedBackBuffers = new SharpDX.Direct3D11.Resource[buffersCounts];
                direct2DRenderTarget = new SharpDX.Direct2D1.RenderTarget[buffersCounts];

                for (int i = 0; i < buffersCounts; i++)
                {
                    D3D12.Resource backBuffer;

                    backBuffer = SwapChain.GetBackBuffer<D3D12.Resource>(i);

                    frameContext[i].mainRanderTargetDescriptor = rtvHandle;

                    Device12.CreateRenderTargetView(backBuffer, null, rtvHandle);
                    frameContext[i].mainRanderTargetResource = backBuffer;
                    rtvHandle.Ptr += rtvDescriptorSize;

                    //init Direct2D surfaces
                    SharpDX.Direct3D11.D3D11ResourceFlags format = new SharpDX.Direct3D11.D3D11ResourceFlags()
                    {
                        BindFlags = (int)SharpDX.Direct3D11.BindFlags.RenderTarget,
                        CPUAccessFlags = (int)SharpDX.Direct3D11.CpuAccessFlags.None
                    };

                    Device11on12.CreateWrappedResource(
                        backBuffer, format,
                        (int)D3D12.ResourceStates.Present,
                        (int)D3D12.ResourceStates.RenderTarget,
                        typeof(SharpDX.Direct3D11.Resource).GUID,
                        out wrappedBackBuffers[i]);

                    //Init direct2D surface
                    var d2dSurface = wrappedBackBuffers[i].QueryInterface<DXGI.Surface>();
                    direct2DRenderTarget[i] = new SharpDX.Direct2D1.RenderTarget(d2dFactory, d2dSurface, new SharpDX.Direct2D1.RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(DXGI.Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied)));
                    d2dSurface.Dispose();

                    WicFactory = new SharpDX.WIC.ImagingFactory2();
                }
                d2dFactory.Dispose();
                
                //Init font
                var directWriteFactory = new SharpDX.DirectWrite.Factory();
                textFormat = new SharpDX.DirectWrite.TextFormat(directWriteFactory, "Arial", SharpDX.DirectWrite.FontWeight.Bold, SharpDX.DirectWrite.FontStyle.Normal, 1) { TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading, ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Near };
                textBrush = new SharpDX.Direct2D1.SolidColorBrush(direct2DRenderTarget[0], Color.White);
                directWriteFactory.Dispose();

                /*
                Texture = new DXSprite(Device11, DeviceContext11);

                Texture.Initialize();
                */
                //var texture = new ImageElement(@"c:\RDRNetwork\images\test.jpg", new System.Drawing.PointF(50,50));
                 //var texture2 = new ImageElement(@"c:\RDRNetwork\images\yoda.png", new System.Drawing.PointF(-50, -50));

                Started = true;
                LogManager.DebugLog("Dx12 Swapchain init ok!");
            }

            Device11on12.AcquireWrappedResources(new SharpDX.Direct3D11.Resource[] { wrappedBackBuffers[frameIndex] }, 1);

            CurrentRenderTarget.BeginDraw();
            
            if (ImageElements.Count > 0)
            {
                try
                {
                    lock (ImageElements)
                    {
                        for (int i = 0; i < ImageElements.Count; i++)
                            ImageElements[i].Draw();
                    }
                }
                catch(Exception ex)
                {
                    LogManager.LogException(ex, "DX Render");
                }
            }
            
            CurrentRenderTarget.DrawText("RDRNetwork", textFormat, new SharpDX.Mathematics.Interop.RawRectangleF(2, 2, 20, 50), textBrush);

            CurrentRenderTarget.EndDraw();

            Device11on12.ReleaseWrappedResources(new SharpDX.Direct3D11.Resource[] { wrappedBackBuffers[frameIndex] }, 1);

            DeviceContext11.Flush();

            return hookPresent.Target(swapChainPtr, syncInterval, flags);
        }
        /*
        public void DrawInstancedHook(IntPtr commandListPtr, int vertexCountPerInstance, int instanceCount, int startVertexLocation, int startInstanceLocation)
        {
            hookDrawInstanced.Target(commandListPtr, vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);
        }

        public void DrawIndexedInstancedHook(IntPtr commandListPtr, int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation)
        {
          
            if (CommandList == null || CommandList != null && CommandList.NativePointer != commandListPtr)
                CommandList = D3D12.GraphicsCommandList.FromPointer<D3D12.GraphicsCommandList>(commandListPtr); // crash with this
            
            hookDrawIndexedInstanced.Target(commandListPtr, indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);
        }*/
        
        public unsafe void ExecuteCommandListsHook(IntPtr commandQueuePtr, uint numCommandLists, IntPtr commandListsOut)
        {     
            
            if (CommandQueue == null || CommandQueue != null && commandQueuePtr != CommandQueue.NativePointer)
                CommandQueue = D3D12.CommandQueue.FromPointer<D3D12.CommandQueue>(commandQueuePtr);
   
            hookExecuteCommandLists.Target(CommandQueue.NativePointer, numCommandLists, commandListsOut);
        }
        /*
        public unsafe void SignalHook(IntPtr commandQueuePtr, IntPtr fenceRef, ulong value)
        {    
            
            if (CommandQueue != null && CommandQueue.NativePointer == commandQueuePtr)
            {
                if (Fence == null || Fence != null && Fence.NativePointer != fenceRef)
                    Fence = D3D12.Fence.FromPointer<D3D12.Fence>(fenceRef);

                FenceValue = value;
            }
        hookSignalDelegate.Target(CommandQueue != null ? CommandQueue.NativePointer : commandQueuePtr, fenceRef, value);
        }
*/
        #region Dispose
        public void Dispose()
        {
            hookPresent.Dispose();
            hookDrawInstanced.Dispose();
            hookDrawIndexedInstanced.Dispose();
            hookExecuteCommandLists.Dispose();
            hookSignalDelegate.Dispose();

            SharpDX.Utilities.Dispose(ref Device12);
        }
        #endregion

        #region Moved

        private List<IntPtr> GetProcAddress()
        {
            var address = new List<IntPtr>();

            DXGI.SwapChain3 _swapChain;
            SharpDX.Direct3D12.Device _device12;
            D3D12.CommandQueue _commandQueue;
            D3D12.CommandAllocator _commandAllocator;
            D3D12.GraphicsCommandList _commandList;


            _device12 = new SharpDX.Direct3D12.Device(null, SharpDX.Direct3D.FeatureLevel.Level_12_0);
            using (var renderForm = new Form())
            {
                using (var factory = new SharpDX.DXGI.Factory4())
                {
                    _commandQueue
                        = _device12.CreateCommandQueue(new SharpDX.Direct3D12.CommandQueueDescription(SharpDX.Direct3D12.CommandListType.Direct));

                    _commandAllocator
                        = _device12.CreateCommandAllocator(D3D12.CommandListType.Direct);

                    _commandList
                        = _device12.CreateCommandList(D3D12.CommandListType.Direct, _commandAllocator, null);

                    var swapChainDesc = new SharpDX.DXGI.SwapChainDescription()
                    {
                        BufferCount = 2,
                        ModeDescription = new SharpDX.DXGI.ModeDescription(100, 100, new SharpDX.DXGI.Rational(60, 1), SharpDX.DXGI.Format.R8G8B8A8_UNorm),
                        Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                        SwapEffect = SharpDX.DXGI.SwapEffect.FlipDiscard,
                        OutputHandle = renderForm.Handle,
                        Flags = DXGI.SwapChainFlags.AllowModeSwitch,
                        SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                        IsWindowed = true
                    };

                    var tempSwapChain = new SharpDX.DXGI.SwapChain(factory, _commandQueue, swapChainDesc);
                    _swapChain = tempSwapChain.QueryInterface<SharpDX.DXGI.SwapChain3>();
                    tempSwapChain.Dispose();
                }

                if (_device12 != null && _swapChain != null)
                {
                    address.AddRange(GetVTblAddresses(_device12.NativePointer, 44));
                    address.AddRange(GetVTblAddresses(_commandQueue.NativePointer, 19));
                    address.AddRange(GetVTblAddresses(_commandAllocator.NativePointer, 9));
                    address.AddRange(GetVTblAddresses(_commandList.NativePointer, 60));
                    address.AddRange(GetVTblAddresses(_swapChain.NativePointer, 18));

                    _device12.Dispose();
                    _device12 = null;

                    _commandQueue.Dispose();
                    _commandQueue = null;

                    _commandAllocator.Dispose();
                    _commandAllocator = null;

                    _commandList.Dispose();
                    _commandList = null;

                    _swapChain.Dispose();
                    _swapChain = null;
                }
            }

            return address;
        }

        protected List<IntPtr> GetVTblAddresses(IntPtr pointer, int numberOfMethods)
        {
            List<IntPtr> vtblAddresses = new List<IntPtr>();

            IntPtr vTable = Marshal.ReadIntPtr(pointer);
            for (int i = 0; i < numberOfMethods; i++)
            {
                var ptr = Marshal.ReadIntPtr(vTable, i * IntPtr.Size);
                vtblAddresses.Add(ptr); // using IntPtr.Size allows us to support both 32 and 64-bit processes
            }

            return vtblAddresses;
        }

        internal static SharpDX.WIC.BitmapSource LoadBitmap(string filename)
        {
            var bitmapDecoder = new SharpDX.WIC.BitmapDecoder(
                WicFactory,
                filename,
                SharpDX.WIC.DecodeOptions.CacheOnDemand
                );

            var result = new SharpDX.WIC.FormatConverter(WicFactory);

            result.Initialize(
                bitmapDecoder.GetFrame(0),
                SharpDX.WIC.PixelFormat.Format32bppPRGBA,
                SharpDX.WIC.BitmapDitherType.None,
                null,
                0.0,
                SharpDX.WIC.BitmapPaletteType.Custom);

            return result;
        }
        #endregion
    }
}