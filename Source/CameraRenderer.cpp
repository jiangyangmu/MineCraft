#include "pch.h"

#include "CameraRenderer.h"

using namespace win32;
using namespace dx;

namespace render
{

void CameraRenderer::Initialize(ID3D11Device * d3dDevice, float aspectRatio)
{
    m_camera.SetAspectRatio(aspectRatio);
    m_d3dDevice = d3dDevice;

    // Constant buffer

    CD3D11_BUFFER_DESC constantBufferDesc(
        sizeof(ConstantBufferStruct),
        D3D11_BIND_CONSTANT_BUFFER
    );

    ENSURE_OK(
        m_d3dDevice->CreateBuffer(&constantBufferDesc,
                                  nullptr,
                                  &m_d3dConstantBuffer));
}

void CameraRenderer::Update(double milliSeconds)
{
    UNREFERENCED_PARAMETER(milliSeconds);

    m_camera.GetController().Update(milliSeconds);
}

void CameraRenderer::Draw(ID3D11DeviceContext * d3dContext)
{
    m_d3dContext = d3dContext;

    DirectX::XMStoreFloat4x4(
        &m_constantBufferData.mvp,
        DirectX::XMMatrixTranspose(
            DirectX::XMMatrixMultiply(
                m_camera.GetViewMatrix(),
                m_camera.GetProjMatrix()
            )));
    m_constantBufferData.pos =
    {
        m_camera.GetPos().x,
        m_camera.GetPos().y,
        m_camera.GetPos().z,
        0.0f
    };

    // Update constant buffer

    m_d3dContext->UpdateSubresource(m_d3dConstantBuffer,
                                    0,
                                    nullptr,
                                    &m_constantBufferData,
                                    0,
                                    0);

    m_d3dContext->VSSetConstantBuffers(0,
                                       1,
                                       &m_d3dConstantBuffer);
}

}