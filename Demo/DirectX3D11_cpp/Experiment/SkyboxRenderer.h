#pragma once

#include "D3DRenderer.h"
#include "RendererUtil.h"

namespace render
{
    class SkyboxRenderer : public IRenderer
    {
    public:
        virtual void    Initialize(ID3D11Device * d3dDevice,float aspectRatio) override;
        
        virtual void    Update(double milliSeconds) override;
        virtual void    Draw(ID3D11DeviceContext * d3dContext) override;

    private:
        ID3D11Device *              m_d3dDevice;
        ID3D11DeviceContext *       m_d3dContext;
        
        ID3D11ShaderResourceView *  m_d3dCubeMapSRV;

        ID3D11Buffer *              m_d3dVertexBuffer;

        ID3D11InputLayout *         m_d3dInputLayout;
        ID3D11VertexShader *        m_d3dVertexShader;
        ID3D11PixelShader *         m_d3dPixelShader;
        ShaderByteCode              m_vertexShaderByteCode;
        ShaderByteCode              m_pixelShaderByteCode;

        ID3D11RasterizerState *     m_rasterizerState;
        ID3D11SamplerState *        m_samplerState;
        ID3D11DepthStencilState *   m_depthStencilState;
    };

}