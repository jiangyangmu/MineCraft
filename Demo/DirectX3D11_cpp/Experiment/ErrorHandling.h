#pragma once

namespace win32
{
    extern void     ENSURE_TRUE(bool bRet);
    extern void     ENSURE_NOT_NULL(void * pv);
    extern void     ENSURE_OK(HRESULT hr);
}

namespace dx
{
    extern void     THROW_IF_FAILED(HRESULT hr);
}