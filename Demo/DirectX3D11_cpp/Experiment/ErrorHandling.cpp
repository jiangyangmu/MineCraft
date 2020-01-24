#include "pch.h"

#include <cstdio>
#include <cwchar>
#include <strsafe.h>
#include <DbgHelp.h>

#include "ErrorHandling.h"

#define IGNORE_ERROR(hr) ((void)hr)

static DWORD GetErrorMessageOfLastError(LPTSTR lpBuffer, DWORD nSize, DWORD dwErrorCode = GetLastError())
{
    DWORD   dwFlags;
    DWORD   dwCount;

    // FORMAT_MESSAGE_FROM_SYSTEM - fetch from system error table
    // FORMAT_MESSAGE_FROM_HMODULE - fetch from user-defined error table
    // FORMAT_MESSAGE_IGNORE_INSERTS - ignore 'Arguments'
    dwFlags         = FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS;
    
    dwCount         = FormatMessage(dwFlags,
                                    NULL, // lpSource
                                    dwErrorCode,
                                    MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), // dwLanguageId
                                    lpBuffer,
                                    nSize,
                                    NULL); // Arguments

    //if (0 == dwCount)
    //{
    //    IGNORE_ERROR(
    //        StringCchPrintf(lpBuffer,
    //                        nSize,
    //                        TEXT("GetErrorMessageOfLastError failed with error %d"),
    //                        GetLastError()));
    //}
    return dwCount;
}
static void PopupErrorMessage(LPTSTR lpMsgPrefix)
{
    const int   BUF_SIZE = 1024;

    TCHAR       szBuffer[BUF_SIZE];

    // Format prefix message
    
    IGNORE_ERROR(
        StringCchPrintf(szBuffer,
                        BUF_SIZE,
                        TEXT("%Ts failed with error %d: "),
                        lpMsgPrefix,
                        GetLastError()));
    
    DWORD       nCount = lstrlen(szBuffer);

    // Format error message

    GetErrorMessageOfLastError(
        szBuffer + nCount,
        BUF_SIZE - nCount);

    // Show message box

    MessageBox(NULL,
               szBuffer,
               TEXT("Error"),
               MB_OK);
}

static void GetStackTrace(LPTSTR lpBuffer, DWORD nSize, PCONTEXT ctx)
{
    const int MaxNameLen = 256;

    BOOL                result;
    HANDLE              process;
    HANDLE              thread;
    HMODULE             hModule;

    STACKFRAME64        stack;
    ULONG               frame;    
    DWORD64             displacement;

    DWORD               disp;
    PIMAGEHLP_LINEW64   line;

    TCHAR               buffer[sizeof(SYMBOL_INFO) + MAX_SYM_NAME * sizeof(TCHAR)];
    // TCHAR               name[MaxNameLen];
    TCHAR               module[MaxNameLen];
    PSYMBOL_INFOW       pSymbol = (PSYMBOL_INFOW)buffer;

    memset( &stack, 0, sizeof( STACKFRAME64 ) );

    process                = GetCurrentProcess();
    thread                 = GetCurrentThread();
    displacement           = 0;
#if !defined(_M_AMD64)
    stack.AddrPC.Offset    = (*ctx).Eip;
    stack.AddrPC.Mode      = AddrModeFlat;
    stack.AddrStack.Offset = (*ctx).Esp;
    stack.AddrStack.Mode   = AddrModeFlat;
    stack.AddrFrame.Offset = (*ctx).Ebp;
    stack.AddrFrame.Mode   = AddrModeFlat;
#endif

    SymInitialize( process, NULL, TRUE ); //load symbols

    for( frame = 0; ; frame++ )
    {
        //get next call from stack
        result = StackWalk64
        (
#if defined(_M_AMD64)
            IMAGE_FILE_MACHINE_AMD64
#else
            IMAGE_FILE_MACHINE_I386
#endif
            ,
            process,
            thread,
            &stack,
            ctx,
            NULL,
            SymFunctionTableAccess64,
            SymGetModuleBase64,
            NULL
        );

        if( !result )
        {
            break;
        }

        //get symbol name for address
        pSymbol->SizeOfStruct = sizeof(SYMBOL_INFO);
        pSymbol->MaxNameLen = MAX_SYM_NAME;
        SymFromAddrW(process, ( ULONG64 )stack.AddrPC.Offset, &displacement, pSymbol);

        line = (PIMAGEHLP_LINEW64)malloc(sizeof(IMAGEHLP_LINEW64));
        line->SizeOfStruct = sizeof(IMAGEHLP_LINEW64);

        //try to get line
        if (SymGetLineFromAddrW64(process, stack.AddrPC.Offset, &disp, line))
        {
            int nCnt = swprintf_s(lpBuffer,
                                  nSize,
                                  TEXT("\tat %Ts in %Ts: line: %lu: address: 0x%llX\n"),
                                  pSymbol->Name,
                                  line->FileName,
                                  line->LineNumber,
                                  pSymbol->Address);
            lpBuffer += nCnt, nSize -= nCnt;
        }
        else
        { 
            //failed to get line
            int nCnt = swprintf_s(lpBuffer,
                                  nSize,
                                  TEXT("\tat %Ts, address 0x%llX.\n"),
                                  pSymbol->Name,
                                  pSymbol->Address);
            lpBuffer += nCnt, nSize -= nCnt;

            hModule = NULL;
            lstrcpy(module,TEXT(""));
            GetModuleHandleEx(
                GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, 
                (LPCTSTR)(stack.AddrPC.Offset),
                &hModule);

            //at least print module name
            if(hModule != NULL)GetModuleFileName(hModule, module, MaxNameLen);       

            nCnt = swprintf_s(lpBuffer,
                              nSize,
                              TEXT("in %Ts\n"),
                              module);
            lpBuffer += nCnt, nSize -= nCnt;
        }

        free(line);
        line = NULL;

        if (lstrcmp(pSymbol->Name, TEXT("wWinMain")) == 0)
        {
            break;
        }
    }
}

