#nullable disable

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

using SharpGen.Runtime;

using Vortice.D3DCompiler;
using Vortice.Direct2D1;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Direct3D11.Debug;
using Vortice.DirectWrite;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.WIC;

using BitmapInterpolationMode = Vortice.WIC.BitmapInterpolationMode;
using Color = Vortice.Mathematics.Color;
using FeatureLevel = Vortice.Direct3D.FeatureLevel;
using ResultCode = Vortice.DXGI.ResultCode;

namespace WinFormsDirect3D11Sample
{
    internal unsafe class Direct3D11 : IDisposable
    {
        private readonly Control leftControl;
        private readonly Control rightControl;
        private readonly MainWindow mainWindow;
        private ID3D11Device1 device; // virtual representation of the GPU and its resources
        private ID3D11DeviceContext1 deviceContext; // represents the graphics processing for the pipeline
        private IDXGISwapChain1 swapChain;
        private IDXGISwapChain1 swapChain2;
        private ID3D11Texture2D backBufferTexture;
        private ID3D11Texture2D backBufferTexture2;
        private ID3D11RenderTargetView renderTargetView;
        private ID3D11RenderTargetView renderTargetView2;
        private ID3D11Texture2D depthStencilTexture;
        private ID3D11Texture2D depthStencilTexture2;
        private ID3D11DepthStencilView depthStencilView;
        private ID3D11DepthStencilView depthStencilView2;
        private FeatureLevel highestSupportedFeatureLevel;
        private readonly bool debug;

        private ID3D11VertexShader vertexShaderPositionColor;
        private ID3D11VertexShader vertexShaderPositionTexture;

        private ID3D11PixelShader pixelShaderPositionColor;
        private ID3D11PixelShader pixelShaderPositionTexture;

        private ID3D11InputLayout inputLayoutPositionColor;
        private ID3D11InputLayout inputLayoutPositionTexture;

        private ID3D11SamplerState samplerState;
        private Stopwatch clock;
        private ID3D11ShaderResourceView shaderResourceView;

        private int lineVerticesBufferSize;
        private int signalVerticesBufferSize;

        private ID2D1RenderTarget renderTarget2DLeft;
        private ID2D1RenderTarget renderTarget2DRight;

        private IDWriteTextFormat textFormat;
        private const string Text = "DirectWrite & Direct2D";
        private ID3D11Texture2D texture;

        private ID3D11Buffer signalBuffer;
        private ID3D11Buffer gridBuffer;
        private ID3D11Buffer vertexBuffer;
        private ID3D11Buffer vertexBuffer2;
        private ID3D11Buffer indexBuffer;
        private ID3D11Buffer indexBuffer2;
        private ID3D11Buffer constantBuffer;

        private ID3D11Debug debugInterface;
        private ID3D11ShaderResourceView textureSrv;
        private ID3D11SamplerState textureSampler;

        // list of featureLevels this app can support
        private static readonly FeatureLevel[] FeatureLevels = {
            FeatureLevel.Level_11_1,
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_10_1,
            FeatureLevel.Level_10_0,
            FeatureLevel.Level_9_3,
            FeatureLevel.Level_9_2,
            FeatureLevel.Level_9_1,
        };

        public Direct3D11(MainWindow form, Control control, Control control2D)
        {
            this.mainWindow = form;
            this.leftControl = control;
            this.rightControl = control2D;
#if DEBUG
            debug = true;
#endif
        }

