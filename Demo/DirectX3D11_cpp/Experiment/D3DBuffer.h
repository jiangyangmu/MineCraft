#pragma once

#include <d3d11_1.h>

namespace render
{
    class D3DConstantVertexBuffer
    {
    public:
        D3DConstantVertexBuffer(ID3D11Device * pDevice);
        ~D3DConstantVertexBuffer();

        // Operations

        void                    Reset(const void * pBytes, size_t nBytes);

        // Properties

        ID3D11Buffer *          Get() { return m_d3dVertexBuffer; }

    private:
        ID3D11Device *          m_d3dDevice;
        ID3D11Buffer *          m_d3dVertexBuffer;

        size_t                  m_capacity;
        size_t                  m_size;
    };

    class D3DDynamicVertexBuffer
    {
    public:

        class Mutator
        {
            friend class D3DDynamicVertexBuffer;

        public:

            ~Mutator();

            Mutator &               Begin(ID3D11DeviceContext * pContext);
            Mutator &               FillBytes(const void * pBytes, size_t nCount);
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

        // resize and clear all data
        void                    Resize(size_t nBytes);
        Mutator                 Mutate();

        // Properties

        ID3D11Buffer *          Get() { return m_d3dVertexBuffer; }

    private:

        ID3D11Device *          m_d3dDevice;
        ID3D11Buffer *          m_d3dVertexBuffer;

        size_t                  m_capacity;
    };

    class D3DConstantIndexBuffer
    {
    public:
        D3DConstantIndexBuffer(ID3D11Device * pDevice);
        ~D3DConstantIndexBuffer();

        // Operations

        void                    Reset(const void * pBytes, size_t nBytes);

        // Properties

        ID3D11Buffer *          Get() { return m_d3dIndexBuffer; }

    private:
        ID3D11Device *          m_d3dDevice;
        ID3D11Buffer *          m_d3dIndexBuffer;

        size_t                  m_capacity;
        size_t                  m_size;
    };
}