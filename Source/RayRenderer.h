#pragma once

#include "D3DRenderer.h"
#include "RendererUtil.h"
#include "D3DBuffer.h"
#include "EventDefinitions.h"

namespace render
{
    class RayRenderer : public IRenderer
    {
    public:

        virtual void    Initialize(ID3D11Device * d3dDevice,float aspectRatio) override;
        
        virtual void    Update(double milliSeconds) override;
        virtual void    Draw(ID3D11DeviceContext * d3dContext) override;

        public: _RECV_EVENT1(RayRenderer, OnCameraDirChange)
            (void * sender, const DirectX::XMFLOAT3  & dir)
        {
            UNREFERENCED_PARAMETER(sender);
            m_dir = dir;
            m_isDirty = true;
        }
        public: _RECV_EVENT1(RayRenderer, OnCameraPosChange)
            (void * sender, const DirectX::XMFLOAT3  & pos)
        {
            UNREFERENCED_PARAMETER(sender);
            m_pos = pos;
            m_isDirty = true;
        }

    private:

        ID3D11Device *              m_d3dDevice;
        ID3D11DeviceContext *       m_d3dContext;
        
        Ptr<D3DDynamicVertexBuffer> m_vertexBuffer;

        ID3D11InputLayout *         m_d3dInputLayout;
        ID3D11VertexShader *        m_d3dVertexShader;
        ID3D11PixelShader *         m_d3dPixelShader;
        ShaderByteCode              m_vertexShaderByteCode;
        ShaderByteCode              m_pixelShaderByteCode;

        ID3D11RasterizerState *     m_rasterizerState;

        const float                 m_length    = 100.0f;
        bool                        m_isDirty   = false;
        DirectX::XMFLOAT3           m_pos       = {0.0f, 0.0f, 0.0f};
        DirectX::XMFLOAT3           m_dir       = {1.0f, 0.0f, 0.0f};
    };

}

