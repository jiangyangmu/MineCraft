#include "pch.h"

#include "CubeRenderer.h"
#include "ErrorHandling.h"
#include "DDSTextureLoader.h"

using namespace win32;
using namespace dx;

namespace render
{

static const float gCubeVertices[] =
{
    // up
    -1.0f, -1.0f,  1.0f, 1.0f  ,  0.5f, 1.0f, 0.0f, 0.0f,
     1.0f,  1.0f,  1.0f, 1.0f  ,  1.0f, 0.0f, 0.0f, 0.0f,
    -1.0f,  1.0f,  1.0f, 1.0f  ,  0.5f, 0.0f, 0.0f, 0.0f,
    -1.0f, -1.0f,  1.0f, 1.0f  ,  0.5f, 1.0f, 0.0f, 0.0f,
     1.0f, -1.0f,  1.0f, 1.0f  ,  1.0f, 1.0f, 0.0f, 0.0f,
     1.0f,  1.0f,  1.0f, 1.0f  ,  1.0f, 0.0f, 0.0f, 0.0f,

    // down
    -1.0f, -1.0f, -1.0f, 1.0f  ,  0.5f, 1.0f, 0.0f, 0.0f,
    -1.0f,  1.0f, -1.0f, 1.0f  ,  0.5f, 0.0f, 0.0f, 0.0f,
     1.0f,  1.0f, -1.0f, 1.0f  ,  1.0f, 0.0f, 0.0f, 0.0f,
    -1.0f, -1.0f, -1.0f, 1.0f  ,  0.5f, 1.0f, 0.0f, 0.0f,
     1.0f,  1.0f, -1.0f, 1.0f  ,  1.0f, 0.0f, 0.0f, 0.0f,
     1.0f, -1.0f, -1.0f, 1.0f  ,  1.0f, 1.0f, 0.0f, 0.0f,

    // front
     1.0f, -1.0f, -1.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 0.0f,
     1.0f,  1.0f,  1.0f, 1.0f  ,  0.5f, 0.0f, 0.0f, 0.0f,
     1.0f, -1.0f,  1.0f, 1.0f  ,  0.0f, 0.0f, 0.0f, 0.0f,
     1.0f, -1.0f, -1.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 0.0f,
     1.0f,  1.0f, -1.0f, 1.0f  ,  0.5f, 1.0f, 0.0f, 0.0f,
     1.0f,  1.0f,  1.0f, 1.0f  ,  0.5f, 0.0f, 0.0f, 0.0f,

    // back
    -1.0f, -1.0f, -1.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 0.0f,
    -1.0f, -1.0f,  1.0f, 1.0f  ,  0.0f, 0.0f, 0.0f, 0.0f,
    -1.0f,  1.0f,  1.0f, 1.0f  ,  0.5f, 0.0f, 0.0f, 0.0f,
    -1.0f, -1.0f, -1.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 0.0f,
    -1.0f,  1.0f,  1.0f, 1.0f  ,  0.5f, 0.0f, 0.0f, 0.0f,
    -1.0f,  1.0f, -1.0f, 1.0f  ,  0.5f, 1.0f, 0.0f, 0.0f,

    // right
    -1.0f,  1.0f, -1.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 0.0f,
    -1.0f,  1.0f,  1.0f, 1.0f  ,  0.0f, 0.0f, 0.0f, 0.0f,
     1.0f,  1.0f,  1.0f, 1.0f  ,  0.5f, 0.0f, 0.0f, 0.0f,
    -1.0f,  1.0f, -1.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 0.0f,
     1.0f,  1.0f,  1.0f, 1.0f  ,  0.5f, 0.0f, 0.0f, 0.0f,
     1.0f,  1.0f, -1.0f, 1.0f  ,  0.5f, 1.0f, 0.0f, 0.0f,

    // left
    -1.0f, -1.0f, -1.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 0.0f,
     1.0f, -1.0f,  1.0f, 1.0f  ,  0.5f, 0.0f, 0.0f, 0.0f,
    -1.0f, -1.0f,  1.0f, 1.0f  ,  0.0f, 0.0f, 0.0f, 0.0f,
    -1.0f, -1.0f, -1.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 0.0f,
     1.0f, -1.0f, -1.0f, 1.0f  ,  0.5f, 1.0f, 0.0f, 0.0f,
     1.0f, -1.0f,  1.0f, 1.0f  ,  0.5f, 0.0f, 0.0f, 0.0f,
};

void CubeRenderer::Initialize(ID3D11Device * d3dDevice, float aspectRatio)
{
    UNREFERENCED_PARAMETER(aspectRatio);

    m_d3dDevice         = d3dDevice;

    // Vertex buffer

    m_vertexBuffer.reset(new D3DConstantVertexBuffer(m_d3dDevice));
    m_vertexBuffer->Reset(gCubeVertices,
                          sizeof(float) * ARRAYSIZE(gCubeVertices));
    
    m_instanceBuffer.reset(new D3DDynamicVertexBuffer(m_d3dDevice));
    m_instanceBuffer->Resize(1); // auto align to 1M
    
    m_isDirty           = true;
    
    // Vertex shader

    LoadCompiledShaderFromFile(TEXT("..\\Debug\\CubeVS.vso"), &m_vertexShaderByteCode);

    ENSURE_OK(
        m_d3dDevice->CreateVertexShader(m_vertexShaderByteCode.pBytes,
                                        m_vertexShaderByteCode.nSize,
                                        nullptr,
                                        &m_d3dVertexShader));

    // Input layout

    D3D11_INPUT_ELEMENT_DESC inputElementDescs[] =
    {
        { "POSITION",    0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0,  0, D3D11_INPUT_PER_VERTEX_DATA,   0 },
        { "TEXCOORD",    0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 16, D3D11_INPUT_PER_VERTEX_DATA,   0 },
        { "TRANSLATION", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 1,  0, D3D11_INPUT_PER_INSTANCE_DATA, 1 },
    };
    ENSURE_OK(
        m_d3dDevice->CreateInputLayout(inputElementDescs,
                                       ARRAYSIZE(inputElementDescs),
                                       m_vertexShaderByteCode.pBytes,
                                       m_vertexShaderByteCode.nSize,
                                       &m_d3dInputLayout));

    // Pixel shader

    LoadCompiledShaderFromFile(TEXT("..\\Debug\\CubePS.pso"), &m_pixelShaderByteCode);

    ENSURE_OK(
        m_d3dDevice->CreatePixelShader(m_pixelShaderByteCode.pBytes,
                                       m_pixelShaderByteCode.nSize,
                                       nullptr,
                                       &m_d3dPixelShader));

    THROW_IF_FAILED(
        DirectX::CreateDDSTextureFromFile(m_d3dDevice,
                                          TEXT("Grass.dds"),
                                          nullptr,
                                          &m_d3dTextureSRV));

    // Constant buffer
    // set by CameraRenderer

    // States

    D3D11_RASTERIZER_DESC defaultRSDesc =
        CD3D11_RASTERIZER_DESC(CD3D11_DEFAULT());
    ENSURE_OK(
        m_d3dDevice->CreateRasterizerState(&defaultRSDesc,
                                           &m_defaultRS));
    D3D11_RASTERIZER_DESC lineRSDesc =
        CD3D11_RASTERIZER_DESC(CD3D11_DEFAULT());
    lineRSDesc.FillMode = D3D11_FILL_WIREFRAME;
    ENSURE_OK(
        m_d3dDevice->CreateRasterizerState(&lineRSDesc,
                                           &m_lineRS));

    D3D11_SAMPLER_DESC defaultSSDesc =
        CD3D11_SAMPLER_DESC(CD3D11_DEFAULT());
    ENSURE_OK(
        m_d3dDevice->CreateSamplerState(&defaultSSDesc,
                                        &m_samplerState));

    D3D11_DEPTH_STENCIL_DESC defaultDSDesc =
        CD3D11_DEPTH_STENCIL_DESC(CD3D11_DEFAULT());
    ENSURE_OK(
        m_d3dDevice->CreateDepthStencilState(&defaultDSDesc,
                                             &m_depthStencilState));

}

void CubeRenderer::Update(double milliSeconds)
{
    UNREFERENCED_PARAMETER(milliSeconds);
}

void CubeRenderer::Draw(ID3D11DeviceContext * d3dContext)
{
    m_d3dContext = d3dContext;

    if (m_isDirty)
    {
        size_t nSize = 0;
        for (int i = 0; i < MAX_TYPE; ++i)
        {
            nSize += m_cubes[i].size();
        }

        m_instanceBuffer->Resize(
            sizeof(m_cubes[0]) * nSize);
        
        auto mutator =
            m_instanceBuffer->Mutate().Begin(m_d3dContext);

        for (int i = 0; i < MAX_TYPE; ++i)
        {
            mutator.Fill(
                m_cubes[i].data(),
                m_cubes[i].size());
        }

        m_isDirty = false;
    }

    // Set IA stage

    ID3D11Buffer *  buffers[] = { m_vertexBuffer->Get(), m_instanceBuffer->Get() };
    UINT            strides[] = { sizeof(float) * 4 * 2, sizeof(m_cubes[0]) };
    UINT            offsets[] = { 0, 0 };
    
    m_d3dContext->IASetVertexBuffers(0, // slot
                                     2, // number of buffers
                                     buffers,
                                     strides,
                                     offsets);
    
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
                                       &m_d3dTextureSRV);

    // Set RS stage

    m_d3dContext->RSSetState(m_defaultRS);
    
    // Set OM stage

    m_d3dContext->OMSetDepthStencilState(m_depthStencilState,
                                         0);
    // Draw

    size_t offset = 0;
    size_t size;
    if ((size = m_cubes[LINE].size()) != 0)
    {
        m_d3dContext->IASetPrimitiveTopology(
            D3D11_PRIMITIVE_TOPOLOGY_LINELIST);
        m_d3dContext->DrawInstanced(36,
                                    size,
                                    0,
                                    offset);
        offset += size;
    }
    if ((size = m_cubes[TEXTURE].size()) != 0)
    {
        m_d3dContext->IASetPrimitiveTopology(
            D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
        m_d3dContext->DrawInstanced(36,
                                    size,
                                    0,
                                    offset);
        offset += size;
    }
}

void CubeRenderer::AddCube(float x, float y, float z, Type type)
{
    m_cubes[type].emplace_back(x, y, z, 0.0f);
    
    m_isDirty = true;
}

}