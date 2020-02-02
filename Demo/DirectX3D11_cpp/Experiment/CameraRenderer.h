#pragma once

#include "D3DRenderer.h"
#include "RendererUtil.h"
#include "Camera.h"

namespace render
{
    class CameraRenderer : public IRenderer
    {
    public:
        CameraRenderer()
            : m_camera(60.0f, 1.0f, {0.0f, 0.0f, 5.0f})
        {}

        virtual void    Initialize(ID3D11Device * d3dDevice, float aspectRatio) override;

        virtual void    Update(double milliSeconds) override;
        virtual void    Draw(ID3D11DeviceContext * d3dContext) override;

        Camera &        GetCamera() { return m_camera; }

    private:
        struct ConstantBufferStruct
        {
            DirectX::XMFLOAT4X4 mvp;
        };

        Camera                      m_camera;

        ID3D11Device *              m_d3dDevice;
        ID3D11DeviceContext *       m_d3dContext;
        ID3D11Buffer *              m_d3dConstantBuffer;
        ConstantBufferStruct        m_constantBufferData;
    };
}