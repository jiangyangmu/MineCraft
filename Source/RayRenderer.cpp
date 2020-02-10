#include "pch.h"

#include "RayRenderer.h"
#include "D3DBuffer.h"

using namespace win32;
using namespace dx;

namespace render
{

void RayRenderer::Initialize(ID3D11Device * d3dDevice, float aspectRatio)
{
    UNREFERENCED_PARAMETER(aspectRatio);

    m_d3dDevice = d3dDevice;

    // Vertex buffer

    float data[] =
    {
          0.0f,     0.0f, 0.0f, 1.0f,       1.0f, 1.0f, 1.0f, 1.0f,
          m_length, m_length, m_length, 1.0f,       1.0f, 1.0f, 1.0f, 1.0f,
    };
    
    m_vertexBuffer.reset(new D3DDynamicVertexBuffer(m_d3dDevice));
    m_vertexBuffer->Resize(sizeof(float) * ARRAYSIZE(data));
    
    m_isDirty       = true;

    // Vertex shader

    LoadCompiledShaderFromFile(STR_VERTEXSHADER_VSO, &m_vertexShaderByteCode);

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

    LoadCompiledShaderFromFile(STR_PIXELSHADER_PSO, &m_pixelShaderByteCode);

    ENSURE_OK(
        m_d3dDevice->CreatePixelShader(m_pixelShaderByteCode.pBytes,
                                       m_pixelShaderByteCode.nSize,
                                       nullptr,
                                       &m_d3dPixelShader));

    // Constant buffer
    // set by CameraRenderer
}

void RayRenderer::Update(double milliSeconds)
{
    UNREFERENCED_PARAMETER(milliSeconds);
}

void RayRenderer::Draw(ID3D11DeviceContext * d3dContext)
{
    m_d3dContext = d3dContext;

    if (m_isDirty)
    {
        float data[] =
        {
            // X, Y, Z
            0.0f, 0.0f, 0.0f, 1.0f,
            1.0f, 0.0f, 0.0f, 1.0f,
            100.0f, 0.0f, 0.0f, 1.0f,
            1.0f, 0.0f, 0.0f, 1.0f,
            0.0f, 0.0f, 0.0f, 1.0f,
            0.0f, 1.0f, 0.0f, 1.0f,
            0.0f, 100.0f, 0.0f, 1.0f,
            0.0f, 1.0f, 0.0f, 1.0f,
            0.0f, 0.0f, 0.0f, 1.0f,
            0.0f, 0.0f, 1.0f, 1.0f,
            0.0f, 0.0f, 100.0f, 1.0f,
            0.0f, 0.0f, 1.0f, 1.0f,
            // Eye dir
            //m_pos.x, m_pos.y, m_pos.z, 1.0f,
            //1.0f, 1.0f, 0.0f, 1.0f,
            //m_pos.x + m_dir.x * m_length, m_pos.y + m_dir.y * m_length, m_pos.z + m_dir.z * m_length, 1.0f,
            //1.0f, 1.0f, 0.0f, 1.0f,
            // Eye dir assist
            //10.0f, 0.0f, 0.0f, 1.0f,
            //1.0f, 1.0f, 1.0f, 1.0f,
            //m_pos.x + m_dir.x * m_length, m_pos.y + m_dir.y * m_length, m_pos.z + m_dir.z * m_length, 1.0f,
            //1.0f, 1.0f, 1.0f, 1.0f,
            //0.0f, 10.0f, 0.0f, 1.0f,
            //1.0f, 1.0f, 1.0f, 1.0f,
            //m_pos.x + m_dir.x * m_length, m_pos.y + m_dir.y * m_length, m_pos.z + m_dir.z * m_length, 1.0f,
            //1.0f, 1.0f, 1.0f, 1.0f,
            //0.0f, 0.0f, 10.0f, 1.0f,
            //1.0f, 1.0f, 1.0f, 1.0f,
            //m_pos.x + m_dir.x * m_length, m_pos.y + m_dir.y * m_length, m_pos.z + m_dir.z * m_length, 1.0f,
            //1.0f, 1.0f, 1.0f, 1.0f,
        };

        (*m_vertexBuffer)
            .Mutate()
            .Begin(m_d3dContext)
            .Fill(data, ARRAYSIZE(data));
        m_isDirty = false;
    }

    // Set IA stage

    ID3D11Buffer * buffers = m_vertexBuffer->Get();
    UINT strides = sizeof(float) * 4 * 2;
    UINT offsets = 0;
    m_d3dContext->IASetVertexBuffers(0, // slot
                                     1, // number of buffers
                                     &buffers,
                                     &strides,
                                     &offsets);

    m_d3dContext->IASetPrimitiveTopology(
        D3D11_PRIMITIVE_TOPOLOGY_LINELIST);

    m_d3dContext->IASetInputLayout(m_d3dInputLayout);

    // Set VS stage

    m_d3dContext->VSSetShader(m_d3dVertexShader,
                              nullptr,
                              0);

    // Set PS stage

    m_d3dContext->PSSetShader(m_d3dPixelShader,
                              nullptr,
                              0);

    // Draw
    m_d3dContext->Draw(28 - 16, 0);
}

}