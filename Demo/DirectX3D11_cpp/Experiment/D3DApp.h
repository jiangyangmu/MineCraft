#pragma once

#include <vector>

#include "Win32App.h"

namespace render
{
    // IRenderer
    interface ID3DGraphicsPipeline
    {
        // Create resources
        virtual void    Prepare(ID3D11Device * d3dDevice, float aspectRatio) = 0;
        // Bind resources to context, configure context
        virtual void    Draw(ID3D11DeviceContext * d3dContext) = 0;
    };

    class D3DTriangle : public ID3DGraphicsPipeline
    {
    public:
        virtual void    Prepare(ID3D11Device * d3dDevice, float aspectRatio) override;
        virtual void    Draw(ID3D11DeviceContext * d3dContext) override;

    private:
        struct ConstantBufferStruct
        {
            DirectX::XMFLOAT4X4 mvp;
        };
        struct ShaderByteCode
        {
            size_t  nSize;
            BYTE *  pBytes;
        };
        void            LoadCompiledShaderFromFile(const TCHAR * pFileName, ShaderByteCode * pSBC);

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

    class D3DApplication : public win32::Application
    {
    public:
        using win32::Application::Application;

        void            RegisterGP(ID3DGraphicsPipeline * pGP);

        void            Initialize();

        virtual void    UpdateScene(double ms);
        virtual void    DrawScene();

    private:
        // Implement win32::Application
        void OnIdle() override;
        void OnMove(int x, int y) override;
        void OnResize(int width, int height) override;

        // --------------------------------------------------------------------------
        // Internal methods
        // --------------------------------------------------------------------------
        void InitializeD3D();
        void PrepareResources();
        void InitializeD3DPipeline();
        
        void UpdateRenderTargets(int width, int height);
        void SetFullScreen(bool isFullScreen);
        
        void ClearScreen();
        void PresentNextFrame();

        // --------------------------------------------------------------------------
        // Internal State
        // --------------------------------------------------------------------------
        typedef std::vector<ID3DGraphicsPipeline*> GPList;
        GPList                      m_graphicsPipelines;
        LARGE_INTEGER               m_timerFrequence;
        LARGE_INTEGER               m_timerPreviousValue;
        double                      m_fps;

        // --------------------------------------------------------------------------
        // DirectX 3D State
        // --------------------------------------------------------------------------
        ID3D11Device *              m_d3dDevice;
        ID3D11DeviceContext *       m_d3dContext;
    
        IDXGISwapChain *            m_SwapChain;
    
        ID3D11RenderTargetView *    m_RenderTargetView;
        ID3D11DepthStencilView *    m_DepthStencilView;

        // --------------------------------------------------------------------------
        // DirectX 2D State
        // --------------------------------------------------------------------------
        ID2D1DeviceContext *        m_d2dContext;
    };

}