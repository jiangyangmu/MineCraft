#pragma once

#include "D3DRenderer.h"

namespace render
{
    struct ShaderByteCode
    {
        size_t  nSize;
        BYTE *  pBytes;
    };

    extern void     LoadCompiledShaderFromFile(const TCHAR * pFileName, ShaderByteCode * pSBC);

    template <typename T>
    using Ptr = std::unique_ptr<T>;
}