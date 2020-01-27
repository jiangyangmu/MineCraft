#pragma once

#include <d3d11_1.h>

namespace render
{
    interface ID3DGraphicsPipeline
    {
        // Create resources
        // * Buffers
        // * Textures
        // * Shaders
        virtual void    Initialize(ID3D11Device * d3dDevice, float aspectRatio) = 0;

        // Draw next frame
        // * Bind resources to context
        // * Configure context
        virtual void    Draw(ID3D11DeviceContext * d3dContext) = 0;
    };

    interface ID3DRenderer : public ID3DGraphicsPipeline
    {
        // Update render objects
        virtual void    Update(double milliSeconds) = 0;
    };
}