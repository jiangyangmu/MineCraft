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

    struct KeyboardEventArgs
    {
        WPARAM virtualKeyCode;
    };
}

// win32 Window
_DEFINE_EVENT(OnWndIdle)
_DEFINE_EVENT1(OnWndMove, win32::WindowRect)
_DEFINE_EVENT1(OnWndResize, win32::WindowRect)

// mouse
_DEFINE_EVENT1(OnMouseMove, win32::MouseEventArgs)
// keyboard
_DEFINE_EVENT1(OnKeyDown, win32::KeyboardEventArgs)
_DEFINE_EVENT1(OnKeyUp, win32::KeyboardEventArgs)

// dx app
_DEFINE_EVENT1(OnAspectRatioChange, float)

// Camera
_DEFINE_EVENT1(OnCameraDirChange, DirectX::XMFLOAT3)
_DEFINE_EVENT1(OnCameraPosChange, DirectX::XMFLOAT3)