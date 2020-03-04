using System;
using Veldrid;

namespace Avalonia.Veldrid
{
    public class AvaloniaVeldridContext : IDisposable
    {
        public AvaloniaVeldridContext(GraphicsDevice graphicsDevice, OutputDescription outputDescription)
        {
            GraphicsDevice = graphicsDevice;
            var factory = GraphicsDevice.ResourceFactory;
            var thisClass = GetType();
            var shaders = ShaderHelper.LoadShader(GraphicsDevice, factory, thisClass.Assembly,
                thisClass.Namespace + ".Shaders.FullScreen");
            SpecializationConstant[] specConstants =
            {
                //new SpecializationConstant(0, gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES)
            };
            TextureResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Input", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("InputSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            FullScreenPipeline = factory.CreateGraphicsPipeline(
                new GraphicsPipelineDescription(
                    BlendStateDescription.SingleOverrideBlend,
                    DepthStencilStateDescription.Disabled,
                    RasterizerStateDescription.CullNone,
                    PrimitiveTopology.TriangleStrip,
                    new ShaderSetDescription(new VertexLayoutDescription[] { }, shaders, specConstants),
                    new[] {TextureResourceLayout},
                    outputDescription
                ));
        }

        public GraphicsDevice GraphicsDevice { get; }

        public ResourceLayout TextureResourceLayout { get; }
        public Pipeline FullScreenPipeline { get; }

        public virtual void Dispose()
        {
            FullScreenPipeline.Dispose();
            TextureResourceLayout.Dispose();
        }
    }
}