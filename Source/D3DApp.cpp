#include "pch.h"

#include <sstream>

#include "D3DApp.h"

using win32::ENSURE_TRUE;
using win32::ENSURE_OK;
using win32::ENSURE_CLIB_SUCCESS;
using dx::THROW_IF_FAILED;

namespace render
{

D3DApplication::D3DApplication(LPCWSTR lpTitle, HINSTANCE hInstance)
    : m_mainWnd(lpTitle, hInstance)
{
    _BIND_EVENT(OnWndIdle, m_mainWnd, *this);
    _BIND_EVENT(OnWndMove, m_mainWnd, *this);
    _BIND_EVENT(OnWndResize, m_mainWnd, *this);
}

// Initialization
void D3DApplication::Initialize()
{
    // Test DirectXMath support

    if (!DirectX::XMVerifyCPUSupport())
        throw new std::exception("Missing DirectXMath support.");

    // Initialize timer.
    
    ENSURE_TRUE(
        QueryPerformanceFrequency(&m_timerFrequence));
    m_timerPreviousValue.QuadPart = 0;
    m_fps = 0.0;

    // Initialize DirectX.

    m_d3dDevice = NULL;
    m_d3dContext = NULL;
    m_SwapChain = NULL;
    m_RenderTargetView = NULL;
    m_DepthStencilView = NULL;
    m_d2dContext = NULL;

    // Create device and swapChain.
    InitializeD3DDevice();

    // Initialize pipeline data & state.
    InitializeD3DPipelines();

    // Initialize render targets.
    UpdateRenderTargets(m_mainWnd.GetWidth(), m_mainWnd.GetHeight());
}
void D3DApplication::InitializeD3DDevice()
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
    swapChainDesc.BufferDesc.Width = m_mainWnd.GetWidth();
    swapChainDesc.BufferDesc.Height = m_mainWnd.GetHeight();
    swapChainDesc.BufferDesc.RefreshRate.Numerator = 60;
    swapChainDesc.BufferDesc.RefreshRate.Denominator = 1;
    swapChainDesc.BufferDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM; // Required to work with DirectX 2D
    swapChainDesc.BufferDesc.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
    swapChainDesc.BufferDesc.Scaling = DXGI_MODE_SCALING_UNSPECIFIED;
    swapChainDesc.SampleDesc.Count = 1;
    swapChainDesc.SampleDesc.Quality = 0;
    swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    swapChainDesc.BufferCount = 1;
    swapChainDesc.OutputWindow = m_mainWnd.GetHWND();
    swapChainDesc.Windowed = true;
    swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
    swapChainDesc.Flags = 0;