        internal void OnInit()
        {
            /*
             * 1. Try to create a hardware device and device context
             * 2. If that fails, try to create a WARP device (software based)
             * 3. Create a swap chain description and a swap chain fullscreeen description
             * 4. Then create a swap chain with given description for a given control.handle
             * 5. Create and set a backbuffer as RenderTargetView
             */

            // get factories
            using IDXGIFactory2 factory = DXGI.CreateDXGIFactory1<IDXGIFactory2>();
            using var writeFactory = DWrite.DWriteCreateFactory<IDWriteFactory>();
            using var direct2DFactory = D2D1.D2D1CreateFactory<ID2D1Factory>();

            // This flag adds support for surfaces with a color-channel ordering different
            // from the API default. It is required for compatibility with Direct2D.
            var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
            if (debug && D3D11.SdkLayersAvailable())
            {
                deviceCreationFlags |= DeviceCreationFlags.Debug;
            }

            using IDXGIAdapter1 adapter = GetHardwareAdapter();
            if (adapter == null)
            {
                return;
            }

            if (D3D11.D3D11CreateDevice(adapter, DriverType.Unknown, deviceCreationFlags, Direct3D11.FeatureLevels,
                    out ID3D11Device tempDevice, out FeatureLevel featureLevel, out ID3D11DeviceContext tempContext).Failure)
            {
                // Handle device interface creation failure if it occurs.
                // For example, reduce the feature level requirement, or fail over 
                // to WARP rendering.
                D3D11.D3D11CreateDevice(null, DriverType.Warp, deviceCreationFlags, Direct3D11.FeatureLevels,
                    out tempDevice, out featureLevel, out tempContext).CheckError();
            }

            var output = adapter.GetOutput(0);
            highestSupportedFeatureLevel = featureLevel;
            this.mainWindow.UpdateLabels(adapter.Description1.Description, highestSupportedFeatureLevel.ToString(), $"{output.Description.DesktopCoordinates.Right}x{output.Description.DesktopCoordinates.Bottom}");
            device = tempDevice.QueryInterface<ID3D11Device1>();
            deviceContext = tempContext.QueryInterface<ID3D11DeviceContext1>();
            tempContext.Dispose();
            tempDevice.Dispose();

            debugInterface = device.QueryInterface<ID3D11Debug>();

            SwapChainDescription1 swapChainDescription = new()
            {
                Width = leftControl.ClientSize.Width,
                Height = leftControl.ClientSize.Height,
                Format = Format.R8G8B8A8_UNorm,
                BufferCount = 2,
                BufferUsage = Usage.RenderTargetOutput,
                SampleDescription = SampleDescription.Default,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipDiscard,
                AlphaMode = AlphaMode.Ignore
            };

            SwapChainDescription1 swapChainDescription2 = new()
            {
                Width = rightControl.ClientSize.Width,
                Height = rightControl.ClientSize.Height,
                Format = Format.R8G8B8A8_UNorm,
                BufferCount = 2,
                BufferUsage = Usage.RenderTargetOutput,
                SampleDescription = SampleDescription.Default,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipDiscard,
                AlphaMode = AlphaMode.Ignore
            };

            SwapChainFullscreenDescription fullscreenDescription = new() { Windowed = true };

            swapChain = factory.CreateSwapChainForHwnd(device, leftControl.Handle, swapChainDescription, fullscreenDescription);
            swapChain2 = factory.CreateSwapChainForHwnd(device, rightControl.Handle, swapChainDescription2, fullscreenDescription);
            factory.MakeWindowAssociation(leftControl.Handle, WindowAssociationFlags.IgnoreAltEnter);
            factory.MakeWindowAssociation(rightControl.Handle, WindowAssociationFlags.IgnoreAltEnter);

            backBufferTexture = swapChain.GetBuffer<ID3D11Texture2D>(0);
            backBufferTexture2 = swapChain2.GetBuffer<ID3D11Texture2D>(0);
            renderTargetView = device.CreateRenderTargetView(backBufferTexture);
            renderTargetView2 = device.CreateRenderTargetView(backBufferTexture2);

            // configure Direct2D and DirectWrite
            textFormat = writeFactory.CreateTextFormat("Arial", 20.0f);
            textFormat.TextAlignment = TextAlignment.Center;
            textFormat.ParagraphAlignment = ParagraphAlignment.Center;

            RenderTargetProperties renderTargetProperties = new()
            {
                Type = RenderTargetType.Default,
                DpiX = 96,
                DpiY = 96,
                PixelFormat = Vortice.DCommon.PixelFormat.Premultiplied
            };

            var backBuffer2 = swapChain.GetBuffer<IDXGISurface>(0);
            renderTarget2DLeft = direct2DFactory.CreateDxgiSurfaceRenderTarget(backBuffer2, renderTargetProperties);

            var backBuffer = swapChain2.GetBuffer<IDXGISurface>(0);
            renderTarget2DRight = direct2DFactory.CreateDxgiSurfaceRenderTarget(backBuffer, renderTargetProperties);

            depthStencilTexture = device.CreateTexture2D(Format.D32_Float, leftControl.Width, leftControl.Height, 1, 1, null, BindFlags.DepthStencil);
            depthStencilView = device.CreateDepthStencilView(depthStencilTexture!, new DepthStencilViewDescription(depthStencilTexture, DepthStencilViewDimension.Texture2D));

            depthStencilTexture2 = device.CreateTexture2D(Format.D32_Float, rightControl.Width, rightControl.Height, 1, 1, null, BindFlags.DepthStencil);
            depthStencilView2 = device.CreateDepthStencilView(depthStencilTexture2!, new DepthStencilViewDescription(depthStencilTexture, DepthStencilViewDimension.Texture2D));

            signalVerticesBufferSize = 10;
            MeshData signal = MeshUtilities.CreateSignal();
            signalBuffer = device.CreateBuffer(signal.VerticesColor, BindFlags.VertexBuffer);

            lineVerticesBufferSize = 90;
            MeshData grid = MeshUtilities.CreateGrid();
            gridBuffer = device.CreateBuffer(grid.VerticesColor, BindFlags.VertexBuffer);

            Span<byte> vertexShaderByteCodePositionColor = CompileBytecode("PositionColor.hlsl", "VSMain", "vs_4_0");
            Span<byte> pixelShaderByteCodePositionColor = CompileBytecode("PositionColor.hlsl", "PSMain", "ps_4_0");

            vertexShaderPositionColor = device.CreateVertexShader(vertexShaderByteCodePositionColor);
            pixelShaderPositionColor = device.CreatePixelShader(pixelShaderByteCodePositionColor);
            inputLayoutPositionColor = device.CreateInputLayout(VertexPositionColor.InputElements, vertexShaderByteCodePositionColor);

            MeshData mesh = MeshUtilities.CreateCube(3.0f);
            this.vertexBuffer = device.CreateBuffer(mesh.Vertices, BindFlags.VertexBuffer);
            this.indexBuffer = device.CreateBuffer(mesh.Indices, BindFlags.IndexBuffer);
            this.constantBuffer = device.CreateConstantBuffer<Matrix4x4>();

            ReadOnlySpan<Color> pixels = stackalloc Color[16] {
                new Color(0xFFFFFFFF),
                new Color(0x00000000),
                new Color(0xFFFFFFFF),
                new Color(0x00000000),
                new Color(0x00000000),
                new Color(0xFFFFFFFF),
                new Color(0x00000000),
                new Color(0xFFFFFFFF),
                new Color(0xFFFFFFFF),
                new Color(0x00000000),
                new Color(0xFFFFFFFF),
                new Color(0x00000000),
                new Color(0x00000000),
                new Color(0xFFFFFFFF),
                new Color(0x00000000),
                new Color(0xFFFFFFFF),
            };

            var texture2D = device.CreateTexture2D(Format.R8G8B8A8_UNorm, 4, 4, pixels);
            shaderResourceView = device.CreateShaderResourceView(texture2D);
            samplerState = device.CreateSamplerState(SamplerDescription.PointWrap);

            Span<byte> vertexShaderByteCodeCube = CompileBytecode("Cube.hlsl", "VSMain", "vs_4_0");
            Span<byte> pixelShaderByteCodeCube = CompileBytecode("Cube.hlsl", "PSMain", "ps_4_0");

            vertexShaderPositionTexture = device.CreateVertexShader(vertexShaderByteCodeCube);
            pixelShaderPositionTexture = device.CreatePixelShader(pixelShaderByteCodeCube);
            inputLayoutPositionTexture = device.CreateInputLayout(VertexPositionNormalTexture.InputElements, vertexShaderByteCodeCube);

            MeshData mesh2 = MeshUtilities.CreateCube(8.0f);
            this.vertexBuffer2 = device.CreateBuffer(mesh2.Vertices, BindFlags.VertexBuffer);
            this.indexBuffer2 = device.CreateBuffer(mesh2.Indices, BindFlags.IndexBuffer);

            LoadTexture("10points.png");
            this.textureSrv = device.CreateShaderResourceView(this.texture);
            this.textureSampler = device.CreateSamplerState(SamplerDescription.PointWrap);

            this.clock = Stopwatch.StartNew();

            // use to report live object which need to be disposed!
            this.debugInterface.ReportLiveDeviceObjects(ReportLiveDeviceObjectFlags.Summary);
        }

