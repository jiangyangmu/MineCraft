#include "pch.h"

#include <sstream>

#include "D3DApp.h"
#include "ErrorHandling.h"

using win32::ENSURE_TRUE;
using win32::ENSURE_OK;
using win32::ENSURE_CLIB_SUCCESS;
using dx::THROW_IF_FAILED;

namespace render
{

void D3DApplication::RegisterGP(ID3DGraphicsPipeline * pGP)
{
    ENSURE_TRUE(
        pGP != nullptr &&
        std::find(m_graphicsPipelines.begin(), m_graphicsPipelines.end(), pGP) == m_graphicsPipelines.end());

    m_graphicsPipelines.push_back(pGP);
}

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
    InitializeD3D();

    // Create resource manager, load external resources.
    PrepareResources();

    // Initialize pipeline data & state.
    InitializeD3DPipeline();

    // Initialize render targets.
    UpdateRenderTargets(GetWidth(), GetHeight());
}

void D3DApplication::UpdateScene(double ms)
{
    UNREFERENCED_PARAMETER(ms);
}

void D3DApplication::DrawScene()
{
    for (ID3DGraphicsPipeline * pGP : m_graphicsPipelines)
    {
        pGP->Draw(m_d3dContext);
    }
}

void D3DApplication::OnIdle()
{
    ClearScreen();

    double ms;
    if (m_timerPreviousValue.QuadPart != 0)
    {
        LARGE_INTEGER delta;
        
        delta           = m_timerPreviousValue;
        
        ENSURE_TRUE(
            QueryPerformanceCounter(&m_timerPreviousValue));
        
        delta.QuadPart  = m_timerPreviousValue.QuadPart - delta.QuadPart;
        delta.QuadPart  *= 1000;
        delta.QuadPart  /= m_timerFrequence.QuadPart;
        
        ms = static_cast<double>(delta.QuadPart);
    }
    else
    {
        ENSURE_TRUE(
            QueryPerformanceCounter(&m_timerPreviousValue));
        
        ms = 1.0;
    }

    m_fps = 0.5 * m_fps + 0.5 * 1000.0 / ms;
    {
        std::wstringstream ss;
        ss.precision(2);
        ss << L"FPS: " << m_fps;
        SetWindowText(GetHWND(), ss.str().c_str());
        Sleep(33);
    }
    
    UpdateScene(ms);
    DrawScene();

    PresentNextFrame();
}

void D3DApplication::OnMove(int x, int y)
{
    UNREFERENCED_PARAMETER(x);
    UNREFERENCED_PARAMETER(y);
}

