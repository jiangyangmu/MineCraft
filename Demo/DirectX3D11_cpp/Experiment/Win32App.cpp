#include "pch.h"

#include <strsafe.h>

#include "Win32App.h"

namespace win32
{

// --------------------------------------------------------------------------
// Error handling
// EH in COM: https://docs.microsoft.com/en-us/windows/win32/learnwin32/error-handling-in-com
// EH in Win32: https://docs.microsoft.com/en-us/windows/win32/debug/error-handling-functions
// --------------------------------------------------------------------------

// Style 1: throw with call stack & error message.
// Style 2: show a message box with call stack & error message, exit.

// TODO: CaptureStackBackTrace
void ErrorExit(
    LPTSTR lpszFunction)
{
    const int   BUF_SIZE = 1024;

    LPVOID      lpDisplayBuf;
    LPVOID      lpMsgBuf;
    DWORD       dwError;

    // Get error code

    dwError = GetLastError();

    // Format error message

    FormatMessage(
        FORMAT_MESSAGE_ALLOCATE_BUFFER |
        FORMAT_MESSAGE_FROM_SYSTEM |
        FORMAT_MESSAGE_IGNORE_INSERTS,
        NULL,
        dwError,
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        (LPTSTR)&lpMsgBuf,
        0, NULL);

    lpDisplayBuf = (LPVOID)LocalAlloc(
        LMEM_ZEROINIT,
        (lstrlen((LPCTSTR)lpMsgBuf) + lstrlen((LPCTSTR)lpszFunction) + 40) * sizeof(TCHAR));

    StringCchPrintf(
        (LPTSTR)lpDisplayBuf,
        LocalSize((HLOCAL)lpDisplayBuf) / sizeof(TCHAR),
        TEXT("%s failed with error %d: %s"),
        lpszFunction,
        dwError,
        lpMsgBuf);

    // Display the error message and exit the process
    MessageBox(NULL, (LPCTSTR)lpDisplayBuf, TEXT("Error"), MB_OK);

    LocalFree(lpMsgBuf);
    LocalFree(lpDisplayBuf);
    ExitProcess(dwError);
}

void ErrorExit(
    HRESULT hr,
    LPTSTR lpszFunction)
{
    if (FAILED(hr))
    {
        ErrorExit(lpszFunction);
    }
}

// --------------------------------------------------------------------------
// Window
// https://docs.microsoft.com/en-us/windows/win32/learnwin32/learn-to-program-for-windows
// --------------------------------------------------------------------------

LRESULT CALLBACK WndProc(
    HWND hWnd,
    UINT message,
    WPARAM wParam,
    LPARAM lParam)
{
    switch (message)
    {
        case WM_CLOSE:
            DestroyWindow(hWnd);
            break;
        case WM_DESTROY:
            PostQuitMessage(0);
            break;
    }

    return DefWindowProc(hWnd, message, wParam, lParam);
}

HWND CreateDesktopWindow(
    HINSTANCE hInstance,
    LPCWSTR lpWndTitle,
    int width,
    int height,
    WNDPROC lpfnWndProc)
{
    static LPCWSTR      wndClassName = L"ExperimentWindowClass";
    static UINT         wndStyle = CS_HREDRAW | CS_VREDRAW | CS_OWNDC;
    static const int    x = CW_USEDEFAULT;
    static const int    y = CW_USEDEFAULT;

    // Register window class

    WNDCLASS wndClass;
    wndClass.style = wndStyle;
    wndClass.lpfnWndProc = lpfnWndProc;
    wndClass.cbClsExtra = 0;
    wndClass.cbWndExtra = 0;
    wndClass.hInstance = hInstance;
    wndClass.hIcon = LoadIconW(hInstance, L"IDI_ICON");
    wndClass.hCursor = LoadCursorW(NULL, IDC_ARROW);
    wndClass.hbrBackground = NULL;
    wndClass.lpszMenuName = NULL;
    wndClass.lpszClassName = wndClassName;

    if (!RegisterClassW(&wndClass))
    {
        return NULL;
    }

    // Create window

    return CreateWindowEx(
        WS_EX_APPWINDOW | WS_EX_WINDOWEDGE,
        wndClassName,
        lpWndTitle,
        WS_OVERLAPPEDWINDOW,
        x,
        y,
        width,
        height,
        NULL, // hWndParent
        NULL, // hMenu
        hInstance,
        NULL // lpParam
    );
}

static inline HINSTANCE GetCurrentInstance()
{
    return GetModuleHandle(NULL);
}

// --------------------------------------------------------------------------
// COM
// --------------------------------------------------------------------------

void InitializeCOM()
{
    ErrorExit(
        CoInitializeEx(nullptr, COINITBASE_MULTITHREADED),
        L"InitializeCOM");
}

void UninitializeCOM()
{
    CoUninitialize();
}

// --------------------------------------------------------------------------
// Application
// --------------------------------------------------------------------------

Application::Application(LPCWSTR name)
    : Application(name, GetCurrentInstance())
{
       
}

Application::Application(LPCWSTR name, HINSTANCE hInst)
{
    m_hInstance   = hInst;
    m_hWnd        = CreateDesktopWindow(m_hInstance,
                                      name,
                                      640,
                                      480,
                                      WndProc);
    if (NULL == m_hWnd)
    {
        ErrorExit(L"Application");
    }

    RECT rect;
    bool bRet = GetClientRect(m_hWnd, &rect);
    assert(bRet);

    m_width       = rect.right - rect.left;
    m_height      = rect.bottom - rect.top;
}

int Application::Run()
{
    if (!IsWindowVisible(m_hWnd))
    {
        ShowWindow(m_hWnd, SW_SHOW);
    }

    // Start message loop

    MSG msg;

    msg.message = WM_NULL;
    PeekMessage(&msg, NULL, 0U, 0U, PM_NOREMOVE);
    while (WM_QUIT != msg.message)
    {
        if (PeekMessage(&msg, NULL, 0U, 0U, PM_REMOVE))
        {
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
        else
        {
            OnIdle();
        }
    }

    return (int)msg.wParam;
}

}
