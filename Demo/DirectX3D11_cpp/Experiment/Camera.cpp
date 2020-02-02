#include "pch.h"

#include "Camera.h"

using namespace DirectX;

namespace render
{

void Camera::SetHorizontalAngle(float fAngle)
{
    float hradian = fAngle / 180.0f * XM_PI;

    if (m_hradian != hradian)
    {
        m_hradian = hradian;
        _DISPATCH_EVENT1(OnCameraDirChange, *this, GetDirection());
    }
}

void Camera::SetVerticalAngle(float fAngle)
{
    float vradian =
        std::max(-90.0f, std::min(fAngle, 90.0f)) / 180.0f * XM_PI;

    if (m_vradian != vradian)
    {
        m_vradian = vradian;
        _DISPATCH_EVENT1(OnCameraDirChange, *this, GetDirection());
    }
}

const DirectX::XMFLOAT3 Camera::GetDirection()
{
    XMVECTOR up = XMLoadFloat3(&m_up);
    XMVECTOR right = XMLoadFloat3(&m_right);

    XMVECTOR _scale, _rotate, _trans;

    XMMatrixDecompose(&_scale, &_rotate, &_trans,
                      XMMatrixTranspose(
                          XMMatrixRotationAxis(up, m_hradian) * XMMatrixRotationAxis(right, -m_vradian)
                      ));

    XMFLOAT3 dir;
    XMStoreFloat3(&dir, XMVector3Normalize(_trans));

    return dir;
}

const DirectX::XMMATRIX & Camera::GetViewMatrix()
{
    XMVECTOR up = XMLoadFloat3(&m_up);
    XMVECTOR fwd = XMLoadFloat3(&m_forward);
    XMVECTOR pos = XMLoadFloat3(&m_pos);

    m_viewMatrix    = XMMatrixTranslation(m_pos.x, m_pos.y, m_pos.z)
                    * XMMatrixRotationAxis(up, -m_hradian)
                    * XMMatrixTranslation(-m_pos.x, -m_pos.y, -m_pos.z)
                    * XMMatrixLookAtLH(pos, pos + fwd, up)
                    * XMMatrixRotationAxis(-fwd, -m_vradian)
                    ;
    return m_viewMatrix;
}

const DirectX::XMMATRIX & Camera::GetProjMatrix()
{
    m_projMatrix = XMMatrixPerspectiveFovLH(m_fov,
                                            m_aspectRatio,
                                            m_nearZ,
                                            m_farZ);
    return m_projMatrix;
}

_RECV_EVENT_IMPL(Camera, OnAspectRatioChange)
(void * sender, const float & aspectRatio)
{
    UNREFERENCED_PARAMETER(sender);

    m_aspectRatio = aspectRatio;
}

_RECV_EVENT_IMPL(CameraMouseController, OnMouseMove)
(void * sender, const win32::MouseEventArgs & args)
{
    UNREFERENCED_PARAMETER(sender);

    if (m_init)
    {
        m_init = false;
    }
    else
    {
        m_hAngle += 0.2f * (args.pixelX - m_pixelX);
        m_vAngle -= 0.2f * (args.pixelY - m_pixelY);
        m_vAngle = std::max(-90.0f, std::min(m_vAngle, 90.0f));
    }
    m_pixelX = args.pixelX;
    m_pixelY = args.pixelY;

    m_camera->SetHorizontalAngle(m_hAngle);
    m_camera->SetVerticalAngle(m_vAngle);
}

}