static int  SEH_ExceptionFilter(DWORD dwExceptionCode,
                                DWORD dwErrorCode,
                                PCONTEXT pContext,
                                LPCWSTR lpMsg)
{
    // TODO: critical section
    static TCHAR szBuffer[4096];

    LPTSTR  lpBuffer = szBuffer;
    DWORD   nSize = 4096;
    int     nCnt;

    if (lpMsg != NULL)
    {
        nCnt = swprintf_s(lpBuffer,
                          nSize,
                          TEXT("%Ts\n\n"),
                          lpMsg);
        lpBuffer += nCnt, nSize -= nCnt;
    }

    if (dwErrorCode != 0)
    {
        nCnt = GetErrorMessageOfLastError(lpBuffer, nSize, dwErrorCode);
        lpBuffer += nCnt, nSize -= nCnt;
        nCnt = swprintf_s(lpBuffer, nSize, TEXT("\n"));
        lpBuffer += nCnt, nSize -= nCnt;
    }
    
    if (dwExceptionCode != 0)
    {
        nCnt = swprintf_s(lpBuffer,
                          nSize,
                          TEXT("*** Exception 0x%x occured ***\n"),
                          dwExceptionCode);
        lpBuffer += nCnt, nSize -= nCnt;
    }

    nCnt = swprintf_s(lpBuffer,
                      nSize,
                      TEXT("********* Stack Track *********\n"));
    lpBuffer += nCnt, nSize -= nCnt;
    GetStackTrace(lpBuffer, nSize, pContext);

     // Show message box

    MessageBox(NULL,
               szBuffer,
               TEXT("Exception"),
               MB_OK);

    return TRUE;
}

namespace win32
{
    void ENSURE_TRUE(bool bRet)
    {
        if (!bRet)
        {
            
            __try
            {
                RaiseException(0, 0, 0, NULL);
            }
            __except (SEH_ExceptionFilter(
                GetExceptionCode(),
                0,
                (GetExceptionInformation())->ContextRecord,
                TEXT("Expect true.")))
            {
            }

            ExitProcess(0);
        }
    }

    void ENSURE_NOT_NULL(void * pv)
    {
        if (pv == NULL)
        {
            __try
            {
                RaiseException(0, 0, 0, NULL);
            }
            __except (SEH_ExceptionFilter(
                GetExceptionCode(),
                0,
                (GetExceptionInformation())->ContextRecord,
                TEXT("Null pointer.")))
            {
            }

            ExitProcess(0);
        }
    }

    void ENSURE_OK(HRESULT hr)
    {
        if (FAILED(hr))
        {
            DWORD dwErrorCode = GetLastError();
            __try
            {
                RaiseException(0, 0, 0, NULL);
            }
            __except (SEH_ExceptionFilter(
                GetExceptionCode(),
                dwErrorCode,
                (GetExceptionInformation())->ContextRecord,
                TEXT("HRESULT error.")))
            {
            }

            ExitProcess(dwErrorCode);
        }
    }
}

namespace dx
{
    void THROW_IF_FAILED(HRESULT hr)
    {
        if (FAILED(hr))
        {
            DWORD dwErrorCode = GetLastError();
            __try
            {
                RaiseException(0, 0, 0, NULL);
            }
            __except (SEH_ExceptionFilter(
                GetExceptionCode(),
                dwErrorCode,
                (GetExceptionInformation())->ContextRecord,
                TEXT("DX HRESULT error.")))
            {
            }

            ExitProcess(dwErrorCode);
        }
    }
}
