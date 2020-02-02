#include "pch.h"

#include "CubeRenderer.h"
#include "ErrorHandling.h"

using namespace win32;
using namespace dx;

namespace render
{

static const float gCubeVertices[] =
{
    -1.0f, -1.0f, -1.0f, 1.0f   ,  1.0f, 0.0f, 0.0f, 1.0f,
    -1.0f,  1.0f, -1.0f, 1.0f  ,  1.0f, 0.0f, 0.0f, 1.0f,
    1.0f,  1.0f, -1.0f, 1.0f   ,  1.0f, 0.0f, 0.0f, 1.0f,
    -1.0f, -1.0f, -1.0f, 1.0f  ,  1.0f, 0.0f, 0.0f, 1.0f,
    1.0f,  1.0f, -1.0f, 1.0f   ,  1.0f, 0.0f, 0.0f, 1.0f,
    1.0f, -1.0f, -1.0f, 1.0f   ,  1.0f, 0.0f, 0.0f, 1.0f,

    -1.0f, -1.0f,  1.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 1.0f,
    1.0f,  1.0f,  1.0f, 1.0f   ,  0.0f, 1.0f, 0.0f, 1.0f,
    -1.0f,  1.0f,  1.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 1.0f,
    -1.0f, -1.0f,  1.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 1.0f,
    1.0f, -1.0f,  1.0f, 1.0f   ,  0.0f, 1.0f, 0.0f, 1.0f,
    1.0f,  1.0f,  1.0f, 1.0f   ,  0.0f, 1.0f, 0.0f, 1.0f,

    -1.0f, 1.0f, -1.0f,  1.0f  ,  0.0f, 0.0f, 1.0f, 1.0f,
    -1.0f, 1.0f,  1.0f,  1.0f  ,  0.0f, 0.0f, 1.0f, 1.0f,
    1.0f, 1.0f,  1.0f,  1.0f   ,  0.0f, 0.0f, 1.0f, 1.0f,
    -1.0f, 1.0f, -1.0f,  1.0f  ,  0.0f, 0.0f, 1.0f, 1.0f,
    1.0f, 1.0f,  1.0f,  1.0f   ,  0.0f, 0.0f, 1.0f, 1.0f,
    1.0f, 1.0f, -1.0f,  1.0f   ,  0.0f, 0.0f, 1.0f, 1.0f,

    -1.0f,-1.0f, -1.0f,  1.0f  ,  1.0f, 1.0f, 0.0f, 1.0f,
    1.0f,-1.0f,  1.0f,  1.0f   ,  1.0f, 1.0f, 0.0f, 1.0f,
    -1.0f,-1.0f,  1.0f,  1.0f  ,  1.0f, 1.0f, 0.0f, 1.0f,
    -1.0f,-1.0f, -1.0f,  1.0f  ,  1.0f, 1.0f, 0.0f, 1.0f,
    1.0f,-1.0f, -1.0f,  1.0f   ,  1.0f, 1.0f, 0.0f, 1.0f,
    1.0f,-1.0f,  1.0f,  1.0f   ,  1.0f, 1.0f, 0.0f, 1.0f,

    -1.0f, -1.0f, -1.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,
    -1.0f, -1.0f,  1.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,
    -1.0f,  1.0f,  1.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,
    -1.0f, -1.0f, -1.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,
    -1.0f,  1.0f,  1.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,
    -1.0f,  1.0f, -1.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,

    1.0f, -1.0f, -1.0f, 1.0f   ,  0.0f, 1.0f, 1.0f, 1.0f,
    1.0f,  1.0f,  1.0f, 1.0f   ,  0.0f, 1.0f, 1.0f, 1.0f,
    1.0f, -1.0f,  1.0f, 1.0f   ,  0.0f, 1.0f, 1.0f, 1.0f,
    1.0f, -1.0f, -1.0f, 1.0f   ,  0.0f, 1.0f, 1.0f, 1.0f,
    1.0f,  1.0f, -1.0f, 1.0f   ,  0.0f, 1.0f, 1.0f, 1.0f,
    1.0f,  1.0f,  1.0f, 1.0f   ,  0.0f, 1.0f, 1.0f, 1.0f,
};

CubeRenderer::~CubeRenderer()
{
    if (m_vertexBuffer)
    {
        delete m_vertexBuffer;
    }
}

void CubeRenderer::Initialize(ID3D11Device * d3dDevice, float aspectRatio)
{
    UNREFERENCED_PARAMETER(aspectRatio);

    m_d3dDevice         = d3dDevice;

    // Vertex buffer

    m_vertexBuffer      = new D3DDynamicVertexBuffer(m_d3dDevice);
    m_vertexBuffer->Resize(sizeof(float) * ARRAYSIZE(gCubeVertices));
    
    m_instanceBuffer    = new D3DDynamicVertexBuffer(m_d3dDevice);
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
        { "POSITION",  0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0,  0, D3D11_INPUT_PER_VERTEX_DATA,   0 },
        { "COLOR",     0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 16, D3D11_INPUT_PER_VERTEX_DATA,   0 },
        { "TEXCOORD",  1, DXGI_FORMAT_R32G32B32A32_FLOAT, 1,  0, D3D11_INPUT_PER_INSTANCE_DATA, 1 },
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
    // set by CameraRenderer

    // States

    D3D11_RASTERIZER_DESC noCullingRSDesc;
    noCullingRSDesc.FillMode = D3D11_FILL_WIREFRAME;
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
        (*m_vertexBuffer)
            .Mutate()
            .Begin(m_d3dContext)
            .Fill(gCubeVertices, sizeof(float) * ARRAYSIZE(gCubeVertices));

        m_instanceBuffer->Resize(sizeof(m_cubes[0]) * m_cubes.size());
        (*m_instanceBuffer)
            .Mutate()
            .Begin(m_d3dContext)
            .Fill(m_cubes.data(), m_cubes.size());
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

    // Set RS stage

    m_d3dContext->RSSetState(m_rasterizerState);

    // Draw
    // m_d3dContext->Draw(36, 0);
    m_d3dContext->DrawInstanced(36,             // vertex count
                                m_cubes.size(), // instance count
                                0,              // vertex start
                                0);             // instance start
}

void CubeRenderer::AddCube(float x, float y, float z)
{
    m_cubes.emplace_back(x, y, z, 0.0f);
    
    m_isDirty = true;
}

}