using System;
using System.IO;
using System.Reflection;
using Veldrid;

namespace Avalonia.Veldrid
{
    internal class ShaderHelper
    {
        public static Shader[] LoadShader(GraphicsDevice graphicsDevice, ResourceFactory resourceFactory,
            Assembly assembly, string name)
        {
            var ext = "vk";
            switch (graphicsDevice.BackendType)
            {
                case GraphicsBackend.Direct3D11:
                    ext = "hlsl";
                    break;
                case GraphicsBackend.Vulkan:
                    ext = "vk";
                    break;
                case GraphicsBackend.OpenGL:
                    ext = "glsl";
                    break;
                case GraphicsBackend.Metal:
                    ext = "msl";
                    break;
                case GraphicsBackend.OpenGLES:
                    ext = "essl";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new[]
            {
                CreateShader(resourceFactory, assembly, name + ".vertex." + ext, ShaderStages.Vertex),
                CreateShader(resourceFactory, assembly, name + ".fragment." + ext, ShaderStages.Fragment)
            };
        }

        private static Shader CreateShader(ResourceFactory resourceFactory, Assembly assembly, string resourceName,
            ShaderStages shaderStages)
        {
            var memoryStream = new MemoryStream();
            using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                resourceStream.CopyTo(memoryStream);
            }

            return resourceFactory.CreateShader(new ShaderDescription(shaderStages, memoryStream.ToArray(), "main"));
        }
    }
}