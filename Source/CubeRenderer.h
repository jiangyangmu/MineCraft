#pragma once

#include "D3DRenderer.h"
#include "D3DBuffer.h"
#include "RendererUtil.h"

#include <vector>

namespace render
{
    class CubeRenderer : public IRenderer
    {
    public:
        enum Type
        {
            LINE            = 0,
            TEXTURE         = 1,

            MAX_TYPE,
        };

        virtual void    Initialize(ID3D11Device * d3dDevice, float aspectRatio) override;
        virtual void    Update(double milliSeconds) override;
        virtual void    Draw(ID3D11DeviceContext * d3dContext) override;

        void            AddCube(float x, float y, float z, Type type = LINE);

    private:
        ID3D11Device *                  m_d3dDevice;
        ID3D11DeviceContext *           m_d3dContext;

        Ptr<D3DConstantVertexBuffer>    m_vertexBuffer;
        Ptr<D3DDynamicVertexBuffer>     m_instanceBuffer;

        ID3D11ShaderResourceView *      m_d3dTextureSRV;

        ID3D11InputLayout *             m_d3dInputLayout;
        ID3D11VertexShader *            m_d3dVertexShader;
        ID3D11PixelShader *             m_d3dPixelShader;
        ShaderByteCode                  m_vertexShaderByteCode;
        ShaderByteCode                  m_pixelShaderByteCode;

        ID3D11RasterizerState *         m_defaultRS;
        ID3D11RasterizerState *         m_lineRS;
        ID3D11SamplerState *            m_samplerState;
        ID3D11DepthStencilState *       m_depthStencilState;

        bool                            m_isDirty;
        std::vector<DirectX::XMFLOAT4>  m_cubes[MAX_TYPE];
    };
}