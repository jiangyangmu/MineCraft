#pragma once

#include "D3DRenderer.h"
#include "RendererUtil.h"

namespace render
{
    class TriangleRenderer : public ID3DRenderer
    {
    public:
        virtual void    Initialize(ID3D11Device * d3dDevice, float aspectRatio) override;
        
        virtual void    Update(double milliSeconds) override;
        virtual void    Draw(ID3D11DeviceContext * d3dContext) override;

    private:
        struct ConstantBufferStruct
        {
            DirectX::XMFLOAT4X4 mvp;
        };

        ID3D11Device *              m_d3dDevice;
        ID3D11DeviceContext *       m_d3dContext;
        
        ID3D11Buffer *              m_d3dVertexBuffer;
        ID3D11Buffer *              m_d3dConstantBuffer;
        ConstantBufferStruct        m_constantBufferData;

        ID3D11InputLayout *         m_d3dInputLayout;
        ID3D11VertexShader *        m_d3dVertexShader;
        ID3D11PixelShader *         m_d3dPixelShader;
        ShaderByteCode              m_vertexShaderByteCode;
        ShaderByteCode              m_pixelShaderByteCode;

        ID3D11RasterizerState *    m_rasterizerState;
    };

}