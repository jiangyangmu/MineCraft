#include "pch.h"

#include <strsafe.h>

#include "D3DApp.h"

int WINAPI wWinMain(
    _In_ HINSTANCE hInstance,
    _In_opt_ HINSTANCE hPrevInstance,
    _In_ LPWSTR lpCmdLine,
    _In_ int nCmdShow)
{
    int ret;

    UNREFERENCED_PARAMETER(hPrevInstance);
    UNREFERENCED_PARAMETER(lpCmdLine);
    UNREFERENCED_PARAMETER(nCmdShow);

    win32::InitializeCOM();

    render::D3DApplication app(L"DX Demo", hInstance);
    app.Initialize();
    ret = app.Run();

    win32::UninitializeCOM();

    return ret;
}
