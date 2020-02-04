#pragma once

#include <d3d11_1.h>

namespace render
{
    // IA(vb, ib) + VS(cb, shader) + RS(viewport) + PS(texture, shader) + OM(render targets)
    interface ID3DPipelineModule
    {
        virtual         ~ID3DPipelineModule() = default;

        // Create resources
        // * Buffers
        // * Textures
        // * Shaders
        virtual void    Initialize(ID3D11Device * d3dDevice, float aspectRatio) = 0;

        // Draw next frame
        // * Bind resources to context
        // * Configure context
        virtual void    Draw(ID3D11DeviceContext * d3dContext) = 0;

        // Render
        // * Update render objects
        virtual void    Update(double milliSeconds) = 0;
    };

    using IRenderer     = ID3DPipelineModule;
}