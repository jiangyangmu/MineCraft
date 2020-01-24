#pragma once

#include <windows.h>
#include <functional>

namespace win32
{
    // Window
    HWND                CreateDesktopWindow(HINSTANCE hInstance, LPCWSTR lpWndTitle, int width, int height, WNDPROC lpfnWndProc, LPVOID lpParam);
    // COM
    void                InitializeCOM();
    void                UninitializeCOM();

    interface IKeyboardInput
    {
        virtual void    OnKeyDown(WPARAM keyCode) = 0;
        virtual void    OnKeyUp(WPARAM keyCode) = 0;
    };

    class KeyboardInput : public IKeyboardInput
    {
    public:
        enum KeyState
        {
            KS_DOWN,
            KS_UP,
            KS_TOGGLED,
            KS_UNTOGGLED,
        };

        KeyboardInput() : m_hWnd(NULL) {}

        // Operations
        // must call in the same thread of OnKeyDown/OnKeyUp
        bool            TestKeyState(WPARAM keyCode, int keyState);

        // Properties
        void            SetHWND(HWND hWnd) { m_hWnd = hWnd; }
        HWND            GetHWND() const { return m_hWnd; }

    private:
        HWND            m_hWnd;
    };

    interface IMouseInput
    {
        virtual void    OnMouseMove(int pixelX, int pixelY, DWORD flags) = 0;
        virtual void    OnMouseLButtonDown(int pixelX, int pixelY, DWORD flags) = 0;
        virtual void    OnMouseLButtonUp() = 0;
    };

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
        void            SetKeyboardInput(IKeyboardInput * pKeyboardInput) { m_keyboardInput = pKeyboardInput; }
        void            SetMouseInput(IMouseInput * pMouseInput) { m_mouseInput = pMouseInput; }

    protected:
        // Events
        virtual void    OnIdle() = 0;
        virtual void    OnMove(int x, int y) = 0;
        virtual void    OnResize(int width, int height) = 0;

    private:
        // Win32 interfaces
        static LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
        LRESULT         ProcessMessage(UINT uMsg, WPARAM wParam, LPARAM lParam);
        
        HINSTANCE       m_hInstance;
        HWND            m_hWnd;
        int             m_width;
        int             m_height;

        IKeyboardInput *    m_keyboardInput;
        IMouseInput *       m_mouseInput;
    };

    class Application : public Window 
    {
    public:
        static int      Run(Application & app);

        using Window::Window;
    };
}