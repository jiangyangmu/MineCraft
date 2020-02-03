TODO:

Gameplay
* 

--------------------------------------------------------------------------------

Win32: Window event
* create, destroy
* resize
* active, deactive
* enter fullscreen, exit fullscreen

Win32: User input
* mouse controller
* keyboard controller

--------------------------------------------------------------------------------

CRT
* Intro: https://docs.microsoft.com/en-us/cpp/c-runtime-library/c-run-time-library-reference?view=vs-2019
* API: https://docs.microsoft.com/en-us/cpp/c-runtime-library/run-time-routines-by-category?view=vs-2019

Error handling
* EH in COM: https://docs.microsoft.com/en-us/windows/win32/learnwin32/error-handling-in-com
* EH in Win32: https://docs.microsoft.com/en-us/windows/win32/debug/error-handling-functions

COM
* Intro: https://docs.microsoft.com/en-us/windows/win32/learnwin32/module-2--using-com-in-your-windows-program
    Init, Error Code, COM-style malloc
    Best practices: __uuidof, IID_PPV_ARGS, SafeRelease/CComPtr
    Error handling pattern: https://docs.microsoft.com/en-us/windows/win32/learnwin32/error-handling-in-com
        Nested ifs, Cascading ifs, Jump on Fail, Throw on Fail

Win32
* Intro: https://docs.microsoft.com/en-us/windows/win32/
* Hello world: https://docs.microsoft.com/en-us/windows/win32/learnwin32/learn-to-program-for-windows
* Window: https://docs.microsoft.com/en-us/windows/win32/winmsg/windows
    desktop window, app window, controls, dialog boxes
    window attributes
    window creation
    window type: overlapped/pop-up/child/layered/message-only
    window relationship: foreground-background/owned/z-order
    window show state: active/disabled/visibility/min/max/restored
    window size & position
    window animation
    window layout & mirroring
    window destruction
* Message: https://docs.microsoft.com/en-us/windows/win32/winmsg/about-messages-and-message-queues?redirectedfrom=MSDN
    WndProc, WM_??: https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-size
    message loop, message Filtering/Handling/Routing
    two message loop style: PeekMessage(async), GetMessage(sync)
* Mouse input: https://docs.microsoft.com/en-us/windows/win32/learnwin32/mouse-input
* Keyboard input: https://docs.microsoft.com/en-us/windows/win32/learnwin32/keyboard-input
    GetKeyState - local to current window state, a snapshot of all key state
    GetAsyncKeyState - physical state
    Translate key combo into app command: https://docs.microsoft.com/en-us/windows/win32/learnwin32/accelerator-tables
* Error handling: https://docs.microsoft.com/en-us/windows/win32/debug/error-handling
    Functions, structs, macros
    GetLastError, FormatMessage
    Beautify integrated error message: https://docs.microsoft.com/en-us/windows/win32/debug/message-tables
    Philosophy: https://docs.microsoft.com/en-us/windows/win32/debug/error-message-guidelines
* Debugger: https://docs.microsoft.com/en-us/windows/win32/api/debugapi/
    
DX
* Math: https://docs.microsoft.com/en-us/windows/win32/dxmath/ovw-xnamath-reference-functions
* Cube Demo: // https://docs.microsoft.com/en-us/windows/win32/direct3dgetstarted/complete-code-sample-for-using-a-corewindow-with-directx
