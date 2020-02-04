#include "pch.h"

#include "RendererUtil.h"

using namespace win32;

namespace render
{

// D3DCompileFromFile
void LoadCompiledShaderFromFile(const TCHAR * pFileName, ShaderByteCode * pSBC)
{
    FILE *  pFile;
    long    nSize;
    size_t  nReadSize;

    ENSURE_CLIB_SUCCESS(    _wfopen_s(&pFile, pFileName, TEXT("rb"))    );
    
    ENSURE_CLIB_SUCCESS(    fseek(pFile, 0, SEEK_END)                   );
    
    nSize                   = ftell(pFile);
    ENSURE_TRUE(nSize > 0);
    
    ENSURE_CLIB_SUCCESS(    fseek(pFile, 0, SEEK_SET)                   );
    
    pSBC->nSize             = static_cast<size_t>(nSize);
    pSBC->pBytes            = new BYTE[nSize];

    nReadSize               = fread_s(pSBC->pBytes,
                                      pSBC->nSize,
                                      sizeof(BYTE),
                                      pSBC->nSize,
                                      pFile);
    ENSURE_TRUE(nReadSize == pSBC->nSize);
}

}