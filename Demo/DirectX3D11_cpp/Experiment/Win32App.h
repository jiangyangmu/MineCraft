#pragma once

#include <windows.h>
#include <functional>

namespace win32
{
    // Error handling
    void                ErrorExit(LPTSTR lpszFunction);
    void                ErrorExit(HRESULT hr, LPTSTR lpszFunction);
    // Window
    LRESULT CALLBACK    WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
    HWND                CreateDesktopWindow(HINSTANCE hInstance, LPCWSTR lpWndTitle, int width, int height, WNDPROC lpfnWndProc);
    // COM
    void                InitializeCOM();
    void                UninitializeCOM();

    class Application
    {
    public:
        Application(LPCWSTR name);
        Application(LPCWSTR name, HINSTANCE hInstance);
        
        virtual         ~Application() = default;

        // Operations
        int             Run();

        // Properties
        HWND            GetHWND() const { return m_hWnd; }
        int             GetWidth() const { return m_width; }
        int             GetHeight() const { return m_height; }

    protected:
        virtual void    OnIdle() = 0;

    private:
        HINSTANCE       m_hInstance;
        HWND            m_hWnd;
        int             m_width;
        int             m_height;
    };

}