void D3DApplication::OnResize(int width, int height)
{
    UpdateRenderTargets(width, height);
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

void D3DApplication::PrepareResources()
{

}

void D3DApplication::InitializeD3DPipeline()
{
    float aspectRatio = static_cast<float>(GetWidth()) / static_cast<float>(GetHeight());
    for (ID3DGraphicsPipeline * pGP : m_graphicsPipelines)
    {
        pGP->Prepare(m_d3dDevice,
                     aspectRatio);
    }
}

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
            0,
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

void D3DApplication::PresentNextFrame()
{
    THROW_IF_FAILED(
        m_SwapChain->Present(0, 0));
}

void D3DTriangle::Prepare(ID3D11Device * d3dDevice, float aspectRatio)
{
    m_d3dDevice = d3dDevice;

    // Vertex buffer

    float data[] =
    {
         -1.0f, 0.0f, 0.0f, 1.0f,       1.0f, 0.0f, 0.0f, 1.0f,
          0.0f, 0.867f, 0.0f, 1.0f,     0.0f, 1.0f, 0.0f, 1.0f,
          1.0f, 0.0f, 0.0f, 1.0f,       0.0f, 0.0f, 1.0f, 1.0f,
    };

    D3D11_BUFFER_DESC vertexBufferDesc;
    vertexBufferDesc.Usage                  = D3D11_USAGE_DEFAULT;
    vertexBufferDesc.ByteWidth              = sizeof(data);
    vertexBufferDesc.BindFlags              = D3D11_BIND_VERTEX_BUFFER;
    vertexBufferDesc.CPUAccessFlags         = 0;
    vertexBufferDesc.MiscFlags              = 0;
    vertexBufferDesc.StructureByteStride    = 0;

    D3D11_SUBRESOURCE_DATA vertexData;
    vertexData.pSysMem                      = data;
    vertexData.SysMemPitch                  = 0;
    vertexData.SysMemSlicePitch             = 0;

    ENSURE_OK(
        m_d3dDevice->CreateBuffer(&vertexBufferDesc,
                                  &vertexData,
                                  &m_d3dVertexBuffer));

    // Vertex shader

    LoadCompiledShaderFromFile(TEXT("..\\Debug\\VertexShader.vso"), &m_vertexShaderByteCode);

    ENSURE_OK(
        m_d3dDevice->CreateVertexShader(m_vertexShaderByteCode.pBytes,
                                        m_vertexShaderByteCode.nSize,
                                        nullptr,
                                        &m_d3dVertexShader));

    // Input layout

    D3D11_INPUT_ELEMENT_DESC inputElementDescs[] =
    {
        { "POSITION", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
        { "COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 16, D3D11_INPUT_PER_VERTEX_DATA, 0 },
    };
    ENSURE_OK(
        m_d3dDevice->CreateInputLayout(inputElementDescs,
                                       ARRAYSIZE(inputElementDescs),
                                       m_vertexShaderByteCode.pBytes,
                                       m_vertexShaderByteCode.nSize,
                                       &m_d3dInputLayout));

    // Pixel shader

    LoadCompiledShaderFromFile(TEXT("..\\Debug\\PixelShader.pso"), &m_pixelShaderByteCode);

    ENSURE_OK(
        m_d3dDevice->CreatePixelShader(m_pixelShaderByteCode.pBytes,
                                       m_pixelShaderByteCode.nSize,
                                       nullptr,
                                       &m_d3dPixelShader));

    // Constant buffer

    DirectX::XMVECTOR eye = DirectX::XMVectorSet(0.0f, 0.0f, -5.0f, 0.f);
    DirectX::XMVECTOR at = DirectX::XMVectorSet(0.0f, 0.0f, 0.0f, 0.f);
    DirectX::XMVECTOR up = DirectX::XMVectorSet(0.0f, 1.0f, 0.0f, 0.f);

    DirectX::XMStoreFloat4x4(
        &m_constantBufferData.mvp,
        DirectX::XMMatrixTranspose(
            DirectX::XMMatrixMultiply(
                DirectX::XMMatrixLookAtLH(
                    eye,
                    at,
                    up
                ),
                DirectX::XMMatrixPerspectiveFovLH(
                    DirectX::XMConvertToRadians(70),
                    aspectRatio,
                    0.01f,
                    1000.0f
                )
            )));

    CD3D11_BUFFER_DESC constantBufferDesc(
        sizeof(ConstantBufferStruct),
        D3D11_BIND_CONSTANT_BUFFER
    );

    ENSURE_OK(
        m_d3dDevice->CreateBuffer(&constantBufferDesc,
                                  nullptr,
                                  &m_d3dConstantBuffer));
}

// https://docs.microsoft.com/en-us/windows/win32/direct3dgetstarted/complete-code-sample-for-using-a-corewindow-with-directx
void D3DTriangle::Draw(ID3D11DeviceContext * d3dContext)
{
    // IA(vb, ib) + VS(cb, shader) + RS(viewport) + PS(texture, shader) + OM(render targets)
    m_d3dContext = d3dContext;
    
    // Update constant buffer

    m_d3dContext->UpdateSubresource(m_d3dConstantBuffer,
                                    0,
                                    nullptr,
                                    &m_constantBufferData,
                                    0,
                                    0);

    // Set IA stage

    UINT strides = sizeof(float) * 4 * 2;
    UINT offsets = 0;
    m_d3dContext->IASetVertexBuffers(0, // slot
                                     1, // number of buffers
                                     &m_d3dVertexBuffer,
                                     &strides,
                                     &offsets);
    
    m_d3dContext->IASetPrimitiveTopology(
        D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
    
    m_d3dContext->IASetInputLayout(m_d3dInputLayout);

    // Set VS stage

    m_d3dContext->VSSetShader(m_d3dVertexShader,
                              nullptr,
                              0);

    m_d3dContext->VSSetConstantBuffers(0,
                                       1,
                                       &m_d3dConstantBuffer);

    // Set PS stage

    m_d3dContext->PSSetShader(m_d3dPixelShader,
                              nullptr,
                              0);

    // Draw
    m_d3dContext->Draw(3, 0);
}

void D3DTriangle::LoadCompiledShaderFromFile(const TCHAR * pFileName, ShaderByteCode * pSBC)
{
    FILE *  pFile;
    long    nSize;
    size_t  nReadSize;

    ENSURE_CLIB_SUCCESS(    _wfopen_s(&pFile, pFileName, TEXT("rb"))    );
    
    ENSURE_CLIB_SUCCESS(    fseek(pFile, 0, SEEK_END)                   );
    
    nSize                   = ftell(pFile);
    ENSURE_TRUE(nSize > 0);
    
    ENSURE_CLIB_SUCCESS(    fseek(pFile, 0, SEEK_SET)                   );
    
    pSBC->nSize             = static_cast<size_t>(nSize);
    pSBC->pBytes            = new BYTE[nSize];

    nReadSize               = fread_s(pSBC->pBytes,
                                      pSBC->nSize,
                                      sizeof(BYTE),
                                      pSBC->nSize,
                                      pFile);
    ENSURE_TRUE(nReadSize == pSBC->nSize);
}

}