        internal void OnRender()
        {
            if ((this.deviceContext == null) ||
                (this.renderTargetView == null) ||
                (this.renderTargetView2 == null) ||
                (this.depthStencilView == null) ||
                (this.swapChain == null))
            {
                return;
            }

            // clear render target and depth stencil view
            deviceContext.ClearRenderTargetView(renderTargetView2, Colors.CornflowerBlue);
            deviceContext.ClearDepthStencilView(depthStencilView2, DepthStencilClearFlags.Depth, 1.0f, 0);

            // set render target, viewport and scrissor rectangle
            deviceContext.OMSetRenderTargets(renderTargetView2, depthStencilView2);
            deviceContext.RSSetViewport(new Viewport(rightControl.Width, rightControl.Height));
            deviceContext.RSSetScissorRect(rightControl.Width, rightControl.Height);

            // draw direct2d
            if (drawText)
            {
                renderTarget2DRight.BeginDraw();
                renderTarget2DRight.Transform = Matrix3x2.Identity; // TODO: Rotation/Translation/etc. possible
                renderTarget2DRight.Clear(Colors.CornflowerBlue);
                var blackBrush = renderTarget2DRight.CreateSolidColorBrush(Colors.YellowGreen);
                var height = rightControl.Height / 5;
                var layoutRect = new Rect(0, height * 4, rightControl.Width, height);
                renderTarget2DRight.DrawText(Direct3D11.Text, textFormat, layoutRect, blackBrush);
                renderTarget2DRight.EndDraw();
            }

            // draw grid
            if (drawGrid)
            {
                deviceContext.IASetPrimitiveTopology(PrimitiveTopology.LineList);
                deviceContext.IASetInputLayout(inputLayoutPositionColor);
                deviceContext.IASetVertexBuffer(0, gridBuffer, VertexPositionColor.SizeInBytes);
                deviceContext.VSSetShader(vertexShaderPositionColor);
                deviceContext.PSSetShader(pixelShaderPositionColor);
                deviceContext.Draw(lineVerticesBufferSize * 2, 0);
            }

            // draw signal
            if (drawLine)
            {
                deviceContext.IASetPrimitiveTopology(PrimitiveTopology.LineStrip);
                deviceContext.IASetInputLayout(inputLayoutPositionColor);
                deviceContext.IASetVertexBuffer(0, signalBuffer, VertexPositionColor.SizeInBytes);
                deviceContext.VSSetShader(vertexShaderPositionColor);
                deviceContext.PSSetShader(pixelShaderPositionColor);
                deviceContext.Draw(signalVerticesBufferSize, 0);
            }

            // draw cube (texture)
            if (drawCube)
            {
                deviceContext.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
                deviceContext.IASetInputLayout(inputLayoutPositionTexture);
                deviceContext.IASetVertexBuffer(0, this.vertexBuffer, VertexPositionNormalTexture.SizeInBytes);
                deviceContext.IASetIndexBuffer(this.indexBuffer, Format.R16_UInt, 0);
                deviceContext.VSSetShader(vertexShaderPositionTexture);
                deviceContext.VSSetConstantBuffer(0, this.constantBuffer);
                deviceContext.PSSetShader(pixelShaderPositionTexture);
                deviceContext.PSSetShaderResource(0, shaderResourceView);
                deviceContext.PSSetSampler(0, samplerState);
                deviceContext.DrawIndexed(36, 0, 0);
            }

            // present swapchain2
            Result result2 = swapChain2.Present(1, PresentFlags.None);
            if (result2.Failure && (result2.Code == ResultCode.DeviceRemoved.Code))
            {
                throw new Exception();
            }

            // clear render target and depth stencil view
            deviceContext.ClearRenderTargetView(renderTargetView, Colors.CornflowerBlue);
            deviceContext.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

            // set render target, viewport and scrissor rectangle
            deviceContext.OMSetRenderTargets(renderTargetView, depthStencilView);
            deviceContext.RSSetViewport(new Viewport(leftControl.Width, leftControl.Height));
            deviceContext.RSSetScissorRect(leftControl.Width, leftControl.Height);

            // draw direct2d
            if (drawText)
            {
                renderTarget2DLeft.BeginDraw();
                renderTarget2DLeft.Transform = Matrix3x2.Identity; // Rotation/Translation/etc. possible
                renderTarget2DLeft.Clear(Colors.CornflowerBlue);
                var blackBrush = renderTarget2DLeft.CreateSolidColorBrush(Colors.Orange);
                var height = leftControl.Height / 5;
                var layoutRect = new Rect(0, height * 4, leftControl.Width, height);
                renderTarget2DLeft.DrawText(Direct3D11.Text, textFormat, layoutRect, blackBrush);
                renderTarget2DLeft.EndDraw();
            }

            // draw grid
            if (drawGrid)
            {
                deviceContext.IASetPrimitiveTopology(PrimitiveTopology.LineList);
                deviceContext.IASetInputLayout(inputLayoutPositionColor);
                deviceContext.IASetVertexBuffer(0, gridBuffer, VertexPositionColor.SizeInBytes);
                deviceContext.VSSetShader(vertexShaderPositionColor);
                deviceContext.PSSetShader(pixelShaderPositionColor);
                deviceContext.Draw(lineVerticesBufferSize * 2, 0);
            }
            
            // draw signal
            if (drawLine)
            {
                deviceContext.IASetPrimitiveTopology(PrimitiveTopology.LineStrip);
                deviceContext.IASetInputLayout(inputLayoutPositionColor);
                deviceContext.IASetVertexBuffer(0, signalBuffer, VertexPositionColor.SizeInBytes);
                deviceContext.VSSetShader(vertexShaderPositionColor);
                deviceContext.PSSetShader(pixelShaderPositionColor);
                deviceContext.Draw(signalVerticesBufferSize, 0);
            }

            // draw cube (texture from file)
            if (drawCube)
            {
                deviceContext.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
                deviceContext.IASetInputLayout(inputLayoutPositionTexture);
                deviceContext.IASetVertexBuffer(0, this.vertexBuffer2, VertexPositionNormalTexture.SizeInBytes);
                deviceContext.IASetIndexBuffer(this.indexBuffer2, Format.R16_UInt, 0);
                deviceContext.VSSetShader(vertexShaderPositionTexture);
                deviceContext.VSSetConstantBuffer(0, this.constantBuffer);
                deviceContext.PSSetShader(pixelShaderPositionTexture);
                deviceContext.PSSetShaderResource(0, this.textureSrv);
                deviceContext.PSSetSampler(0, this.textureSampler);
                deviceContext.DrawIndexed(36, 0, 0);
            }

            // present swapchain1
            Result result = swapChain.Present(1, PresentFlags.None);
            if (result.Failure && (result.Code == ResultCode.DeviceRemoved.Code))
            {
                throw new Exception();
            }
        }

