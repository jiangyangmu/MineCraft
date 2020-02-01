#pragma once

#include "Event.h"

namespace win32
{
    struct WindowRect
    {
        int x, y;
        int width, height;
    };

    struct MouseEventArgs
    {
        int pixelX;
        int pixelY;
        DWORD flags;
    };
}

// win32 Window
_DEFINE_EVENT(OnWndIdle)
_DEFINE_EVENT1(OnWndMove, win32::WindowRect)
_DEFINE_EVENT1(OnWndResize, win32::WindowRect)

// mouse
_DEFINE_EVENT1(OnMouseMove, win32::MouseEventArgs)

// dx app
_DEFINE_EVENT1(OnAspectRatioChange, float)

// Camera
_DEFINE_EVENT1(OnCameraDirChange, DirectX::XMFLOAT3)