#include "pch.h"

#include "D3DApp.h"

namespace render
{

void DXExitIfError(HRESULT hr)
{
    if (FAILED(hr))
    {
        // ErrorExit(L"DirectX");
        DX::ThrowIfFailed(hr);
    }
}

void D3DApplication::Initialize()
{
    // Test DirectXMath support

    if (!DirectX::XMVerifyCPUSupport())
        throw new std::exception("Missing DirectXMath support.");

    // Create device

    D3D_FEATURE_LEVEL       featureLevel;
    ID3D11Device *          md3dDevice;
    ID3D11DeviceContext *   md3dImmediateContext;

    DXExitIfError(
        D3D11CreateDevice(
            0, // default adapter
            D3D_DRIVER_TYPE_HARDWARE,
            0, // no software device
            0, // D3D11_CREATE_DEVICE_DEBUG
            0, 0, // default feature level array
            D3D11_SDK_VERSION,
            &md3dDevice,
            &featureLevel,
            &md3dImmediateContext));

    // Create swapchain

    IDXGIDevice* dxgiDevice = 0;

    DXExitIfError(
        md3dDevice->QueryInterface(
            __uuidof(IDXGIDevice),
            (void**)&dxgiDevice));

    IDXGIAdapter* dxgiAdapter = 0;

    DXExitIfError(
        dxgiDevice->GetParent(
            __uuidof(IDXGIAdapter),
            (void**)&dxgiAdapter));

    IDXGIFactory* dxgiFactory = 0;

    DXExitIfError(
        dxgiAdapter->GetParent(
            __uuidof(IDXGIFactory),
            (void**)&dxgiFactory));

    DXGI_SWAP_CHAIN_DESC sd;

    sd.BufferDesc.Width = GetWidth();
    sd.BufferDesc.Height = GetHeight();
    sd.BufferDesc.RefreshRate.Numerator = 60;
    sd.BufferDesc.RefreshRate.Denominator = 1;
    sd.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    sd.BufferDesc.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
    sd.BufferDesc.Scaling = DXGI_MODE_SCALING_UNSPECIFIED;
    sd.SampleDesc.Count = 1;
    sd.SampleDesc.Quality = 0;
    sd.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    sd.BufferCount = 1;
    sd.OutputWindow = GetHWND();
    sd.Windowed = true;
    sd.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
    sd.Flags = 0;

    IDXGISwapChain * mSwapChain;

    DXExitIfError(
        dxgiFactory->CreateSwapChain(
            md3dDevice,
            &sd,
            &mSwapChain));

    dxgiDevice->Release();
    dxgiAdapter->Release();
    dxgiFactory->Release();

    // Create render view

    ID3D11RenderTargetView * mRenderTargetView;
    ID3D11Texture2D * backBuffer;

    DXExitIfError(
        mSwapChain->GetBuffer(
            0,
            __uuidof(ID3D11Texture2D),
            reinterpret_cast<void**>(&backBuffer)));

    DXExitIfError(
        md3dDevice->CreateRenderTargetView(
            backBuffer,
            0,
            &mRenderTargetView));

    backBuffer->Release();

    // Create depth stencil view

    D3D11_TEXTURE2D_DESC depthStencilDesc;

    depthStencilDesc.Width = GetWidth();
    depthStencilDesc.Height = GetHeight();
    depthStencilDesc.MipLevels = 1;
    depthStencilDesc.ArraySize = 1;
    depthStencilDesc.Format = DXGI_FORMAT_D24_UNORM_S8_UINT;
    depthStencilDesc.SampleDesc.Count = 1;
    depthStencilDesc.SampleDesc.Quality = 0;
    depthStencilDesc.Usage = D3D11_USAGE_DEFAULT;
    depthStencilDesc.BindFlags = D3D11_BIND_DEPTH_STENCIL;
    depthStencilDesc.CPUAccessFlags = 0;
    depthStencilDesc.MiscFlags = 0;

    ID3D11Texture2D* mDepthStencilBuffer;

    DXExitIfError(
        md3dDevice->CreateTexture2D(
            &depthStencilDesc,
            0,
            &mDepthStencilBuffer));

    ID3D11DepthStencilView* mDepthStencilView;

    DXExitIfError(
        md3dDevice->CreateDepthStencilView(
            mDepthStencilBuffer,
            0,
            &mDepthStencilView));

    mDepthStencilBuffer->Release();

    // Set viewport

    D3D11_VIEWPORT vp;
    vp.TopLeftX = 0.0f;
    vp.TopLeftY = 0.0f;
    vp.Width = static_cast<float>(GetWidth());
    vp.Height = static_cast<float>(GetHeight());
    vp.MinDepth = 0.0f;
    vp.MaxDepth = 1.0f;

    md3dImmediateContext->RSSetViewports(1, &vp);

    m_d3dDevice = md3dDevice;
    m_d3dContext = md3dImmediateContext;
    m_SwapChain = mSwapChain;
    m_RenderTargetView = mRenderTargetView;
    m_DepthStencilView = mDepthStencilView;
    m_d2dContext = nullptr;
}

void D3DApplication::OnIdle()
{
    ClearScreen();
    PresentNextFrame();
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
    DXExitIfError(
        m_SwapChain->Present(0, 0));
}

}