    THROW_IF_FAILED(
        D3D11CreateDevice(
            dxgiAdapter,
            D3D_DRIVER_TYPE_UNKNOWN,
            0,
            // D3D11_CREATE_DEVICE_BGRA_SUPPORT - Required to work with DirectX 2D
            // D3D11_CREATE_DEVICE_DEBUG - For debug
            D3D11_CREATE_DEVICE_BGRA_SUPPORT, 
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
void D3DApplication::RegisterRenderer(IRenderer * pRender)
{
    ENSURE_TRUE(
        pRender != nullptr &&
        std::find(m_renders.begin(), m_renders.end(), pRender) == m_renders.end());

    m_renders.push_back(pRender);
}
void D3DApplication::InitializeD3DPipelines()
{
    float aspectRatio = m_mainWnd.GetAspectRatio();
    
    for (ID3DPipelineModule * pGP : m_renders)
    {
        pGP->Initialize(m_d3dDevice, aspectRatio);
    }
}

// Per frame
void D3DApplication::ClearScreen()
{
    m_d3dContext->ClearDepthStencilView(
        m_DepthStencilView,
        D3D11_CLEAR_DEPTH | D3D11_CLEAR_STENCIL,
        1.0f,
        0
    );

    DirectX::XMFLOAT4 Black(0.0f, 0.0f, 0.0f, 1.0f);
    m_d3dContext->ClearRenderTargetView(
        m_RenderTargetView,
        reinterpret_cast<float *>(&Black)
    );
}
void D3DApplication::UpdateScene(double milliSeconds)
{
    for (IRenderer * pRender : m_renders)
    {
        pRender->Update(milliSeconds);
    }
}
void D3DApplication::DrawScene()
{
    for (ID3DPipelineModule * pGP : m_renders)
    {
        pGP->Draw(m_d3dContext);
    }
}
void D3DApplication::PresentNextFrame()
{
    THROW_IF_FAILED(
        m_SwapChain->Present(0, 0));
}

// Resizing
void D3DApplication::UpdateRenderTargets(int width, int height)
{
    const int W = width;
    const int H = height;

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
            nullptr,
            &m_RenderTargetView));
    backBuffer->Release();

    ID3D11Texture2D * depthStencilBuffer;
    D3D11_TEXTURE2D_DESC depthStencilDesc;
    depthStencilDesc.Width = W;
    depthStencilDesc.Height = H;
    depthStencilDesc.MipLevels = 1;
    depthStencilDesc.ArraySize = 1;
    depthStencilDesc.Format = DXGI_FORMAT_D32_FLOAT_S8X24_UINT;
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

    // Set render targets

    m_d3dContext->OMSetRenderTargets(1,
                                     &m_RenderTargetView,
                                     m_DepthStencilView);

    // D2D: context

    m_d2dContext = nullptr;
}
void D3DApplication::SetFullScreen(bool isFullScreen)
{
    THROW_IF_FAILED(
        m_SwapChain->SetFullscreenState(isFullScreen, NULL));
}

// On win32 events
_RECV_EVENT_IMPL(D3DApplication, OnWndIdle)
(void * sender)
{
    UNREFERENCED_PARAMETER(sender);

    ClearScreen();

    double milliSeconds;
    if (m_timerPreviousValue.QuadPart != 0)
    {
        LARGE_INTEGER delta;
        
        delta           = m_timerPreviousValue;
        
        ENSURE_TRUE(
            QueryPerformanceCounter(&m_timerPreviousValue));
        
        delta.QuadPart  = m_timerPreviousValue.QuadPart - delta.QuadPart;
        delta.QuadPart  *= 1000;
        delta.QuadPart  /= m_timerFrequence.QuadPart;
        
        milliSeconds = static_cast<double>(delta.QuadPart);
    }
    else
    {
        ENSURE_TRUE(
            QueryPerformanceCounter(&m_timerPreviousValue));
        
        milliSeconds = 16.0;
    }

    m_fps = 0.5 * m_fps + 0.5 * 1000.0 / milliSeconds;
    {
        std::wstringstream ss;
        ss.precision(2);
        ss << L"FPS: " << m_fps << " ms: " << milliSeconds;
        SetWindowText(m_mainWnd.GetHWND(), ss.str().c_str());
        Sleep(static_cast<DWORD>(std::max(0.0, 16.0 - milliSeconds)));
    }
    
    UpdateScene(milliSeconds);
    DrawScene();

    PresentNextFrame();
}
_RECV_EVENT_IMPL(D3DApplication, OnWndMove)
(void * sender, const win32::WindowRect & rect)
{
    UNREFERENCED_PARAMETER(sender);
    UNREFERENCED_PARAMETER(rect);
}
_RECV_EVENT_IMPL(D3DApplication, OnWndResize)
(void * sender, const win32::WindowRect & rect)
{
    UNREFERENCED_PARAMETER(sender);

    UpdateRenderTargets(rect.width, rect.height);

    float aspectRatio = static_cast<float>(rect.width) / static_cast<float>(rect.height);
    _DISPATCH_EVENT1(OnAspectRatioChange, *this, aspectRatio);
}


}