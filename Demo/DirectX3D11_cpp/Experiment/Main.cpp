#include "pch.h"

#include <strsafe.h>
#include <sstream>

#include "D3DApp.h"
#include "ErrorHandling.h"

#include "CameraRenderer.h"
#include "TriangleRenderer.h"
#include "CubeRenderer.h"
#include "SkyboxRenderer.h"

using win32::ENSURE_TRUE;
using render::D3DApplication;

struct Scene
{
    render::CameraRenderer      camera;
    render::SkyboxRenderer      skybox;
    render::CubeRenderer        cube;
    render::TriangleRenderer    tri;
};

void BuildScene(Scene & scene, render::D3DApplication & app)
{
    for (int x = -5; x <= 5; ++x)
    {
        for (int y = -5; y <= 5; ++y)
        {
            scene.cube.AddCube((float)x * 2.0f, (float)y * 2.0f, 0.0f);
        }
    }

    app.RegisterRenderer(&scene.camera);
    app.RegisterRenderer(&scene.skybox);
    app.RegisterRenderer(&scene.cube);
    // app.RegisterRenderer(&scene.tri);

    _BIND_EVENT(OnAspectRatioChange,    app,                scene.camera.GetCamera());
    _BIND_EVENT(OnMouseMove,            app.GetWindow(),    scene.camera.GetCamera().GetController());
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

    D3DApplication  app(L"DX Demo", hInstance);
    Scene           scene;

    BuildScene(scene, app);
    app.Initialize();
    
    int ret = win32::Application::Run(app.GetWindow());
    
    win32::UninitializeCOM();

    return ret;
}
