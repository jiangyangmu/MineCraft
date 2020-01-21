#pragma once

#include "Win32App.h"

namespace render
{
    extern void DXExitIfError(HRESULT hr);

    class D3DApplication : public win32::Application
    {
    public:
        using win32::Application::Application;

        void Initialize();

    private:
        // Implement win32::Application
        void OnIdle() override;

        // --------------------------------------------------------------------------
        // Internal methods
        // --------------------------------------------------------------------------
        void ClearScreen();
        void PresentNextFrame();

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