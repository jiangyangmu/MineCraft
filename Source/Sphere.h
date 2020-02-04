#pragma once

#include <d3d11_1.h>
#include <DirectXMath.h>
#include <vector>

using namespace DirectX;

struct Sphere
{
    struct Vertex
    {
        XMFLOAT4 pos;
        XMFLOAT4 norm;
    };
    typedef uint32_t Index;

    static_assert(sizeof(Vertex) == 32, "");

    std::vector<Vertex> vertices;
    std::vector<Index> indices;
};

void CreateSphere(float diameter,
                  size_t tessellation,
                  Sphere * pSphere)
{
    // ENSURE_NOT_NULL(pSphere);
    // ENSURE_TRUE(3 <= tessellation);

    auto & vertices = pSphere->vertices;
    auto & indices = pSphere->indices;

    vertices.clear();
    indices.clear();

    size_t verticalSegments = tessellation;
    size_t horizontalSegments = tessellation * 2;
    float radius = diameter / 2;

    // Create rings of vertices at progressively higher latitudes.
    for (size_t i = 0; i <= verticalSegments; i++)
    {
        float v = 1 - (float)i / verticalSegments;
        float latitude = (i * XM_PI / verticalSegments) - XM_PIDIV2;
        float dy, dxz;
        XMScalarSinCos(&dy, &dxz, latitude);
        // Create a single ring of vertices at this latitude.
        for (size_t j = 0; j <= horizontalSegments; j++)
        {
            float u = (float)j / horizontalSegments;
            float longitude = j * XM_2PI / horizontalSegments;
            float dx, dz;
            XMScalarSinCos(&dx, &dz, longitude);
            dx *= dxz;
            dz *= dxz;
            XMVECTOR normal = XMVectorSet(dx, dy, dz, 0);
            XMVECTOR textureCoordinate = XMVectorSet(u, v, 0, 0);

            // normal * radius, normal, textureCoordinate
            XMFLOAT4 vpos, vnorm;
            XMStoreFloat4(&vpos, normal * radius);
            XMStoreFloat4(&vnorm, normal);
            vertices.push_back({ vpos, vnorm });
        }
    }
    // Fill the index buffer with triangles joining each pair of latitude rings.
    size_t stride = horizontalSegments + 1;
    for (size_t i = 0; i < verticalSegments; i++)
    {
        for (size_t j = 0; j <= horizontalSegments; j++)
        {
            size_t nextI = i + 1;
            size_t nextJ = (j + 1) % stride;
            indices.push_back(i * stride + j);
            indices.push_back(nextI * stride + j);
            indices.push_back(i * stride + nextJ);
            indices.push_back(i * stride + nextJ);
            indices.push_back(nextI * stride + j);
            indices.push_back(nextI * stride + nextJ);
        }
    }
}
