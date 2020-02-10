#pragma once

namespace render
{
    class PooledCubeRenderer;
}

namespace scene
{

    enum BlockType
    {
        EMPTY_BLOCK,
        GRASS_BLOCK,
    };

    class BlockSystem
    {
    public:
        BlockSystem();
        ~BlockSystem();

        void        Set(int x, int y, int z, BlockType t);
        void        Unset(int x, int y, int z);
        void        Sync(int cx, int cy, int cz, int nMaxUpdate = 3);
        BlockType   Query(int x, int y, int z) const;

        void        BindRenderer(render::PooledCubeRenderer * pRenderer);

    private:

        class BlockSystemImpl * pImpl;
    };
}