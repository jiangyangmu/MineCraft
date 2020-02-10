#pragma once

#include <array>
#include <vector>
#include <map>

#include "CubeRenderer.h"

namespace scene
{

    enum Type
    {
        EMPTY, // AIR
        GRASS,
    };

    typedef std::vector<Type>               TypeCube;
    typedef struct Int2 { int _0, _1; }     Int2;

    struct Position16
    {
        int bx, by, bz;
        int lx, ly, lz;

        Position16(int x, int y, int z)
            : bx(x >> 4)
            , by(y >> 4)
            , bz(z >> 4)
            , lx(x & 0xf)
            , ly(y & 0xf)
            , lz(z & 0xf)
        {}
    };
    struct Cube
    {
        Int2 xx, yy, zz;

        void    ClampBy(const Cube & c);
        int     Volumn() const;
    };

    // Memory repr + Sync to GPU instance buffer
    template <int L>
    struct BlockCube
    {
        std::array<Type, L * L * L> typeInfo;
        bool isDirty;

        BlockCube() : isDirty(false) { typeInfo.fill(Type::EMPTY); }

        Type Get(int lx, int ly, int lz) const
        {
            return typeInfo[lz * L * L + ly * L + lx];
        }
        void Set(int lx, int ly, int lz, Type t)
        {
            Type & t0 = typeInfo[lz * L * L + ly * L + lx];
            isDirty = t0 != t;
            t0 = t;
        }
        void Set(Int2 lxx, Int2 lyy, Int2 lzz, Type t)
        {
            for (int lz = lzz._0; lz <= lzz._1; ++lz)
            for (int ly = lyy._0; ly <= lyy._1; ++ly)
            for (int lx = lxx._0; lx <= lxx._1; ++lx)
            {
                typeInfo[lz * L * L + ly * L + lx] = t;
            }
            isDirty = true;
        }
        int Length() const
        {
            return L;
        }

        bool Sync(int bx, int by, int bz, render::CubeRenderer * pRenderer)
        {
            if (!isDirty)
                return false;

            isDirty = false;

            std::vector<DirectX::XMFLOAT4> buffer(
                typeInfo.size() - std::count(typeInfo.cbegin(), typeInfo.cend(), Type::EMPTY));

            Type *              pType = typeInfo.data();
            DirectX::XMFLOAT4 * pData = buffer.data();
            for (int lz = 0; lz < L; ++lz)
            for (int ly = 0; ly < L; ++ly)
            for (int lx = 0; lx < L; ++lx)
            {
                if (*pType == Type::GRASS)
                {
                    *pData =
                    {
                        static_cast<float>((bx * L + lx) * 2),
                        static_cast<float>((by * L + ly) * 2),
                        static_cast<float>((bz * L + lz) * 2),
                        1.0f
                    };
                    ++pData;
                }
                ++pType;
            }
            win32::ENSURE_TRUE(buffer.data() + buffer.size() == pData);
            pRenderer->Set(render::CubeRenderer::TEXTURE, buffer);

            return true;
        }
    };
    typedef BlockCube<16> BlockCube16;

    // Manage BlockCube + schdule sync
    class BlockWorld
    {
    public:

        // Operations

        void        Set(int x, int y, int z, Type t);
        void        Set(Int2 xx, Int2 yy, Int2 zz, Type t);
        void        Unset(int x, int y, int z);
        void        Unset(Int2 xx, Int2 yy, Int2 zz);
        Type        Query(int x, int y, int z) const;
        TypeCube    Query(Int2 xx, Int2 yy, Int2 zz) const;

        void        Sync(int cx, int cy, int cz, int nMaxUpdate = 3);

        std::vector<render::CubeRenderer *> GetAllRenderers();

    private:

        // Implementation

        BlockCube16 &           GetBlockCube(const Position16 & pos)
        {
            static size_t nextIndex = 0;

            auto & xy = tree[pos.bx][pos.by];
            auto xyz = xy.find(pos.bz);
            if (xyz != xy.end())
            {
                return xyz->second.sceneInfo;
            }
            else
            {
                Node u;
                u.rendererIndex = (nextIndex++) & 0xf;
                // win32::ENSURE_TRUE(u.rendererIndex < 16);
                return xy.emplace_hint(xyz, pos.bz, u)->second.sceneInfo;
            }
        }
        const BlockCube16 &     GetBlockCube(const Position16 & pos) const
        {
            return tree.at(pos.bx).at(pos.by).at(pos.bz).sceneInfo;
        }

        struct Node
        {
            BlockCube16     sceneInfo;
            size_t          rendererIndex;
        };
        typedef std::map<int, std::map<int, std::map<int, Node>>> NodeTree;
        typedef std::array<render::CubeRenderer, 16> RendererPool;
        
        RendererPool    renderers;
        NodeTree        tree;
    };

}