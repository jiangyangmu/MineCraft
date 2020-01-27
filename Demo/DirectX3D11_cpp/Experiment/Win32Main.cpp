#include "pch.h"

#include <strsafe.h>
#include <sstream>

#include "D3DApp.h"
#include "ErrorHandling.h"

namespace win32
{
    class WindowDebug : public Application
    {
    public:
        using Application::Application;

    protected:
        virtual void OnIdle() override
        {
            SetWindowText(GetHWND(), L"OnIdle");
            Sleep(1);
        }
        virtual void OnMove(int x, int y) override
        {
            TCHAR buffer[64];
            StringCchPrintf(buffer, 64, L"OnMove: x:%d y:%d", x, y);
            SetWindowText(GetHWND(), buffer);
        }
        virtual void OnResize(int width, int height) override
        {
            TCHAR buffer[64];
            StringCchPrintf(buffer, 64, L"OnResize: w:%d h:%d", width, height);
            SetWindowText(GetHWND(), buffer);
        }
    };

    class KeyboardDebug : public KeyboardInput
    {
    public:
        virtual void OnKeyDown(WPARAM keyCode) override
        {
            wchar_t msg[32];
            swprintf_s(msg, L"Key Down: 0x%x\n", keyCode);
            OutputDebugString(msg);
        }
        virtual void OnKeyUp(WPARAM keyCode) override
        {
            wchar_t msg[32];
            swprintf_s(msg, L"Key Up: 0x%x\n", keyCode);
            OutputDebugString(msg);
        }
    };

    class MouseDebug : public IMouseInput
    {
    public:
        virtual void OnMouseMove(int pixelX, int pixelY, DWORD flags) override
        {
            UNREFERENCED_PARAMETER(flags);

            wchar_t msg[64];
            swprintf_s(msg, L"Mose Move: %d %d\n", pixelX, pixelY);
            OutputDebugString(msg);
        }
        virtual void OnMouseLButtonDown(int pixelX, int pixelY, DWORD flags) override
        {
            UNREFERENCED_PARAMETER(flags);

            wchar_t msg[64];
            swprintf_s(msg, L"Mose LButton Down: %d %d\n", pixelX, pixelY);
            OutputDebugString(msg);
        }
        virtual void OnMouseLButtonUp() override
        {
            wchar_t msg[32];
            swprintf_s(msg, L"Mose LButton Up\n");
            OutputDebugString(msg);
        }
    };
}

int WINAPI wWinMain(
    _In_ HINSTANCE hInstance,
    _In_opt_ HINSTANCE hPrevInstance,
    _In_ LPWSTR lpCmdLine,
    _In_ int nCmdShow)
{
    UNREFERENCED_PARAMETER(hPrevInstance);
    UNREFERENCED_PARAMETER(lpCmdLine);
    UNREFERENCED_PARAMETER(nCmdShow);
    
    // Enable run-time memory check for debug builds.
    //#if defined(DEBUG) | defined(_DEBUG)
    //    _CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
    //#endif

    // win32::ENSURE_TRUE(false);
    // win32::ENSURE_OK(E_OUTOFMEMORY);
    // win32::ENSURE_NOT_NULL(nullptr);
    // dx::THROW_IF_FAILED(E_OUTOFMEMORY);

    win32::InitializeCOM();

    auto * keyboard     = new win32::KeyboardDebug();
    auto * mouse        = new win32::MouseDebug();
    auto * app          = new render::D3DApplication(L"DX Demo", hInstance);
    
    keyboard->SetHWND(app->GetHWND());
    
    render::D3DTriangle triangle;
    app->RegisterGP(&triangle);

    app->Initialize();
    app->SetKeyboardInput(keyboard);
    app->SetMouseInput(mouse);
    
    int ret             = win32::Application::Run(*app);
    
    delete app;
    delete keyboard;
    delete mouse;

    win32::UninitializeCOM();

    return ret;
}
