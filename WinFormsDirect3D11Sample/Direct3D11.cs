#nullable disable

using System.ComponentModel;
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
using Color = Vortice.Mathematics.Color;
using FeatureLevel = Vortice.Direct3D.FeatureLevel;
using ResultCode = Vortice.DXGI.ResultCode;

namespace WinFormsDirect3D11Sample;

internal unsafe class Direct3D11 : IDisposable
{
    #region Fields and Constants

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
    private ID3D11Buffer constantBuffer2;

    private ID3D11Debug debugInterface;
    private ID3D11ShaderResourceView textureSrv;
    private ID3D11SamplerState textureSampler;

    private TextureManager textureManager;

    private float eyeX;
    private float eyeY;
    private float eyeZ = 2;
    private float atX;
    private float atY;
    private float atZ;
    private float upX;
    private float upY = 1;
    private float upZ;

    private bool drawGrid;
    private bool drawLine;
    private bool drawCube;
    private bool drawText;

    // list of featureLevels this app can support
    private static readonly FeatureLevel[] FeatureLevels = {
        FeatureLevel.Level_12_1,
        FeatureLevel.Level_12_0,
        FeatureLevel.Level_11_1,
        FeatureLevel.Level_11_0,
        FeatureLevel.Level_10_1,
        FeatureLevel.Level_10_0,
        FeatureLevel.Level_9_3,
        FeatureLevel.Level_9_2,
        FeatureLevel.Level_9_1,
    };

    #endregion

    #region Constructors

    public Direct3D11(MainWindow form, Control control, Control control2D)
    {
        this.mainWindow = form;
        this.leftControl = control;
        this.rightControl = control2D;
    }

    #endregion

    #region Properties

    [Category("Camera (Eye)")]
    [DisplayName("x")]
    public float EyeX
    {
        get => this.eyeX;
        set
        {
            this.eyeX = value;
            this.leftControl.Invalidate();
            this.rightControl.Invalidate();
        }
    }

    [Category("Camera (Eye)")]
    [DisplayName("y")]
    public float EyeY
    {
        get => this.eyeY;
        set
        {
            this.eyeY = value;
            this.leftControl.Invalidate();
            this.rightControl.Invalidate();
        }
    }

    [Category("Camera (Eye)")]
    [DisplayName("z")]
    public float EyeZ
    {
        get => this.eyeZ;
        set
        {
            this.eyeZ = value;
            this.leftControl.Invalidate();
            this.rightControl.Invalidate();
        }
    }

    [Category("Camera (At)")]
    [DisplayName("x")]
    public float AtX
    {
        get => this.atX;
        set
        {
            this.atX = value;
            this.leftControl.Invalidate();
            this.rightControl.Invalidate();
        }
    }

    [Category("Camera (At)")]
    [DisplayName("y")]
    public float AtY
    {
        get => this.atY;
        set
        {
            this.atY = value;
            this.leftControl.Invalidate();
            this.rightControl.Invalidate();
        }
    }

    [Category("Camera (At)")]
    [DisplayName("z")]
    public float AtZ
    {
        get => this.atZ;
        set
        {
            this.atZ = value;
            this.leftControl.Invalidate();
            this.rightControl.Invalidate();
        }
    }

    [Category("Camera (Up)")]
    [DisplayName("x")]
    public float UpX
    {
        get => this.upX;
        set
        {
            this.upX = value;
            this.leftControl.Invalidate();
            this.rightControl.Invalidate();
        }
    }

    [Category("Camera (Up)")]
    [DisplayName("y")]
    public float UpY
    {
        get => this.upY;
        set
        {
            this.upY = value;
            this.leftControl.Invalidate();
            this.rightControl.Invalidate();
        }
    }

    [Category("Camera (Up)")]
    [DisplayName("z")]
    public float UpZ
    {
        get => this.upZ;
        set
        {
            this.upZ = value;
            this.leftControl.Invalidate();
            this.rightControl.Invalidate();
        }
    }

    #endregion

    #region Methods

