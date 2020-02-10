#pragma once

namespace win32
{
    extern void     ENSURE_TRUE(bool bRet);
    extern void     ENSURE_NOT_NULL(const void * pv);
    extern void     ENSURE_OK(HRESULT hr);
    extern void     ENSURE_CLIB_SUCCESS(int iRet, LPCTSTR msg = NULL);
}

namespace dx
{
    extern void     THROW_IF_FAILED(HRESULT hr);
}