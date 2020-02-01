#pragma once

#include <algorithm>

#include <DirectXMath.h>

#include "EventDefinitions.h"

namespace render
{
    class Camera;

    class CameraMouseController
    {
    public:

        CameraMouseController(Camera * pCamera) : m_camera(pCamera) {}

        public: _RECV_EVENT_DECL1(CameraMouseController, OnMouseMove);

    private:

        Camera *    m_camera;
        bool        m_init = true;
        int         m_pixelX = 0;
        int         m_pixelY = 0;
        float       m_hAngle = 0.0f;
        float       m_vAngle = -30.0f;
    };

    class Camera
    {
    public:

        Camera(float fov,
               float aspectRatio,
               const DirectX::XMFLOAT3 pos)
            : m_fov(DirectX::XMConvertToRadians(fov))
            , m_aspectRatio(aspectRatio)
            , m_up(0.0f, 0.0f, 1.0f)
            , m_right(0.0f, 1.0f, 0.0f)
            , m_forward(1.0f, 0.0f, 0.0f)
            , m_hradian(0.0f)
            , m_vradian(0.0f)
            , m_pos(pos)
            , m_controller(this)
        {
        }

        // Properties
        
        // Left: 0.0f, Right: 360.0f
        void                            SetHorizontalAngle(float fAngle);
        // Down: -90.0f, Up: 90.0f
        void                            SetVerticalAngle(float fAngle);
        void                            SetAspectRatio(float fAspectRatio) { m_aspectRatio = fAspectRatio; }

        const DirectX::XMFLOAT3         GetDirection();
        const DirectX::XMMATRIX &       GetViewMatrix();
        const DirectX::XMMATRIX &       GetProjMatrix();

        CameraMouseController &         GetController() { return m_controller; }
        
        // Events

        public: _SEND_EVENT(OnCameraDirChange)

        public: _RECV_EVENT_DECL1(Camera, OnAspectRatioChange)

    private:

        // --------------------------------------------------------------------------
        // Camera lens parameters
        // --------------------------------------------------------------------------
        const float                 m_nearZ = 0.1f;
        const float                 m_farZ = 1000.0f;
        float                       m_fov;
        float                       m_aspectRatio;

        // --------------------------------------------------------------------------
        // World parameters
        // --------------------------------------------------------------------------
        const DirectX::XMFLOAT3     m_up;
        const DirectX::XMFLOAT3     m_right;
        const DirectX::XMFLOAT3     m_forward;
        float                       m_hradian; // horizontal rotation
        float                       m_vradian; // vertical rotation
        
        DirectX::XMFLOAT3           m_pos;

        // --------------------------------------------------------------------------
        // Cache
        // --------------------------------------------------------------------------
        DirectX::XMMATRIX           m_viewMatrix;
        DirectX::XMMATRIX           m_projMatrix;

        // --------------------------------------------------------------------------
        // Controllers
        // --------------------------------------------------------------------------
        CameraMouseController       m_controller;
    };

}