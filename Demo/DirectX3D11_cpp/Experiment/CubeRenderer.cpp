#include "pch.h"

#include "CubeRenderer.h"
#include "ErrorHandling.h"

using namespace win32;
using namespace dx;

namespace render
{

void CubeRenderer::Initialize(ID3D11Device * d3dDevice, float aspectRatio)
{
    m_d3dDevice = d3dDevice;

    // Vertex buffer

    float data[] =
    {
          -1.0f, -1.0f, -1.0f, 1.0f   ,  1.0f, 0.0f, 0.0f, 1.0f,
          - 1.0f,  1.0f, -1.0f, 1.0f  ,  1.0f, 0.0f, 0.0f, 1.0f,
           1.0f,  1.0f, -1.0f, 1.0f   ,  1.0f, 0.0f, 0.0f, 1.0f,
          - 1.0f, -1.0f, -1.0f, 1.0f  ,  1.0f, 0.0f, 0.0f, 1.0f,
           1.0f,  1.0f, -1.0f, 1.0f   ,  1.0f, 0.0f, 0.0f, 1.0f,
           1.0f, -1.0f, -1.0f, 1.0f   ,  1.0f, 0.0f, 0.0f, 1.0f,

          - 1.0f, -1.0f,  1.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 1.0f,
           1.0f,  1.0f,  1.0f, 1.0f   ,  0.0f, 1.0f, 0.0f, 1.0f,
          - 1.0f,  1.0f,  1.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 1.0f,
          - 1.0f, -1.0f,  1.0f, 1.0f  ,  0.0f, 1.0f, 0.0f, 1.0f,
           1.0f, -1.0f,  1.0f, 1.0f   ,  0.0f, 1.0f, 0.0f, 1.0f,
           1.0f,  1.0f,  1.0f, 1.0f   ,  0.0f, 1.0f, 0.0f, 1.0f,

          - 1.0f, 1.0f, -1.0f,  1.0f  ,  0.0f, 0.0f, 1.0f, 1.0f,
          - 1.0f, 1.0f,  1.0f,  1.0f  ,  0.0f, 0.0f, 1.0f, 1.0f,
           1.0f, 1.0f,  1.0f,  1.0f   ,  0.0f, 0.0f, 1.0f, 1.0f,
          - 1.0f, 1.0f, -1.0f,  1.0f  ,  0.0f, 0.0f, 1.0f, 1.0f,
           1.0f, 1.0f,  1.0f,  1.0f   ,  0.0f, 0.0f, 1.0f, 1.0f,
           1.0f, 1.0f, -1.0f,  1.0f   ,  0.0f, 0.0f, 1.0f, 1.0f,

          - 1.0f,-1.0f, -1.0f,  1.0f  ,  1.0f, 1.0f, 0.0f, 1.0f,
           1.0f,-1.0f,  1.0f,  1.0f   ,  1.0f, 1.0f, 0.0f, 1.0f,
          - 1.0f,-1.0f,  1.0f,  1.0f  ,  1.0f, 1.0f, 0.0f, 1.0f,
          - 1.0f,-1.0f, -1.0f,  1.0f  ,  1.0f, 1.0f, 0.0f, 1.0f,
           1.0f,-1.0f, -1.0f,  1.0f   ,  1.0f, 1.0f, 0.0f, 1.0f,
           1.0f,-1.0f,  1.0f,  1.0f   ,  1.0f, 1.0f, 0.0f, 1.0f,

          - 1.0f, -1.0f, -1.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,
          - 1.0f, -1.0f,  1.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,
          - 1.0f,  1.0f,  1.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,
          - 1.0f, -1.0f, -1.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,
          - 1.0f,  1.0f,  1.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,
          - 1.0f,  1.0f, -1.0f, 1.0f  ,  1.0f, 0.0f, 1.0f, 1.0f,

           1.0f, -1.0f, -1.0f, 1.0f   ,  0.0f, 1.0f, 1.0f, 1.0f,
           1.0f,  1.0f,  1.0f, 1.0f   ,  0.0f, 1.0f, 1.0f, 1.0f,
           1.0f, -1.0f,  1.0f, 1.0f   ,  0.0f, 1.0f, 1.0f, 1.0f,
           1.0f, -1.0f, -1.0f, 1.0f   ,  0.0f, 1.0f, 1.0f, 1.0f,
           1.0f,  1.0f, -1.0f, 1.0f   ,  0.0f, 1.0f, 1.0f, 1.0f,
           1.0f,  1.0f,  1.0f, 1.0f   ,  0.0f, 1.0f, 1.0f, 1.0f,
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

void CubeRenderer::Update(double milliSeconds)
{
    UNREFERENCED_PARAMETER(milliSeconds);
}

void CubeRenderer::Draw(ID3D11DeviceContext * d3dContext)
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
    m_d3dContext->Draw(36, 0);
}

}