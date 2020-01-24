#include "pch.h"

#include "D3DApp.h"
#include "ErrorHandling.h"

using dx::THROW_IF_FAILED;

namespace render
{

void D3DApplication::Initialize()
{
    // Test DirectXMath support

    if (!DirectX::XMVerifyCPUSupport())
        throw new std::exception("Missing DirectXMath support.");

    m_d3dDevice = NULL;
    m_d3dContext = NULL;
    m_SwapChain = NULL;
    m_RenderTargetView = NULL;
    m_DepthStencilView = NULL;
    m_d2dContext = NULL;

    // Create device and swapChain.
    InitializeD3D();

    // Create resource manager, load external resources.
    PrepareResources();

    // Initialize pipeline data & state.
    InitializeD3DPipeline();

    // Initialize render targets.
    UpdateRenderTargets();
}

void D3DApplication::OnIdle()
{
    ClearScreen();
    PresentNextFrame();
}

void D3DApplication::OnMove(int x, int y)
{
    UNREFERENCED_PARAMETER(x);
    UNREFERENCED_PARAMETER(y);
}

void D3DApplication::OnResize(int width, int height)
{
    UNREFERENCED_PARAMETER(width);
    UNREFERENCED_PARAMETER(height);
}

void D3DApplication::InitializeD3D()
{
     // Create device & swapchain

    IDXGIFactory1 *         dxgiFactory;
    IDXGIAdapter1 *         dxgiAdapter;

    THROW_IF_FAILED(
        CreateDXGIFactory1(
            __uuidof(IDXGIFactory1),
            (void**)&dxgiFactory));

    // 0: Intel 1: Nvidia 2: CPU
    THROW_IF_FAILED(
        dxgiFactory->EnumAdapters1(
            0,
            &dxgiAdapter));

    ID3D11Device *          d3dDevice;
    ID3D11DeviceContext *   d3dImmediateContext;
    IDXGISwapChain *        dxgiSwapChain;

    // Swapchain defines
    // * display mode (resolution, refresh rate, format, scanline, scale)
    // * surface format
    DXGI_SWAP_CHAIN_DESC swapChainDesc;
    swapChainDesc.BufferDesc.Width = GetWidth();
    swapChainDesc.BufferDesc.Height = GetHeight();
    swapChainDesc.BufferDesc.RefreshRate.Numerator = 60;
    swapChainDesc.BufferDesc.RefreshRate.Denominator = 1;
    swapChainDesc.BufferDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM; // Required to work with DirectX 2D
    swapChainDesc.BufferDesc.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
    swapChainDesc.BufferDesc.Scaling = DXGI_MODE_SCALING_UNSPECIFIED;
    swapChainDesc.SampleDesc.Count = 1;
    swapChainDesc.SampleDesc.Quality = 0;
    swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    swapChainDesc.BufferCount = 1;
    swapChainDesc.OutputWindow = GetHWND();
    swapChainDesc.Windowed = true;
    swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
    swapChainDesc.Flags = 0;

    THROW_IF_FAILED(
        D3D11CreateDevice(
            dxgiAdapter,
            D3D_DRIVER_TYPE_UNKNOWN,
            0,
            D3D11_CREATE_DEVICE_BGRA_SUPPORT, // D3D11_CREATE_DEVICE_BGRA_SUPPORT - Required to work with DirectX 2D
                                              // D3D11_CREATE_DEVICE_DEBUG - For debug
            NULL,
            0,
            D3D11_SDK_VERSION,
            &d3dDevice,
            NULL,
            &d3dImmediateContext));
    THROW_IF_FAILED(
        dxgiFactory->CreateSwapChain(
            d3dDevice,
            &swapChainDesc,
            &dxgiSwapChain));

    dxgiAdapter->Release();
    dxgiFactory->Release();

    m_d3dDevice = d3dDevice;
    m_d3dContext = d3dImmediateContext;
    m_SwapChain = dxgiSwapChain;
}

void D3DApplication::PrepareResources()
{

}

void D3DApplication::InitializeD3DPipeline()
{
    // Setup graphics pipeline
    // 0. Prepare buffers
    // 1. IA, VS, PS stage: Prepare shaders, bind buffers
}

void D3DApplication::UpdateRenderTargets()
{
    const int W = GetWidth();
    const int H = GetHeight();

    // Dispose D3D & D2D render target objects.

    if (m_RenderTargetView) m_RenderTargetView->Release();
    if (m_DepthStencilView) m_DepthStencilView->Release();
    if (m_d2dContext)       m_d2dContext->Release();

    // D3D: swapChain, viewport, renderTargetView, depthBuffer, depthStencilView

    THROW_IF_FAILED(
        m_SwapChain->ResizeBuffers(1, W, H, DXGI_FORMAT_UNKNOWN, 0));

    D3D11_VIEWPORT viewport;
    viewport.TopLeftX = 0.0f;
    viewport.TopLeftY = 0.0f;
    viewport.Width = static_cast<float>(W);
    viewport.Height = static_cast<float>(H);
    viewport.MinDepth = 0.0f;
    viewport.MaxDepth = 1.0f;
    m_d3dContext->RSSetViewports(1, &viewport);

    ID3D11Texture2D * backBuffer;
    THROW_IF_FAILED(
        m_SwapChain->GetBuffer(
            0,
            __uuidof(ID3D11Texture2D),
            reinterpret_cast<void**>(&backBuffer)));
    THROW_IF_FAILED(
        m_d3dDevice->CreateRenderTargetView(
            backBuffer,
            0,
            &m_RenderTargetView));
    backBuffer->Release();

    ID3D11Texture2D * depthStencilBuffer;
    D3D11_TEXTURE2D_DESC depthStencilDesc;
    depthStencilDesc.Width = W;
    depthStencilDesc.Height = H;
    depthStencilDesc.MipLevels = 1;
    depthStencilDesc.ArraySize = 1;
    depthStencilDesc.Format = DXGI_FORMAT_D32_FLOAT_S8X24_UINT; // DXGI_FORMAT_D24_UNORM_S8_UINT;
    depthStencilDesc.SampleDesc.Count = 1;
    depthStencilDesc.SampleDesc.Quality = 0;
    depthStencilDesc.Usage = D3D11_USAGE_DEFAULT;
    depthStencilDesc.BindFlags = D3D11_BIND_DEPTH_STENCIL;
    depthStencilDesc.CPUAccessFlags = 0;
    depthStencilDesc.MiscFlags = 0;
    THROW_IF_FAILED(
        m_d3dDevice->CreateTexture2D(
            &depthStencilDesc,
            0,
            &depthStencilBuffer));
    THROW_IF_FAILED(
        m_d3dDevice->CreateDepthStencilView(
            depthStencilBuffer,
            0,
            &m_DepthStencilView));
    depthStencilBuffer->Release();

    // D2D: context

    m_d2dContext = nullptr;
}

void D3DApplication::SetFullScreen(bool isFullScreen)
{
    THROW_IF_FAILED(
        m_SwapChain->SetFullscreenState(isFullScreen, NULL));
}

void D3DApplication::ClearScreen()
{
    m_d3dContext->ClearDepthStencilView(
        m_DepthStencilView,
        D3D11_CLEAR_DEPTH,
        1.0f,
        0
    );
    DirectX::XMFLOAT4 Black(0.0f, 0.0f, 0.0f, 0.0f);
    m_d3dContext->ClearRenderTargetView(
        m_RenderTargetView,
        reinterpret_cast<float *>(&Black)
    );
}

void D3DApplication::PresentNextFrame()
{
    THROW_IF_FAILED(
        m_SwapChain->Present(0, 0));
}

}