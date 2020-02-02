#include "pch.h"

#include "D3DBuffer.h"
#include "ErrorHandling.h"

using namespace win32;

namespace render
{

using Mutator = D3DDynamicVertexBuffer::Mutator;

Mutator::Mutator(ID3D11Buffer * pBuffer, size_t nCapacity)
    : pD3DVertexBuffer(pBuffer)
    , nSize(nCapacity)
    , pD3DContext(nullptr)
    , pData(nullptr)
    , nOffset(0)
{
}

Mutator::~Mutator()
{
    if (pD3DContext)
    {
        pD3DContext->Unmap(pD3DVertexBuffer,
                           0);
    }
}

Mutator & Mutator::Begin(ID3D11DeviceContext * pContext)
{
    D3D11_MAPPED_SUBRESOURCE mappedSubresources;

    ENSURE_TRUE(nullptr == pD3DContext);
    ENSURE_TRUE(nullptr == pData);

    pD3DContext = pContext;

    dx::THROW_IF_FAILED(
        pD3DContext->Map(pD3DVertexBuffer,
                         0,
                         D3D11_MAP_WRITE_DISCARD,
                         0,
                         &mappedSubresources));

    pData = (BYTE *)mappedSubresources.pData;

    return *this;
}

Mutator & Mutator::FillBytes(const void * pObjects, size_t nBytes)
{
    ENSURE_NOT_NULL(pD3DContext);
    ENSURE_NOT_NULL(pData);
    ENSURE_TRUE(nBytes <= (nSize - nOffset));

    CopyMemory(pData + nOffset, pObjects, nBytes);
    nOffset += nBytes;

    return *this;
}


D3DDynamicVertexBuffer::D3DDynamicVertexBuffer(ID3D11Device * pDevice)
    : m_d3dDevice(pDevice)
    , m_d3dVertexBuffer(nullptr)
    , m_capacity(0)
{
}

D3DDynamicVertexBuffer::~D3DDynamicVertexBuffer()
{
    if (m_d3dVertexBuffer)
    {
        m_d3dVertexBuffer->Release();
    }
}

void D3DDynamicVertexBuffer::Resize(size_t nBytes)
{
    // round up to 1MB aligned
    nBytes = (nBytes & 0xffff) ? ((nBytes & ~0xfffff) + 0x100000) : (nBytes & ~0xfffff);

    if (m_capacity < nBytes)
    {
        if (m_d3dVertexBuffer)
        {
            m_d3dVertexBuffer->Release();
        }

        D3D11_BUFFER_DESC vertexBufferDesc;
        vertexBufferDesc.Usage                  = D3D11_USAGE_DYNAMIC;
        vertexBufferDesc.ByteWidth              = nBytes;
        vertexBufferDesc.BindFlags              = D3D11_BIND_VERTEX_BUFFER;
        vertexBufferDesc.CPUAccessFlags         = D3D11_CPU_ACCESS_WRITE;
        vertexBufferDesc.MiscFlags              = 0;
        vertexBufferDesc.StructureByteStride    = 0;

        dx::THROW_IF_FAILED(
            m_d3dDevice->CreateBuffer(&vertexBufferDesc,
                                      nullptr,
                                      &m_d3dVertexBuffer));

        m_capacity = nBytes;
    }
}

D3DDynamicVertexBuffer::Mutator D3DDynamicVertexBuffer::Mutate()
{
    ENSURE_NOT_NULL(m_d3dVertexBuffer);

    return Mutator(m_d3dVertexBuffer, m_capacity);
}

}