    internal void OnInit()
    {
        /*
         * 1. Try to create a hardware device and device context
         * 2. If that fails, try to create a WARP device (software based)
         * 3. Create a swap chain description and a swap chain fullscreen description
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
#if DEBUG
        if (D3D11.SdkLayersAvailable())
        {
            deviceCreationFlags |= DeviceCreationFlags.Debug;
        }
#endif

        using IDXGIAdapter1 adapter = GetHardwareAdapter();
        if (D3D11.D3D11CreateDevice(null, DriverType.Hardware, deviceCreationFlags, Direct3D11.FeatureLevels,
                out ID3D11Device tempDevice, out FeatureLevel featureLevel, out ID3D11DeviceContext tempContext).Failure)
        {
            // Handle device interface creation failure if it occurs.
            // For example, reduce the feature level requirement, or fail over 
            // to WARP rendering.
            D3D11.D3D11CreateDevice(null, DriverType.Warp, deviceCreationFlags, Direct3D11.FeatureLevels,
                out tempDevice, out featureLevel, out tempContext).CheckError();
        }

        string resolution = "<not available>";
        if (adapter != null && adapter.EnumOutputs(0, out IDXGIOutput output).Success)
        {
            resolution = $"{output.Description.DesktopCoordinates.Right} x {output.Description.DesktopCoordinates.Bottom}";
        }

        highestSupportedFeatureLevel = featureLevel;
        if (adapter != null)
        {
            this.mainWindow.UpdateLabels(adapter.Description1.Description, highestSupportedFeatureLevel.ToString(), resolution);
        }

        device = tempDevice.QueryInterface<ID3D11Device1>();
        deviceContext = tempContext.QueryInterface<ID3D11DeviceContext1>();
        tempContext.Dispose();
        tempDevice.Dispose();

        this.textureManager = new TextureManager(device);

#if DEBUG
        if (D3D11.SdkLayersAvailable())
        {
            debugInterface = device.QueryInterface<ID3D11Debug>();
        }
#endif

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
            SwapEffect = SwapEffect.FlipDiscard
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
        this.constantBuffer2 = device.CreateConstantBuffer<Matrix4x4>();

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

        var texture2D = device.CreateTexture2D(pixels, Format.R8G8B8A8_UNorm, 4, 4);
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

        this.texture = this.textureManager.LoadTexture("10points.png");
        this.textureSrv = device.CreateShaderResourceView(this.texture);
        this.textureSampler = device.CreateSamplerState(SamplerDescription.PointWrap);

        this.clock = Stopwatch.StartNew();
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
            deviceContext.VSSetConstantBuffer(1, this.constantBuffer2);
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
            deviceContext.VSSetConstantBuffer(1, this.constantBuffer2);
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

        if (drawGrid || drawLine)
        {
            Matrix4x4 view2 = Matrix4x4.CreateLookAt(new Vector3(eyeX, eyeY, eyeZ), new Vector3(atX, atY, atZ), new Vector3(upX, upY, upZ));
            var AspectRatio2 = (float)leftControl.ClientSize.Width / leftControl.ClientSize.Height;
            Matrix4x4 projection2 = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4, AspectRatio2, 0.1f, 100);
            Matrix4x4 viewProjection2 = Matrix4x4.Multiply(view2, projection2);

            // Update constant buffer2 data
            MappedSubresource mappedResource2 = deviceContext.Map(this.constantBuffer2, 0, MapMode.WriteDiscard);
            Unsafe.Copy(mappedResource2.DataPointer.ToPointer(), ref viewProjection2);
            deviceContext.Unmap(this.constantBuffer2, 0);
        }

        if (drawCube)
        {
            // update constant buffer for textured cube
            var time = this.clock.ElapsedMilliseconds / 1000.0f;
            Matrix4x4 world = Matrix4x4.CreateRotationX(time) * Matrix4x4.CreateRotationY(time * 2) * Matrix4x4.CreateRotationZ(time * .7f);

            Matrix4x4 view = Matrix4x4.CreateLookAt(new Vector3(0, 0, 25f), new Vector3(0, 0, 0), new Vector3(upX, upY, upZ));
            var AspectRatio = (float)leftControl.ClientSize.Width / leftControl.ClientSize.Height;
            Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4, AspectRatio, 0.1f, 100);
            Matrix4x4 viewProjection = Matrix4x4.Multiply(view, projection);
            Matrix4x4 worldViewProjection = Matrix4x4.Multiply(world, viewProjection);

            // Update constant buffer data
            MappedSubresource mappedResource = deviceContext.Map(this.constantBuffer, 0, MapMode.WriteDiscard);
            Unsafe.Copy(mappedResource.DataPointer.ToPointer(), ref worldViewProjection);
            deviceContext.Unmap(this.constantBuffer, 0);
        }
    }

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
            case "drawAll":
                drawGrid = true;
                drawLine = true;
                drawCube = true;
                drawText = true;
                break;
        }
    }

    private static Span<byte> CompileBytecode(string shaderName, string entryPoint, string profile)
    {
        string assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");
        string shaderFile = Path.Combine(assetsPath, shaderName);

        Compiler.CompileFromFile(shaderFile, entryPoint, profile, out Blob blob, out _);
        return blob.AsBytes();
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

#if DEBUG
        // use to report live object which need to be disposed!
        this.debugInterface?.ReportLiveDeviceObjects(ReportLiveDeviceObjectFlags.Summary);
        this.debugInterface?.Dispose();
        MainWindow.InfoManager?.PrintMessages();
#endif
    }

    #endregion
}