        internal void OnUpdate()
        {
            if (deviceContext == null)
            {
                return;
            }

            if (drawCube)
            {
                // update constant buffer for textured cube
                var time = this.clock.ElapsedMilliseconds / 1000.0f;
                Matrix4x4 world = Matrix4x4.CreateRotationX(time) * Matrix4x4.CreateRotationY(time * 2) * Matrix4x4.CreateRotationZ(time * .7f);

                Matrix4x4 view = Matrix4x4.CreateLookAt(new Vector3(0, 0, 25), new Vector3(0, 0, 0), Vector3.UnitY);
                var AspectRatio = (float)leftControl.ClientSize.Width / leftControl.ClientSize.Height;
                Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4, AspectRatio, 0.1f, 100);
                Matrix4x4 viewProjection = Matrix4x4.Multiply(view, projection);
                Matrix4x4 worldViewProjection = Matrix4x4.Multiply(world, viewProjection);

                // Update constant buffer data
                MappedSubresource mappedResource = deviceContext.Map(this.constantBuffer, MapMode.WriteDiscard);
                Unsafe.Copy(mappedResource.DataPointer.ToPointer(), ref worldViewProjection);
                deviceContext.Unmap(this.constantBuffer, 0);
            }
        }

        private static Format ToDXGIFormat(Guid guid)
        {
            if (s_WICFormats.TryGetValue(guid, out Format format))
            {
                return format;
            }

            return Format.Unknown;
        }

