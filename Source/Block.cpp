#include "pch.h"

#include "Block.h"
#include "CubeRenderer.h"

#include <array>
#include <vector>
#include <map>
#include <sstream>

namespace scene
{

typedef std::vector<BlockType>          TypeCube;
typedef struct Int2 { int _0, _1; }     Int2;

struct Cube
{
    Int2 xx, yy, zz;

    void ClampBy(const Cube & c)
    {
        xx._0 = std::max(c.xx._0, xx._0); xx._1 = std::min(c.xx._1, xx._1);
        yy._0 = std::max(c.yy._0, yy._0); yy._1 = std::min(c.yy._1, yy._1);
        zz._0 = std::max(c.zz._0, zz._0); zz._1 = std::min(c.zz._1, zz._1);
    }

    int Volumn() const
    {
        return (xx._1 - xx._0) * (yy._1 - yy._0) * (zz._1 - zz._0);
    }

};

// 1. memory repr
// 2. sync to GPU instance buffer
template <int L>
struct BlockCubeT
{
    std::array<BlockType, L * L * L> typeInfo;
    bool isDirty;

    BlockCubeT() : isDirty(false) { typeInfo.fill(BlockType::EMPTY_BLOCK); }

    BlockType Get(int lx, int ly, int lz) const
    {
        return typeInfo[lz * L * L + ly * L + lx];
    }
    void Set(int lx, int ly, int lz, BlockType t)
    {
        BlockType & t0 = typeInfo[lz * L * L + ly * L + lx];
        isDirty = t0 != t;
        t0 = t;
    }
    void Set(Int2 lxx, Int2 lyy, Int2 lzz, BlockType t)
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

    bool Sync(int bx, int by, int bz,
              render::PooledCubeRenderer * pRenderer,
              int nIndex)
    {
        if (!isDirty)
            return false;

        isDirty = false;

        std::vector<DirectX::XMFLOAT4> buffer(
            typeInfo.size() - std::count(typeInfo.cbegin(), typeInfo.cend(), BlockType::EMPTY_BLOCK));

        BlockType *         pType = typeInfo.data();
        DirectX::XMFLOAT4 * pData = buffer.data();
        for (int lz = 0; lz < L; ++lz)
        for (int ly = 0; ly < L; ++ly)
        for (int lx = 0; lx < L; ++lx)
        {
            if (*pType == BlockType::GRASS_BLOCK)
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
        
        pRenderer->SetInstanceBuffer(nIndex,
                                     render::PooledCubeRenderer::TEXTURE,
                                     std::move(buffer));

        return true;
    }
};
typedef BlockCubeT<64> BlockCube; // ~1MB

template <int TShift, int TAnd>
struct PositionT
{
    int bx, by, bz;
    int lx, ly, lz;

    PositionT(int x, int y, int z)
        : bx(x >> TShift)
        , by(y >> TShift)
        , bz(z >> TShift)
        , lx(x & TAnd)
        , ly(y & TAnd)
        , lz(z & TAnd)
    {}
};
typedef PositionT<6, 63> Position;

static inline
int AbsDot(int x0, int y0, int z0, int x1, int y1, int z1)
{
    return std::abs((x1 - x0) * (y1 - y0) * (z1 - z0));
}


// 1. manage BlockCube
// 2. schdule sync
class BlockSystemImpl
{
public:
    BlockSystemImpl() : m_nBlockCube(0) {}

    // Operations

    void        Set(int x, int y, int z, BlockType t)
    {
        Position        pos(x, y, z);
        BlockCube &     bc = GetOrCreateBlockCube(pos);

        bc.Set(pos.lx, pos.ly, pos.lz, t);
    }
    void        Set(Int2 xx, Int2 yy, Int2 zz, BlockType t)
    {
        Position a(xx._0, yy._0, zz._0);
        Position b(xx._1, yy._1, zz._1);

        for (int bx = a.bx; bx <= b.bx; ++bx)
        for (int by = a.by; by <= b.by; ++by)
        for (int bz = a.bz; bz <= b.bz; ++bz)
        {
            Position        p(bx, by, bz);
            BlockCube &     bc = GetOrCreateBlockCube(p);

            Cube            c = { xx, yy, zz };
            Cube            cb = { {bx, bx + bc.Length()}, {by, by + bc.Length()}, {bz, bz + bc.Length()} };
            
            c.ClampBy(cb);

            bc.Set(c.xx, c.yy, c.zz, t);
        }
    }   
    void        Unset(int x, int y, int z)
    {
        Set(x, y, z, EMPTY_BLOCK);
    }
    void        Unset(Int2 xx, Int2 yy, Int2 zz)
    {
        Set(xx, yy, zz, EMPTY_BLOCK);
    }
    BlockType   Query(int x, int y, int z) const
    {
        Position                pos(x, y, z);
        const BlockCube &       bc = GetBlockCube(pos);

        return bc.Get(pos.lx, pos.ly, pos.lz);
    }
    TypeCube    Query(Int2 xx, Int2 yy, Int2 zz) const
    {
        TypeCube    tc;

        Cube        c = { xx, yy, zz };

        tc.reserve(c.Volumn());
        for (int x = xx._0; x <= xx._1; ++x)
        for (int y = yy._0; y <= yy._1; ++y)
        for (int z = zz._0; z <= zz._1; ++z)
        {
            Position            p(x, y, z);
            const BlockCube &   bc = GetBlockCube(p);

            tc.push_back(
                bc.Get(p.lx, p.ly, p.lz));
        }

        return tc;
    }

