#pragma once

#include <windows.h>
#include <functional>

#include "EventDefinitions.h"

namespace win32
{
    // Window
    HWND                CreateDesktopWindow(HINSTANCE hInstance, LPCWSTR lpWndTitle, int width, int height, WNDPROC lpfnWndProc, LPVOID lpParam);
    // COM
    void                InitializeCOM();
    void                UninitializeCOM();

    class Window
    {
    public:
        Window(LPCWSTR lpTitle, HINSTANCE hInstance);

        virtual         ~Window() = default;

        // Operations
        void            Show();

        // Properties
        HWND            GetHWND() const { return m_hWnd; }
        int             GetWidth() const { return m_width; }
        int             GetHeight() const { return m_height; }
        float           GetAspectRatio() const { return static_cast<float>(m_width) / static_cast<float>(m_height); }

        // Events
        public: _SEND_EVENT(OnWndIdle)
        public: _SEND_EVENT(OnWndMove)
        public: _SEND_EVENT(OnWndResize)
        public: _SEND_EVENT(OnMouseMove)

    private:
        // Win32 interfaces
        static LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
        LRESULT         ProcessMessage(UINT uMsg, WPARAM wParam, LPARAM lParam);
        
        HINSTANCE       m_hInstance;
        HWND            m_hWnd;
        int             m_width;
        int             m_height;
    };

    class Application
    {
    public:
        static int      Run(Window & mainWnd);
    };
}