        private static int WICBitsPerPixel(IWICImagingFactory factory, Guid targetGuid)
        {
            using IWICComponentInfo info = factory.CreateComponentInfo(targetGuid);
            ComponentType type = info.ComponentType;
            if (type != ComponentType.PixelFormat)
            {
                return 0;
            }

            using IWICPixelFormatInfo pixelFormatInfo = info.QueryInterface<IWICPixelFormatInfo>();
            return pixelFormatInfo.BitsPerPixel;
        }

        private void LoadTexture(string fileName, int width = 0, int height = 0)
        {
            string assetsPath = Path.Combine(AppContext.BaseDirectory, "Textures");
            string textureFile = Path.Combine(assetsPath, fileName);

            using var wicFactory = new IWICImagingFactory();
            using IWICBitmapDecoder decoder = wicFactory.CreateDecoderFromFileName(textureFile);
            using IWICBitmapFrameDecode frame = decoder.GetFrame(0);

            SizeI size = frame.Size;

            // Determine format
            Guid pixelFormat = frame.PixelFormat;
            Guid convertGUID = pixelFormat;

            Format format = ToDXGIFormat(pixelFormat);
            int bpp = 0;
            if (format == Format.Unknown)
            {
                if (pixelFormat == PixelFormat.Format96bppRGBFixedPoint)
                {
                    convertGUID = PixelFormat.Format96bppRGBFixedPoint;
                    format = Format.R32G32B32_Float;
                    bpp = 96;
                }
                else
                {
                    foreach (KeyValuePair<Guid, Guid> item in s_WICConvert)
                    {
                        if (item.Key == pixelFormat)
                        {
                            convertGUID = item.Value;

                            format = ToDXGIFormat(item.Value);
                            Debug.Assert(format != Format.Unknown);
                            bpp = WICBitsPerPixel(wicFactory, convertGUID);
                            break;
                        }
                    }
                }

                if (format == Format.Unknown)
                {
                    throw new InvalidOperationException("WICTextureLoader does not support all DXGI formats");
                    //Debug.WriteLine("ERROR: WICTextureLoader does not support all DXGI formats (WIC GUID {%8.8lX-%4.4X-%4.4X-%2.2X%2.2X-%2.2X%2.2X%2.2X%2.2X%2.2X%2.2X}). Consider using DirectXTex.\n",
                    //    pixelFormat.Data1, pixelFormat.Data2, pixelFormat.Data3,
                    //    pixelFormat.Data4[0], pixelFormat.Data4[1], pixelFormat.Data4[2], pixelFormat.Data4[3],
                    //    pixelFormat.Data4[4], pixelFormat.Data4[5], pixelFormat.Data4[6], pixelFormat.Data4[7]);
                }
            }
            else
            {
                // Convert BGRA8UNorm to RGBA8Norm
                if (pixelFormat == PixelFormat.Format32bppBGRA)
                {
                    format = ToDXGIFormat(PixelFormat.Format32bppRGBA);
                    convertGUID = PixelFormat.Format32bppRGBA;
                }

                bpp = WICBitsPerPixel(wicFactory, pixelFormat);
            }

            if (format == Format.R32G32B32_Float)
            {
                // Special case test for optional device support for autogen mipchains for R32G32B32_FLOAT
                FormatSupport fmtSupport = device.CheckFormatSupport(Format.R32G32B32_Float);
                if (!fmtSupport.HasFlag(FormatSupport.MipAutogen))
                {
                    // Use R32G32B32A32_FLOAT instead which is required for Feature Level 10.0 and up
                    convertGUID = PixelFormat.Format128bppRGBAFloat;
                    format = Format.R32G32B32A32_Float;
                    bpp = 128;
                }
            }

            // Verify our target format is supported by the current device
            // (handles WDDM 1.0 or WDDM 1.1 device driver cases as well as DirectX 11.0 Runtime without 16bpp format support)
            FormatSupport support = device.CheckFormatSupport(format);
            if (!support.HasFlag(FormatSupport.Texture2D))
            {
                // Fallback to RGBA 32-bit format which is supported by all devices
                convertGUID = PixelFormat.Format32bppRGBA;
                format = Format.R8G8B8A8_UNorm;
                bpp = 32;
            }

            int rowPitch = (size.Width * bpp + 7) / 8;
            int sizeInBytes = rowPitch * size.Height;

            byte[] pixels = new byte[sizeInBytes];

            if (width == 0)
            {
                width = size.Width;
            }

            if (height == 0)
            {
                height = size.Height;
            }

            // Load image data
            if (convertGUID == pixelFormat && size.Width == width && size.Height == height)
            {
                // No format conversion or resize needed
                frame.CopyPixels(rowPitch, pixels);
            }
            else if (size.Width != width || size.Height != height)
            {
                // Resize
                using IWICBitmapScaler scaler = wicFactory.CreateBitmapScaler();
                scaler.Initialize(frame, width, height, BitmapInterpolationMode.Fant);

                Guid pixelFormatScaler = scaler.PixelFormat;

                if (convertGUID == pixelFormatScaler)
                {
                    // No format conversion needed
                    scaler.CopyPixels(rowPitch, pixels);
                }
                else
                {
                    using IWICFormatConverter converter = wicFactory.CreateFormatConverter();

                    bool canConvert = converter.CanConvert(pixelFormatScaler, convertGUID);
                    if (!canConvert)
                    {
                        return;
                    }

                    converter.Initialize(scaler, convertGUID, BitmapDitherType.ErrorDiffusion, null, 0, BitmapPaletteType.MedianCut);
                    converter.CopyPixels(rowPitch, pixels);
                }
            }
            else
            {
                // Format conversion but no resize
                using IWICFormatConverter converter = wicFactory.CreateFormatConverter();

                bool canConvert = converter.CanConvert(pixelFormat, convertGUID);
                if (!canConvert)
                {
                    return;
                }

                converter.Initialize(frame, convertGUID, BitmapDitherType.ErrorDiffusion, null, 0, BitmapPaletteType.MedianCut);
                converter.CopyPixels(rowPitch, pixels);
            }

            this.texture = device.CreateTexture2D(format, size.Width, size.Height, pixels);
        }

