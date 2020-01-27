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
}