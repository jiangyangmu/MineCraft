#include "pch.h"

#include <strsafe.h>
#include <sstream>

#include "D3DApp.h"

#include "CameraRenderer.h"
#include "TriangleRenderer.h"
#include "CubeRenderer.h"
#include "SkyboxRenderer.h"
#include "RayRenderer.h"
#include "Block.h"

using win32::ENSURE_TRUE;
using render::D3DApplication;

struct Scene
{
    render::CameraRenderer      camera;
    render::RayRenderer         ray;
    render::SkyboxRenderer      skybox;
    render::CubeRenderer        cube;
    render::TriangleRenderer    tri;

    render::CubeRenderer        blockRenderers[16];
    scene::BlockSystem          block;
};

class MyApplication : public render::D3DApplication
{
public:

    using D3DApplication::D3DApplication;

    void            SetUpdateSceneCallback(std::function<void(double)> cb)
    {
        updateSceneCallback = std::move(cb);
    }

    virtual void    UpdateScene(double milliSeconds) override
    {
        if (updateSceneCallback)
        {
            updateSceneCallback(milliSeconds);
        }
        D3DApplication::UpdateScene(milliSeconds);
    }

private:

    std::function<void(double)> updateSceneCallback;
};

void BuildScene(Scene & scene, MyApplication & app)
{
    app.RegisterRenderer(&scene.camera);
    app.RegisterRenderer(&scene.ray);
    // app.RegisterRenderer(&scene.skybox);
    // app.RegisterRenderer(&scene.cube);
    // app.RegisterRenderer(&scene.tri);

    for (auto & renderer : scene.blockRenderers)
    {
        app.RegisterRenderer(&renderer);
    }
    scene.block.SetRendererPool(scene.blockRenderers,
                                ARRAYSIZE(scene.blockRenderers));

    app.SetUpdateSceneCallback(
        [&](double milliSeconds)
        {
            DirectX::XMFLOAT3 pos = scene.camera.GetCamera().GetPos();
            int x = static_cast<int>(pos.x);
            int y = static_cast<int>(pos.y);
            int z = static_cast<int>(pos.z - 5.0f);

            static double elapsed = 0.0;
            if ((elapsed += milliSeconds) > 100.0)
            {
                //std::wostringstream ss;
                //ss << "Pos " << pos.x << " " << pos.y << " " << pos.z << std::endl;
                //ss << "Pos " << x << " " << y << " " << z << std::endl;
                //OutputDebugString(ss.str().c_str());

                // add a block below feet.
                scene.block.Set(x, y, z, scene::GRASS_BLOCK);
                
                elapsed = 0.0;
            }

            scene.block.Sync(x, y, z);
        }
    );

    _BIND_EVENT(OnAspectRatioChange,    app,                scene.camera.GetCamera());
    _BIND_EVENT(OnMouseMove,            app.GetWindow(),    scene.camera.GetCamera().GetController());
    _BIND_EVENT(OnKeyDown,              app.GetWindow(),    scene.camera.GetCamera().GetController());
    _BIND_EVENT(OnKeyUp,                app.GetWindow(),    scene.camera.GetCamera().GetController());

    _BIND_EVENT(OnCameraDirChange,      scene.camera.GetCamera(),       scene.ray);
    _BIND_EVENT(OnCameraPosChange,      scene.camera.GetCamera(),       scene.ray);
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

    MyApplication   app(L"DX Demo", hInstance);
    Scene           scene;

    BuildScene(scene, app);
    app.Initialize();
    
    int             ret =
    win32::Application::Run(app.GetWindow());
    
    win32::UninitializeCOM();

    return ret;
}