    void        Sync(int cx, int cy, int cz, int nMaxUpdate)
    {
        struct Record
        {
            int bx, by, bz;
            Node * p;
        };

        std::vector<Record> records;
        for (auto & kv0 : m_worldTree)
        for (auto & kv1 : kv0.second)
        for (auto & kv2 : kv1.second)
        {
            int bx = kv0.first;
            int by = kv1.first;
            int bz = kv2.first;
            Record r = { bx, by, bz, &kv2.second };

            records.emplace_back(r);
        }

        Position cpos(cx, cy, cz);
        Record c = { cpos.bx, cpos.by, cpos.bz, nullptr };

        std::sort(records.begin(),
                  records.end(),
                  [&c] (const Record & r0, const Record & r1) -> bool
                  {
                      int d0 = AbsDot(r0.bx, r0.by, r0.bz, c.bx, c.by, c.bz);
                      int d1 = AbsDot(r1.bx, r1.by, r1.bz, c.bx, c.by, c.bz);
                      return d0 < d1;
                  });

        // sync first nMaxUpdate changed
        int nUpdated = 0;
        for (Record & r : records)
        {
            if (r.p->sceneInfo->Sync(r.bx, r.by, r.bz,
                                     m_renderer,
                                     r.p->rendererIndex))
            {
                //std::wostringstream ss;
                //ss << L"Sync block " << r.bx << L" " << r.by << L" " << r.bz << std::endl;
                //OutputDebugString(ss.str().c_str());

                ++nUpdated;
                if (nUpdated > nMaxUpdate)
                    break;
            }
        }
    }

    void        BindRenderer(render::PooledCubeRenderer * pRenderer)
    {
        m_renderer = pRenderer;
    }

private:

    // Implementation

    BlockCube &             GetOrCreateBlockCube(const Position & pos)
    {
        static size_t nextIndex = 0;

        auto & xy = m_worldTree[pos.bx][pos.by];
        auto xyz = xy.find(pos.bz);
        if (xyz != xy.end())
        {
            return *(xyz->second.sceneInfo);
        }
        else
        {
            ++m_nBlockCube;

            Node u;
            u.sceneInfo.reset(new BlockCube);
            // TODO: take renderer far from (cx, cy, cz)
            u.rendererIndex = (nextIndex++) % m_renderer->GetPoolSize();
            return *(xy.emplace_hint(xyz, pos.bz, std::move(u))->second.sceneInfo);
        }
    }
    const BlockCube &       GetBlockCube(const Position & pos) const
    {
        return *(m_worldTree.at(pos.bx).at(pos.by).at(pos.bz).sceneInfo);
    }

    struct Node
    {
        std::unique_ptr<BlockCube>  sceneInfo;
        size_t                      rendererIndex;
    };
    typedef std::map<int, std::map<int, std::map<int, Node>>> NodeTree;

    render::PooledCubeRenderer *    m_renderer;
    NodeTree                        m_worldTree;
    size_t                          m_nBlockCube;
};



BlockSystem::BlockSystem()
    : pImpl(new BlockSystemImpl)
{
}

BlockSystem::~BlockSystem()
{
    delete pImpl;
}

void BlockSystem::Set(int x, int y, int z, BlockType t)
{
    return pImpl->Set(x, y, z, t);
}

void BlockSystem::Unset(int x, int y, int z)
{
    return pImpl->Unset(x, y , z);
}

BlockType BlockSystem::Query(int x, int y, int z) const
{
    return pImpl->Query(x, y, z);
}

void BlockSystem::BindRenderer(render::PooledCubeRenderer * pRenderer)
{
    pImpl->BindRenderer(pRenderer);
}

void BlockSystem::Sync(int cx, int cy, int cz, int nMaxUpdate)
{
    pImpl->Sync(cx, cy, cz, nMaxUpdate);
}

}