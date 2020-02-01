#pragma once

#include <vector>

#include "Win32App.h"
#include "D3DRenderer.h"
#include "Camera.h"

namespace render
{
    class D3DApplication
    {
    public:

        D3DApplication(LPCWSTR lpTitle, HINSTANCE hInstance);

        // Operations

        // call before Initialize()
        void            RegisterRenderer(IRenderer * pRender);
        void            Initialize();
        virtual void    UpdateScene(double milliSeconds);
        virtual void    DrawScene();

        // Properties

        win32::Window & GetWindow() { return m_mainWnd; }

        // Events

        public: _SEND_EVENT(OnAspectRatioChange)

        private: _RECV_EVENT_DECL(D3DApplication, OnWndIdle)
        private: _RECV_EVENT_DECL1(D3DApplication, OnWndMove)
        private: _RECV_EVENT_DECL1(D3DApplication, OnWndResize)

    private:

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
        win32::Window               m_mainWnd;
        LARGE_INTEGER               m_timerFrequence;
        LARGE_INTEGER               m_timerPreviousValue;
        double                      m_fps;

        std::vector<IRenderer*>     m_renders;

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