        // TODO: Remove once new release of Vortice gets out (Vortice.WIC.PixelFormat)
        private static readonly Dictionary<Guid, Format> s_WICFormats = new()
        {
            { PixelFormat.Format128bppRGBAFloat,        Format.R32G32B32A32_Float },

            { PixelFormat.Format64bppRGBAHalf,          Format.R16G16B16A16_Float},
            { PixelFormat.Format64bppRGBA,              Format.R16G16B16A16_UNorm },

            { PixelFormat.Format32bppRGBA,              Format.R8G8B8A8_UNorm },
            { PixelFormat.Format32bppBGRA,              Format.B8G8R8A8_UNorm }, // DXGI 1.1
            { PixelFormat.Format32bppBGR,               Format.B8G8R8X8_UNorm }, // DXGI 1.1

            { PixelFormat.Format32bppRGBA1010102XR,     Format.R10G10B10_Xr_Bias_A2_UNorm }, // DXGI 1.1
            { PixelFormat.Format32bppRGBA1010102,       Format.R10G10B10A2_UNorm },

            { PixelFormat.Format16bppBGRA5551,          Format.B5G5R5A1_UNorm },
            { PixelFormat.Format16bppBGR565,            Format.B5G6R5_UNorm },

            { PixelFormat.Format32bppGrayFloat,         Format.R32_Float },
            { PixelFormat.Format16bppGrayHalf,          Format.R16_Float },
            { PixelFormat.Format16bppGray,              Format.R16_UNorm },
            { PixelFormat.Format8bppGray,               Format.R8_UNorm },

            { PixelFormat.Format8bppAlpha,              Format.A8_UNorm },
            { PixelFormat.Format96bppRGBFloat,          Format.R32G32B32_Float },
        };

