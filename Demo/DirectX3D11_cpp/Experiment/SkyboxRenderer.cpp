#include "pch.h"

#include "SkyboxRenderer.h"
#include "ErrorHandling.h"
#include "DDSTextureLoader.h"

using namespace win32;
using namespace dx;

namespace render
{

static const float gCubeVertices[] =
{
    -100.0f, -100.0f, -100.0f, 1.0f  ,  1.0f, 0.0f, 0.0f, 1.0f,
    -100.0f,  100.0f, -100.0f, 1.0f  ,  1.0f, 0.0f, 0.0f, 1.0f,
     100.0f,  100.0f, -100.0f, 1.0f  ,  1.0f, 0.0f, 0.0f, 1.0f,
    -100.0f, -100.0f, -100.0f, 1.0f  ,  1.0f, 0.0f, 0.0f, 1.0f,
     100.0f,  100.0f, -100.0f, 1.0f  ,  1.0f, 0.0f, 0.0f, 1.0f,
     100.0f, -100.0f, -100.0f, 1.0f  ,  1.0f, 0.0f, 0.0f, 1.0f,

    -100.0f, -100.0f,  100.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 1.0f,
     100.0f,  100.0f,  100.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 1.0f,
    -100.0f,  100.0f,  100.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 1.0f,
    -100.0f, -100.0f,  100.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 1.0f,
     100.0f, -100.0f,  100.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 1.0f,
     100.0f,  100.0f,  100.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 1.0f,

    -100.0f,  100.0f, -100.0f, 1.0f  ,  0.0f, 0.0f, 1.0f, 1.0f,
    -100.0f,  100.0f,  100.0f, 1.0f  ,  0.0f, 0.0f, 1.0f, 1.0f,
     100.0f,  100.0f,  100.0f, 1.0f  ,  0.0f, 0.0f, 1.0f, 1.0f,
    -100.0f,  100.0f, -100.0f, 1.0f  ,  0.0f, 0.0f, 1.0f, 1.0f,
     100.0f,  100.0f,  100.0f, 1.0f  ,  0.0f, 0.0f, 1.0f, 1.0f,
     100.0f,  100.0f, -100.0f, 1.0f  ,  0.0f, 0.0f, 1.0f, 1.0f,

    -100.0f, -100.0f, -100.0f, 1.0f  ,  1.0f, 1.0f, 0.0f, 1.0f,
     100.0f, -100.0f,  100.0f, 1.0f  ,  1.0f, 1.0f, 0.0f, 1.0f,
    -100.0f, -100.0f,  100.0f, 1.0f  ,  1.0f, 1.0f, 0.0f, 1.0f,
    -100.0f, -100.0f, -100.0f, 1.0f  ,  1.0f, 1.0f, 0.0f, 1.0f,
     100.0f, -100.0f, -100.0f, 1.0f  ,  1.0f, 1.0f, 0.0f, 1.0f,
     100.0f, -100.0f,  100.0f, 1.0f  ,  1.0f, 1.0f, 0.0f, 1.0f,

    -100.0f, -100.0f, -100.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,
    -100.0f, -100.0f,  100.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,
    -100.0f,  100.0f,  100.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,
    -100.0f, -100.0f, -100.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,
    -100.0f,  100.0f,  100.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,
    -100.0f,  100.0f, -100.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,

     100.0f, -100.0f, -100.0f, 1.0f ,  0.0f, 1.0f, 1.0f, 1.0f,
     100.0f,  100.0f,  100.0f, 1.0f ,  0.0f, 1.0f, 1.0f, 1.0f,
     100.0f, -100.0f,  100.0f, 1.0f ,  0.0f, 1.0f, 1.0f, 1.0f,
     100.0f, -100.0f, -100.0f, 1.0f ,  0.0f, 1.0f, 1.0f, 1.0f,
     100.0f,  100.0f, -100.0f, 1.0f ,  0.0f, 1.0f, 1.0f, 1.0f,
     100.0f,  100.0f,  100.0f, 1.0f ,  0.0f, 1.0f, 1.0f, 1.0f,
};

void SkyboxRenderer::Initialize(ID3D11Device * d3dDevice, float aspectRatio)
{
    UNREFERENCED_PARAMETER(aspectRatio);

    m_d3dDevice = d3dDevice;

    // Cube map
    THROW_IF_FAILED(
        DirectX::CreateDDSTextureFromFile(m_d3dDevice,
                                          TEXT("Skybox.dds"),
                                          nullptr,
                                          &m_d3dCubeMapSRV));

    // Vertex buffer

    m_vertexBuffer.reset(new D3DConstantVertexBuffer(m_d3dDevice));
    m_vertexBuffer->Reset(gCubeVertices,
                          sizeof(float) * ARRAYSIZE(gCubeVertices));

     // Vertex shader

    LoadCompiledShaderFromFile(TEXT("..\\Debug\\SkyboxVS.vso"), &m_vertexShaderByteCode);

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

    LoadCompiledShaderFromFile(TEXT("..\\Debug\\SkyboxPS.pso"), &m_pixelShaderByteCode);

    ENSURE_OK(
        m_d3dDevice->CreatePixelShader(m_pixelShaderByteCode.pBytes,
                                       m_pixelShaderByteCode.nSize,
                                       nullptr,
                                       &m_d3dPixelShader));

    // States

    D3D11_RASTERIZER_DESC noCullingRSDesc;
    noCullingRSDesc.FillMode = D3D11_FILL_SOLID;
    noCullingRSDesc.CullMode = D3D11_CULL_NONE;
    noCullingRSDesc.FrontCounterClockwise = FALSE;
    noCullingRSDesc.DepthBias = 0;
    noCullingRSDesc.SlopeScaledDepthBias = 0.0f;
    noCullingRSDesc.DepthClipEnable = TRUE;
    noCullingRSDesc.ScissorEnable = FALSE;
    noCullingRSDesc.MultisampleEnable = FALSE;
    noCullingRSDesc.AntialiasedLineEnable = FALSE;

    ENSURE_OK(
        m_d3dDevice->CreateRasterizerState(&noCullingRSDesc,
                                           &m_rasterizerState));

    D3D11_SAMPLER_DESC samplerStateDesc;
    ZeroMemory(&samplerStateDesc, sizeof(samplerStateDesc));
    samplerStateDesc.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
    samplerStateDesc.AddressU = D3D11_TEXTURE_ADDRESS_WRAP;
    samplerStateDesc.AddressV = D3D11_TEXTURE_ADDRESS_WRAP;
    samplerStateDesc.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
    samplerStateDesc.ComparisonFunc = D3D11_COMPARISON_NEVER;
    samplerStateDesc.MinLOD = 0;
    samplerStateDesc.MaxLOD = D3D11_FLOAT32_MAX;

    ENSURE_OK(
        m_d3dDevice->CreateSamplerState(&samplerStateDesc,
                                        &m_samplerState));

    D3D11_DEPTH_STENCIL_DESC lessEqualDSDesc;
    ZeroMemory(&lessEqualDSDesc, sizeof(D3D11_DEPTH_STENCIL_DESC));
    lessEqualDSDesc.DepthEnable = true;
    lessEqualDSDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
    lessEqualDSDesc.DepthFunc = D3D11_COMPARISON_LESS_EQUAL;

    ENSURE_OK(
        m_d3dDevice->CreateDepthStencilState(&lessEqualDSDesc,
                                             &m_depthStencilState));
}

void SkyboxRenderer::Update(double milliSeconds)
{
    UNREFERENCED_PARAMETER(milliSeconds);
}

void SkyboxRenderer::Draw(ID3D11DeviceContext * d3dContext)
{
    m_d3dContext = d3dContext;

    // Set IA stage

    ID3D11Buffer * buffers  = m_vertexBuffer->Get();
    UINT strides            = sizeof(float) * 4 * 2;
    UINT offsets            = 0;

    m_d3dContext->IASetVertexBuffers(0, // slot
                                     1, // number of buffers
                                     &buffers,
                                     &strides,
                                     &offsets);

    m_d3dContext->IASetPrimitiveTopology(
        D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

    m_d3dContext->IASetInputLayout(m_d3dInputLayout);

    // Set VS stage

    m_d3dContext->VSSetShader(m_d3dVertexShader,
                              nullptr,
                              0);

    // Set PS stage

    m_d3dContext->PSSetShader(m_d3dPixelShader,
                              nullptr,
                              0);

    m_d3dContext->PSSetSamplers(0,
                                1,
                                &m_samplerState);
    m_d3dContext->PSSetShaderResources(0,
                                       1,
                                       &m_d3dCubeMapSRV);

    // Set RS stage

    m_d3dContext->RSSetState(m_rasterizerState);

    // Set OM stage

    m_d3dContext->OMSetDepthStencilState(m_depthStencilState,
                                         0);

    // Draw
    m_d3dContext->Draw(36, 0);
}

}