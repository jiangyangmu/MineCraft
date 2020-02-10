#include "pch.h"

#include "Block.h"

#include <sstream>

namespace scene
{


void Cube::ClampBy(const Cube & c)
{
    xx._0 = std::max(c.xx._0, xx._0); xx._1 = std::min(c.xx._1, xx._1);
    yy._0 = std::max(c.yy._0, yy._0); yy._1 = std::min(c.yy._1, yy._1);
    zz._0 = std::max(c.zz._0, zz._0); zz._1 = std::min(c.zz._1, zz._1);
}

int Cube::Volumn() const
{
    return (xx._1 - xx._0) * (yy._1 - yy._0) * (zz._1 - zz._0);
}

void BlockWorld::Set(int x, int y, int z, Type t)
{
    Position16 pos(x, y, z);

    BlockCube16 & bc = GetBlockCube(pos);

    bc.Set(pos.lx, pos.ly, pos.lz, t);
}

void BlockWorld::Set(Int2 xx, Int2 yy, Int2 zz, Type t)
{
    Position16 a(xx._0, yy._0, zz._0);
    Position16 b(xx._1, yy._1, zz._1);

    for (int bx = a.bx; bx <= b.bx; ++bx)
    for (int by = a.by; by <= b.by; ++by)
    for (int bz = a.bz; bz <= b.bz; ++bz)
    {
        Position16 p(bx, by, bz);
        BlockCube16 & bc = GetBlockCube(p);

        Cube c = { xx, yy, zz };
        Cube cb = { {bx, bx + bc.Length()}, {by, by + bc.Length()}, {bz, bz + bc.Length()} };
        c.ClampBy(cb);

        bc.Set(c.xx, c.yy, c.zz, t);
    }
}

void BlockWorld::Unset(int x, int y, int z)
{
    Set(x, y, z, EMPTY);
}

void BlockWorld::Unset(Int2 xx, Int2 yy, Int2 zz)
{
    Set(xx, yy, zz, EMPTY);
}

scene::Type BlockWorld::Query(int x, int y, int z) const
{
    Position16 pos(x, y, z);

    const BlockCube16 & bc = GetBlockCube(pos);

    return bc.Get(pos.lx, pos.ly, pos.lz);
}

scene::TypeCube BlockWorld::Query(Int2 xx, Int2 yy, Int2 zz) const
{
    TypeCube tc;

    Cube c = { xx, yy, zz };
    tc.reserve(c.Volumn());
    for (int x = xx._0; x <= xx._1; ++x)
        for (int y = yy._0; y <= yy._1; ++y)
            for (int z = zz._0; z <= zz._1; ++z)
            {
                Position16 p(x, y, z);
                const BlockCube16 & bc = GetBlockCube(p);

                tc.push_back(bc.Get(p.lx, p.ly, p.lz));
            }

    return tc;
}

static inline
int AbsDot(int x0, int y0, int z0,
           int x1, int y1, int z1)
{
    return std::abs((x1 - x0) * (y1 - y0) * (z1 - z0));
}

void BlockWorld::Sync(int cx, int cy, int cz, int nMaxUpdate)
{
    struct Record
    {
        int bx, by, bz;
        Node * p;
    };

    std::vector<Record> records;
    for (auto & kv0 : tree)
    for (auto & kv1 : kv0.second)
    for (auto & kv2 : kv1.second)
    {
        int bx = kv0.first;
        int by = kv1.first;
        int bz = kv2.first;
        Record r = { bx, by, bz, &kv2.second };

        records.emplace_back(r);
    }

    Position16 cpos(cx, cy, cz);
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
        if (r.p->sceneInfo.Sync(r.bx, r.by, r.bz,
                                &renderers.at(r.p->rendererIndex)))
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

std::vector<render::CubeRenderer *> BlockWorld::GetAllRenderers()
{
    std::vector<render::CubeRenderer *> r;
    r.reserve(renderers.size());
    for (render::CubeRenderer & render : renderers)
    {
        r.push_back(&render);
    }
    return r;
}

}