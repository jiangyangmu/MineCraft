#pragma once

#include "D3DRenderer.h"

namespace render
{
    struct ShaderByteCode
    {
        size_t  nSize;
        BYTE *  pBytes;
    };

    template <typename T>
    using Ptr = std::unique_ptr<T>;
    
    extern void     LoadCompiledShaderFromFile(const TCHAR * pFileName, ShaderByteCode * pSBC);

}