        private static readonly Dictionary<Guid, Guid> s_WICConvert = new()
        {
            // Note target GUID in this conversion table must be one of those directly supported formats (above).

            { PixelFormat.FormatBlackWhite,            PixelFormat.Format8bppGray }, // DXGI_FORMAT_R8_UNORM

            { PixelFormat.Format1bppIndexed,           PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
            { PixelFormat.Format2bppIndexed,           PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
            { PixelFormat.Format4bppIndexed,           PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
            { PixelFormat.Format8bppIndexed,           PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM

            { PixelFormat.Format2bppGray,              PixelFormat.Format8bppGray }, // DXGI_FORMAT_R8_UNORM
            { PixelFormat.Format4bppGray,              PixelFormat.Format8bppGray }, // DXGI_FORMAT_R8_UNORM

            { PixelFormat.Format16bppGrayFixedPoint,   PixelFormat.Format16bppGrayHalf }, // DXGI_FORMAT_R16_FLOAT
            { PixelFormat.Format32bppGrayFixedPoint,   PixelFormat.Format32bppGrayFloat }, // DXGI_FORMAT_R32_FLOAT

            { PixelFormat.Format16bppBGR555,           PixelFormat.Format16bppBGRA5551 }, // DXGI_FORMAT_B5G5R5A1_UNORM

            { PixelFormat.Format32bppBGR101010,        PixelFormat.Format32bppRGBA1010102 }, // DXGI_FORMAT_R10G10B10A2_UNORM

            { PixelFormat.Format24bppBGR,              PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
            { PixelFormat.Format24bppRGB,              PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
            { PixelFormat.Format32bppPBGRA,            PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
            { PixelFormat.Format32bppPRGBA,            PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM

            { PixelFormat.Format48bppRGB,              PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM
            { PixelFormat.Format48bppBGR,              PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM
            { PixelFormat.Format64bppBGRA,             PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM
            { PixelFormat.Format64bppPRGBA,            PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM
            { PixelFormat.Format64bppPBGRA,            PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM

            { PixelFormat.Format48bppRGBFixedPoint,    PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT
            { PixelFormat.Format48bppBGRFixedPoint,    PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT
            { PixelFormat.Format64bppRGBAFixedPoint,   PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT
            { PixelFormat.Format64bppBGRAFixedPoint,   PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT
            { PixelFormat.Format64bppRGBFixedPoint,    PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT
            { PixelFormat.Format64bppRGBHalf,          PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT
            { PixelFormat.Format48bppRGBHalf,          PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT

            { PixelFormat.Format128bppPRGBAFloat,      PixelFormat.Format128bppRGBAFloat }, // DXGI_FORMAT_R32G32B32A32_FLOAT
            { PixelFormat.Format128bppRGBFloat,        PixelFormat.Format128bppRGBAFloat }, // DXGI_FORMAT_R32G32B32A32_FLOAT
            { PixelFormat.Format128bppRGBAFixedPoint,  PixelFormat.Format128bppRGBAFloat }, // DXGI_FORMAT_R32G32B32A32_FLOAT
            { PixelFormat.Format128bppRGBFixedPoint,   PixelFormat.Format128bppRGBAFloat }, // DXGI_FORMAT_R32G32B32A32_FLOAT
            { PixelFormat.Format32bppRGBE,             PixelFormat.Format128bppRGBAFloat }, // DXGI_FORMAT_R32G32B32A32_FLOAT

            { PixelFormat.Format32bppCMYK,             PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
            { PixelFormat.Format64bppCMYK,             PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM
            { PixelFormat.Format40bppCMYKAlpha,        PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
            { PixelFormat.Format80bppCMYKAlpha,        PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM

            { PixelFormat.Format32bppRGB,              PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
            { PixelFormat.Format64bppRGB,              PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM
            { PixelFormat.Format64bppPRGBAHalf,        PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT

            // We don't support n-channel formats
        };

        private bool drawGrid;
        private bool drawLine;
        private bool drawCube;
        private bool drawText;

        public void SetDrawObject(string name)
        {
            drawGrid = false;
            drawLine = false;
            drawCube = false;
            drawText = false;

            switch (name)
            {
                case "drawGrid":
                    this.drawGrid = true;
                    break;
                case "drawLine":
                    this.drawLine = true;
                    break;
                case "drawCube":
                    this.drawCube = true;
                    break;
                case "drawText":
                    this.drawText = true;
                    break;
            }
        }

        private static Span<byte> CompileBytecode(string shaderName, string entryPoint, string profile)
        {
            string assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");
            string shaderFile = Path.Combine(assetsPath, shaderName);

            using Blob blob = Compiler.CompileFromFile(shaderFile, entryPoint, profile);
            return blob.AsSpan();
        }

        private static IDXGIAdapter1 GetHardwareAdapter()
        {
            /*
             * Try to get a high performance hardware adapter 
             * return null if no hardware adapter has been found
             */

            IDXGIAdapter1 adapter = null;
            DXGI.CreateDXGIFactory1<IDXGIFactory6>(out var factory6);
            if (factory6 != null)
            {
                for (int adapterIndex = 0; factory6.EnumAdapterByGpuPreference(adapterIndex, GpuPreference.HighPerformance, out adapter).Success; adapterIndex++)
                {
                    if (adapter == null)
                    {
                        continue;
                    }

                    AdapterDescription1 desc = adapter.Description1;
                    if ((desc.Flags & AdapterFlags.Software) != AdapterFlags.None)
                    {
                        // Don't select the Basic Render Driver adapter.
                        adapter.Dispose();
                        continue;
                    }

                    factory6.Dispose();

                    return adapter;
                }
            }

            if ((adapter == null) && (factory6 != null))
            {
                for (int adapterIndex = 0; factory6.EnumAdapters1(adapterIndex, out adapter).Success; adapterIndex++)
                {
                    AdapterDescription1 desc = adapter.Description1;
                    if ((desc.Flags & AdapterFlags.Software) != AdapterFlags.None)
                    {
                        // Don't select the Basic Render Driver adapter.
                        adapter.Dispose();
                        continue;
                    }

                    factory6.Dispose();

                    return adapter;
                }
            }

            factory6?.Dispose();

            return adapter;
        }

        public void Dispose()
        {
            this.debugInterface.Dispose();

            this.device.Dispose();
            this.deviceContext.Dispose();
            this.swapChain.Dispose();

            this.backBufferTexture.Dispose();
            this.renderTarget2DRight.Dispose();
            this.renderTargetView.Dispose();
            this.depthStencilTexture.Dispose();
            this.depthStencilView.Dispose();

            this.gridBuffer.Dispose();
            this.signalBuffer.Dispose();
            this.constantBuffer.Dispose();
            this.vertexBuffer.Dispose();
            this.indexBuffer.Dispose();

            this.pixelShaderPositionColor.Dispose();
            this.pixelShaderPositionTexture.Dispose();
            this.vertexShaderPositionColor.Dispose();
            this.vertexShaderPositionTexture.Dispose();
        }
    }
}
