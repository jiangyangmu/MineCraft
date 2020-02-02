#pragma once

#include "D3DRenderer.h"
#include "RendererUtil.h"
#include "D3DBuffer.h"

#include <vector>

namespace render
{
    class CubeRenderer : public IRenderer
    {
    public:
        virtual         ~CubeRenderer() override;

        virtual void    Initialize(ID3D11Device * d3dDevice, float aspectRatio) override;
        virtual void    Update(double milliSeconds) override;
        virtual void    Draw(ID3D11DeviceContext * d3dContext) override;

        void            AddCube(float x, float y, float z);

    private:
        ID3D11Device *              m_d3dDevice;
        ID3D11DeviceContext *       m_d3dContext;
        
        bool                        m_isDirty;
        D3DDynamicVertexBuffer *    m_vertexBuffer;
        D3DDynamicVertexBuffer *    m_instanceBuffer;
        std::vector<DirectX::XMFLOAT4>
                                    m_cubes;

        ID3D11InputLayout *         m_d3dInputLayout;
        ID3D11VertexShader *        m_d3dVertexShader;
        ID3D11PixelShader *         m_d3dPixelShader;
        ShaderByteCode              m_vertexShaderByteCode;
        ShaderByteCode              m_pixelShaderByteCode;

        ID3D11RasterizerState *     m_rasterizerState;
    };
}