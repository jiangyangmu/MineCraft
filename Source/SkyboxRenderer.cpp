#include "pch.h"

#include "SkyboxRenderer.h"
#include "DDSTextureLoader.h"
#include "Sphere.h"

using namespace win32;
using namespace dx;
using namespace DirectX;

// #define SKYBOX_USE_SPHERE

namespace render
{

#ifndef SKYBOX_USE_SPHERE

constexpr float gN = 0.577350269f; // 1 / sqrt(3)
static const float gCubeVertices[] =
{
     100.0f, -100.0f, -100.0f, 1.0f,     gN, -gN, -gN, 1.0f,
     100.0f,  100.0f,  100.0f, 1.0f,     gN,  gN,  gN, 1.0f,
     100.0f, -100.0f,  100.0f, 1.0f,     gN, -gN,  gN, 1.0f,
     100.0f, -100.0f, -100.0f, 1.0f,     gN, -gN, -gN, 1.0f,
     100.0f,  100.0f, -100.0f, 1.0f,     gN,  gN, -gN, 1.0f,
     100.0f,  100.0f,  100.0f, 1.0f,     gN,  gN,  gN, 1.0f,

    -100.0f, -100.0f, -100.0f, 1.0f,    -gN, -gN, -gN, 1.0f,
    -100.0f, -100.0f,  100.0f, 1.0f,    -gN, -gN,  gN, 1.0f,
    -100.0f,  100.0f,  100.0f, 1.0f,    -gN,  gN,  gN, 1.0f,
    -100.0f, -100.0f, -100.0f, 1.0f,    -gN, -gN, -gN, 1.0f,
    -100.0f,  100.0f,  100.0f, 1.0f,    -gN,  gN,  gN, 1.0f,
    -100.0f,  100.0f, -100.0f, 1.0f,    -gN,  gN, -gN, 1.0f,

    -100.0f,  100.0f, -100.0f, 1.0f,    -gN,  gN, -gN, 1.0f,
    -100.0f,  100.0f,  100.0f, 1.0f,    -gN,  gN,  gN, 1.0f,
     100.0f,  100.0f,  100.0f, 1.0f,     gN,  gN,  gN, 1.0f,
    -100.0f,  100.0f, -100.0f, 1.0f,    -gN,  gN, -gN, 1.0f,
     100.0f,  100.0f,  100.0f, 1.0f,     gN,  gN,  gN, 1.0f,
     100.0f,  100.0f, -100.0f, 1.0f,     gN,  gN, -gN, 1.0f,

    -100.0f, -100.0f, -100.0f, 1.0f,    -gN, -gN, -gN, 1.0f,
     100.0f, -100.0f,  100.0f, 1.0f,     gN, -gN,  gN, 1.0f,
    -100.0f, -100.0f,  100.0f, 1.0f,    -gN, -gN,  gN, 1.0f,
    -100.0f, -100.0f, -100.0f, 1.0f,    -gN, -gN, -gN, 1.0f,
     100.0f, -100.0f, -100.0f, 1.0f,     gN, -gN, -gN, 1.0f,
     100.0f, -100.0f,  100.0f, 1.0f,     gN, -gN,  gN, 1.0f,

    -100.0f, -100.0f,  100.0f, 1.0f,    -gN, -gN,  gN, 1.0f,
     100.0f,  100.0f,  100.0f, 1.0f,     gN,  gN,  gN, 1.0f,
    -100.0f,  100.0f,  100.0f, 1.0f,    -gN,  gN,  gN, 1.0f,
    -100.0f, -100.0f,  100.0f, 1.0f,    -gN, -gN,  gN, 1.0f,
     100.0f, -100.0f,  100.0f, 1.0f,     gN, -gN,  gN, 1.0f,
     100.0f,  100.0f,  100.0f, 1.0f,     gN,  gN,  gN, 1.0f,

    -100.0f, -100.0f, -100.0f, 1.0f,    -gN, -gN, -gN, 1.0f,
    -100.0f,  100.0f, -100.0f, 1.0f,    -gN,  gN, -gN, 1.0f,
     100.0f,  100.0f, -100.0f, 1.0f,     gN,  gN, -gN, 1.0f,
    -100.0f, -100.0f, -100.0f, 1.0f,    -gN, -gN, -gN, 1.0f,
     100.0f,  100.0f, -100.0f, 1.0f,     gN,  gN, -gN, 1.0f,
     100.0f, -100.0f, -100.0f, 1.0f,     gN, -gN, -gN, 1.0f,
};

#endif

void SkyboxRenderer::Initialize(ID3D11Device * d3dDevice, float aspectRatio)
{
    UNREFERENCED_PARAMETER(aspectRatio);

    m_d3dDevice = d3dDevice;

    // Cube map
    THROW_IF_FAILED(
        DirectX::CreateDDSTextureFromFile(m_d3dDevice,
                                          STR_SKYBOX_DDS,
                                          nullptr,
                                          &m_d3dCubeMapSRV));

    // Vertex buffer

#ifndef SKYBOX_USE_SPHERE
    m_vertexBuffer.reset(new D3DConstantVertexBuffer(m_d3dDevice));
    m_vertexBuffer->Reset(gCubeVertices,
                          sizeof(float) * ARRAYSIZE(gCubeVertices));
#else
    Sphere sphere;
    CreateSphere(100.0f, 10, &sphere);

    m_vertexBuffer.reset(new D3DConstantVertexBuffer(m_d3dDevice));
    m_vertexBuffer->Reset(sphere.vertices.data(),
                          sizeof(sphere.vertices[0]) * sphere.vertices.size());
    m_indexBuffer.reset(new D3DConstantIndexBuffer(m_d3dDevice));
    m_indexBuffer->Reset(sphere.indices.data(),
                         sizeof(sphere.indices[0]) * sphere.indices.size());
    m_indexSize = sphere.indices.size();
#endif

     // Vertex shader

    LoadCompiledShaderFromFile(STR_SKYBOXVS_VSO, &m_vertexShaderByteCode);

    ENSURE_OK(
        m_d3dDevice->CreateVertexShader(m_vertexShaderByteCode.pBytes,
                                        m_vertexShaderByteCode.nSize,
                                        nullptr,
                                        &m_d3dVertexShader));

    // Input layout

    D3D11_INPUT_ELEMENT_DESC inputElementDescs[] =
    {
        { "POSITION", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0,  0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
        { "NORMAL",   0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 16, D3D11_INPUT_PER_VERTEX_DATA, 0 },
    };
    ENSURE_OK(
        m_d3dDevice->CreateInputLayout(inputElementDescs,
                                       ARRAYSIZE(inputElementDescs),
                                       m_vertexShaderByteCode.pBytes,
                                       m_vertexShaderByteCode.nSize,
                                       &m_d3dInputLayout));

    // Pixel shader

    LoadCompiledShaderFromFile(STR_SKYBOXPS_PSO, &m_pixelShaderByteCode);

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

#ifdef SKYBOX_USE_SPHERE
    m_d3dContext->IASetIndexBuffer(m_indexBuffer->Get(),
                                   DXGI_FORMAT_R32_UINT,
                                   0);
#endif

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
#ifndef SKYBOX_USE_SPHERE
    m_d3dContext->Draw(36, 0);
#else
    m_d3dContext->DrawIndexed(m_indexSize, 0, 0);
#endif
}

}