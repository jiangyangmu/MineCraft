#include "pch.h"

#include <strsafe.h>
#include <sstream>

#include "D3DApp.h"
#include "ErrorHandling.h"

#include "CameraRenderer.h"
#include "TriangleRenderer.h"
#include "CubeRenderer.h"

using win32::ENSURE_TRUE;

namespace win32
{
    class WindowEventDebug
    {
    public:
        _RECV_EVENT(WindowEventDebug, OnWndIdle)
            (void * sender)
        {
            Window *    pWindow = static_cast<Window *>(sender); // safe?

            TCHAR       szTitle[128];
            GetWindowText(pWindow->GetHWND(), szTitle, 128);

            std::wostringstream ss;
            ss << "Receive OnWndIdle from window: " << szTitle << std::endl;
            OutputDebugString(ss.str().c_str());
            Sleep(1);
        }
        _RECV_EVENT1(WindowEventDebug, OnWndMove)
            (void * sender, const WindowRect & rect)
        {
            Window *    pWindow = static_cast<Window *>(sender); // safe?

            TCHAR       szTitle[128];
            GetWindowText(pWindow->GetHWND(), szTitle, 128);

            std::wostringstream ss;
            ss << "Receive OnWndMove from window: " << szTitle << " x: " << rect.x << " y: " << rect.y << std::endl;
            OutputDebugString(ss.str().c_str());
        }
        _RECV_EVENT1(WindowEventDebug, OnWndResize)
            (void * sender, const WindowRect & rect)
        {
            Window *    pWindow = static_cast<Window *>(sender); // safe?

            TCHAR       szTitle[128];
            GetWindowText(pWindow->GetHWND(), szTitle, 128);

            std::wostringstream ss;
            ss << "Receive OnWndResize from window: " << szTitle << " w: " << rect.width << " h: " << rect.height << std::endl;
            OutputDebugString(ss.str().c_str());
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

    win32::InitializeCOM();

    auto app    = render::D3DApplication(L"DX Demo", hInstance);

    render::CameraRenderer      camera;
    render::TriangleRenderer    tri;
    render::CubeRenderer        cube;

    app.RegisterRenderer(&camera);
    app.RegisterRenderer(&tri);
    app.RegisterRenderer(&cube);

    _BIND_EVENT(OnAspectRatioChange, app, camera.GetCamera());
    _BIND_EVENT(OnMouseMove, app.GetWindow(), camera.GetCamera().GetController());

    app.Initialize();
    
    //win32::WindowEventDebug wed;
    //_BIND_EVENT(OnWndIdle, app.GetWindow(), wed);
    //_BIND_EVENT(OnWndMove, app.GetWindow(), wed);
    //_BIND_EVENT(OnWndResize, app.GetWindow(), wed);

    int ret             = win32::Application::Run(app.GetWindow());
    
    win32::UninitializeCOM();

    return ret;
}
