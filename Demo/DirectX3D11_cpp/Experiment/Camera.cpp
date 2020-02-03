#include "pch.h"

#include "Camera.h"
#include "ErrorHandling.h"

using namespace DirectX;
using win32::ENSURE_TRUE;

namespace render
{

void Camera::Move(const DirectX::XMFLOAT3 & delta)
{
    m_pos.x += delta.x;
    m_pos.y += delta.y;
    m_pos.z += delta.z;
    _DISPATCH_EVENT1(OnCameraPosChange, *this, GetPos());
}

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
    
    // Current forward
    XMFLOAT4X4 m0;
    XMStoreFloat4x4(
        &m0,
        XMMatrixTranspose(XMMatrixRotationAxis(up, m_hradian)));
    XMFLOAT3 fwd0 =
    {
        m0._11 * m_forward.x + m0._12 * m_forward.y + m0._13 * m_forward.z,
        m0._21 * m_forward.x + m0._22 * m_forward.y + m0._23 * m_forward.z,
        0.0f
    };
    XMVECTOR forward = XMVector3Normalize(XMLoadFloat3(&fwd0));

    // Current right
    XMVECTOR right = XMVector3Cross(up, forward);

    XMFLOAT4X4 m;
    XMStoreFloat4x4(
        &m,
        XMMatrixTranspose(
            XMMatrixRotationAxis(up, m_hradian) * XMMatrixRotationAxis(right, -m_vradian)));

    // Current direction
    XMFLOAT3 dir =
    {
        m._11 * m_forward.x + m._12 * m_forward.y + m._13 * m_forward.z,
        m._21 * m_forward.x + m._22 * m_forward.y + m._23 * m_forward.z,
        m._31 * m_forward.x + m._32 * m_forward.y + m._33 * m_forward.z
    };
    XMStoreFloat3(&dir, XMVector3Normalize(XMLoadFloat3(&dir)));

    return dir;
}

const DirectX::XMMATRIX & Camera::GetViewMatrix()
{
    XMVECTOR up = XMLoadFloat3(&m_up);
    XMVECTOR fwd = XMLoadFloat3(&m_forward);
    XMVECTOR pos = XMLoadFloat3(&m_pos);

    m_viewMatrix    = XMMatrixTranslation(-m_pos.x, -m_pos.y, -m_pos.z)
                    * XMMatrixRotationAxis(up, -m_hradian)
                    * XMMatrixTranslation(m_pos.x, m_pos.y, m_pos.z)
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


void CameraController::Update(double milliSeconds)
{
    // HACK: assume up is unit Z
    ENSURE_TRUE(m_camera->GetUp().z == 1.0f);

    XMFLOAT3 fwd, right;
    
    fwd = m_camera->GetDirection();
    fwd.z = 0.0f;
    XMStoreFloat3(
        &fwd,
        XMVector3Normalize(XMLoadFloat3(&fwd)));

    XMStoreFloat3(
        &right,
        XMVector3Cross(
            XMLoadFloat3(&m_camera->GetUp()),
            XMLoadFloat3(&fwd)
        ));
    right.z = 0.0f;
    XMStoreFloat3(
        &right,
        XMVector3Normalize(XMLoadFloat3(&right)));

    float duration = static_cast<float>(milliSeconds / 1000.0f);

    XMFLOAT3 delta =
    {
        (m_forwardFactor * fwd.x + m_rightFactor * right.x) * duration * m_speed,
        (m_forwardFactor * fwd.y + m_rightFactor * right.y) * duration * m_speed,
        (m_forwardFactor * fwd.z + m_rightFactor * right.z) * duration * m_speed,
    };

    if (m_forwardFactor != 0.0f || m_rightFactor != 0.0f)
    {
        m_camera->Move(delta);
    }
}

_RECV_EVENT_IMPL(CameraController, OnMouseMove)
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

_RECV_EVENT_IMPL(CameraController, OnKeyDown)
(void * sender, const win32::KeyboardEventArgs & args)
{
    UNREFERENCED_PARAMETER(sender);

    switch (args.virtualKeyCode)
    {
        case 'W': m_forwardFactor = 1.0f; break;
        case 'S': m_forwardFactor = -1.0f; break;
        case 'A': m_rightFactor = -1.0f; break;
        case 'D': m_rightFactor = 1.0f; break;
        default: break;
    }
}

_RECV_EVENT_IMPL(CameraController, OnKeyUp)
(void * sender, const win32::KeyboardEventArgs & args)
{
    UNREFERENCED_PARAMETER(sender);

    switch (args.virtualKeyCode)
    {
        case 'W':
        case 'S': m_forwardFactor = 0.0f; break;
        case 'A':
        case 'D': m_rightFactor = 0.0f; break;
        default: break;
    }
}

}