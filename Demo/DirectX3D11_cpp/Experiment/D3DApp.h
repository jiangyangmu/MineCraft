#pragma once

#include <vector>

#include "Win32App.h"
#include "D3DRenderer.h"

namespace render
{
    class D3DApplication : public win32::Application
    {
    public:
        using win32::Application::Application;

        // Before Initialize()
        void            RegisterRenderer(ID3DRenderer * pRender);

        void            Initialize();

        virtual void    UpdateScene(double milliSeconds);
        virtual void    DrawScene();

    private:
        // --------------------------------------------------------------------------
        // Main window events
        // --------------------------------------------------------------------------
        void OnIdle() override;
        void OnMove(int x, int y) override;
        void OnResize(int width, int height) override;

        // --------------------------------------------------------------------------
        // Internal methods
        // --------------------------------------------------------------------------
        void InitializeD3DDevice();
        void InitializeD3DPipelines();
        
        void UpdateRenderTargets(int width, int height);
        void SetFullScreen(bool isFullScreen);
        
        void ClearScreen();
        void PresentNextFrame();

        // --------------------------------------------------------------------------
        // Internal State
        // --------------------------------------------------------------------------
        std::vector<ID3DRenderer*>  m_renders;
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