#pragma once

#include "Event.h"

namespace win32
{
    struct WindowRect
    {
        int x, y;
        int width, height;
    };
}

// win32 Window
_DEFINE_EVENT(OnWndIdle)
_DEFINE_EVENT1(OnWndMove, win32::WindowRect)
_DEFINE_EVENT1(OnWndResize, win32::WindowRect)
