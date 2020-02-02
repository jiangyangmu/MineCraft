#pragma once

#include <d3d11_1.h>

namespace render
{
    class D3DDynamicVertexBuffer
    {
    public:

        class Mutator
        {
            friend class D3DDynamicVertexBuffer;

        public:

            ~Mutator();

            Mutator &               Begin(ID3D11DeviceContext * pContext);
            Mutator &               FillBytes(const void * pObjects, size_t nCount);
            template <typename T>
            Mutator &               Fill(const T * pObjects, size_t nCount)
            {
                size_t nBytes = sizeof(T) * nCount;
                return FillBytes(pObjects, nBytes);
            }

        private:

            Mutator(ID3D11Buffer * pBuffer, size_t nCapacity);
            
            ID3D11Buffer * const    pD3DVertexBuffer;
            const size_t            nSize;

            ID3D11DeviceContext *   pD3DContext;
            BYTE *                  pData;
            size_t                  nOffset;
        };
        
        D3DDynamicVertexBuffer(ID3D11Device * pDevice);
        ~D3DDynamicVertexBuffer();

        // Operations

        void                    Resize(size_t nBytes);
        Mutator                 Mutate();

        // Properties

        ID3D11Buffer *          Get() { return m_d3dVertexBuffer; }

    private:

        ID3D11Device *          m_d3dDevice;
        ID3D11Buffer *          m_d3dVertexBuffer;

        size_t                  m_capacity;
    };
}