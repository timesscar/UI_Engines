﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmptyKeys.UserInterface.Media;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using Texture2D = SiliconStudio.Xenko.Graphics.Texture;

namespace EmptyKeys.UserInterface.Renderers
{
    public class XenkoRenderer : Renderer
    {
        private static GraphicsDeviceManager manager;
        private static GraphicsContext graphicsContext;

        /// <summary>
        /// The graphics device
        /// </summary>
        /// <value>
        /// The graphics device.
        /// </value>
        public static GraphicsDevice GraphicsDevice
        {
            get
            {
                return manager.GraphicsDevice;
            }
        }

        /// <summary>
        /// Gets or sets the graphics context.
        /// </summary>
        /// <value>
        /// The graphics context.
        /// </value>
        public static GraphicsContext GraphicsContext
        {
            get
            {
                return graphicsContext;
            }

            set
            {
                graphicsContext = value;
            }
        }

        private Matrix view = Matrix.LookAtRH(new Vector3(0.0f, 0.0f, 1.0f), Vector3.Zero, Vector3.UnitY);
        private Matrix projection;
        private Size activeViewportSize;

        private SpriteBatch spriteBatch;
        private Vector2 vecPosition;
        private Vector2 vecScale;
        private Color vecColor;
        private Rectangle testRectangle;
        private Rectangle sourceRect;
        private Rectangle currentScissorRectangle;
        private Stack<Rectangle> clipRectanges;

        private bool isSpriteRenderInProgress;
        private bool isClipped;
        private Rectangle clipRectangle;        
        private MutablePipelineState geometryPipelineState;        
        private RasterizerStateDescription scissorRasterizerStateDescription;
        private RasterizerStateDescription geometryRasterizerStateDescription;

        /// <summary>
        /// Gets a value indicating whether is full screen.
        /// </summary>
        /// <value>
        /// <c>true</c> if is full screen; otherwise, <c>false</c>.
        /// </value>
        public override bool IsFullScreen
        {
            get { return manager.IsFullScreen; }
        }        

        public XenkoRenderer(GraphicsDeviceManager graphicsDeviceManager)
            : base()
        {
            manager = graphicsDeviceManager;
            spriteBatch = new SpriteBatch(manager.GraphicsDevice);
            clipRectanges = new Stack<Rectangle>();

            scissorRasterizerStateDescription = RasterizerStates.CullNone;
            scissorRasterizerStateDescription.ScissorTestEnable = true; // enables the scissor test            

            geometryRasterizerStateDescription = RasterizerStates.CullNone;
            //geometryRasterizerStateDescription.FillMode = FillMode.Wireframe;            
            geometryPipelineState = new MutablePipelineState(manager.GraphicsDevice);            
        }

        public override void Begin()
        {
            isClipped = false;
            isSpriteRenderInProgress = true;
            if (clipRectanges.Count == 0)
            {
                spriteBatch.Begin(graphicsContext);
            }
            else
            {
                Rectangle previousClip = clipRectanges.Pop();
                BeginClipped(previousClip);
            }
        }

        public override void End()
        {
            isClipped = false;
            spriteBatch.End();
            isSpriteRenderInProgress = false;
        }

        public override void BeginClipped(Rect clipRect)
        {
            clipRectangle.X = (int)clipRect.X;
            clipRectangle.Y = (int)clipRect.Y;
            clipRectangle.Width = (int)clipRect.Width;
            clipRectangle.Height = (int)clipRect.Height;

            BeginClipped(clipRectangle);
        }

        private void BeginClipped(Rectangle clipRect)
        {
            isClipped = true;
            isSpriteRenderInProgress = true;

            if (clipRectanges.Count > 0)
            {
                Rectangle previousClip = clipRectanges.Pop();
                if (previousClip.Intersects(clipRect))
                {
                    clipRect = Rectangle.Intersect(previousClip, clipRect);
                }
                else
                {
                    clipRect = previousClip;
                }

                clipRectanges.Push(previousClip);
            }

            graphicsContext.CommandList.SetScissorRectangles(clipRect.Left, clipRect.Top, clipRect.Right, clipRect.Bottom);            

            currentScissorRectangle = clipRect;
            spriteBatch.Begin(graphicsContext, SpriteSortMode.Deferred,  null, null, null, scissorRasterizerStateDescription, null, 0);
            clipRectanges.Push(clipRect);
        }

        public override void EndClipped()
        {
            isClipped = false;
            isSpriteRenderInProgress = false;
            spriteBatch.End();
            clipRectanges.Pop();
        }

        public override void DrawText(FontBase font, string text, PointF position, Size renderSize, ColorW color, PointF scale, float depth)
        {
            if (isClipped)
            {
                testRectangle.X = (int)position.X;
                testRectangle.Y = (int)position.Y;
                testRectangle.Width = (int)renderSize.Width;
                testRectangle.Height = (int)renderSize.Height;

                if (!currentScissorRectangle.Intersects(testRectangle))
                {
                    return;
                }
            }

            vecPosition.X = position.X;
            vecPosition.Y = position.Y;
            vecScale.X = scale.X;
            vecScale.Y = scale.Y;
            vecColor.A = color.A;
            vecColor.R = color.R;
            vecColor.G = color.G;
            vecColor.B = color.B;
            SpriteFont native = font.GetNativeFont() as SpriteFont;
            spriteBatch.DrawString(native, text, vecPosition, vecColor);
        }

        public override void Draw(TextureBase texture, PointF position, Size renderSize, ColorW color, bool centerOrigin)
        {
            testRectangle.X = (int)position.X;
            testRectangle.Y = (int)position.Y;
            testRectangle.Width = (int)renderSize.Width;
            testRectangle.Height = (int)renderSize.Height;
            if (isClipped && !currentScissorRectangle.Intersects(testRectangle))
            {
                return;
            }

            vecColor.A = color.A;
            vecColor.R = color.R;
            vecColor.G = color.G;
            vecColor.B = color.B;
            Texture2D native = texture.GetNativeTexture() as Texture2D;
            spriteBatch.Draw(native, testRectangle, vecColor);
        }

        public override void Draw(TextureBase texture, PointF position, Size renderSize, ColorW color, Rect source, bool centerOrigin)
        {
            testRectangle.X = (int)position.X;
            testRectangle.Y = (int)position.Y;
            testRectangle.Width = (int)renderSize.Width;
            testRectangle.Height = (int)renderSize.Height;
            if (isClipped && !currentScissorRectangle.Intersects(testRectangle))
            {
                return;
            }

            sourceRect.X = (int)source.X;
            sourceRect.Y = (int)source.Y;
            sourceRect.Width = (int)source.Width;
            sourceRect.Height = (int)source.Height;
            vecColor.A = color.A;
            vecColor.R = color.R;
            vecColor.G = color.G;
            vecColor.B = color.B;
            Texture2D native = texture.GetNativeTexture() as Texture2D;
            spriteBatch.Draw(native, testRectangle, sourceRect, vecColor, 0, Vector2.Zero);
        }

        public override Rect GetViewport()
        {
            Viewport viewport = graphicsContext.CommandList.Viewport;
            return new Rect(viewport.X, viewport.Y, viewport.Width, viewport.Height);
        }

        public override TextureBase CreateTexture(object nativeTexture)
        {
            if (nativeTexture == null)
            {
                return null;
            }

            return new XenkoTexture(nativeTexture);
        }

        public override TextureBase CreateTexture(int width, int height, bool mipmap, bool dynamic)
        {
            if (width == 0 || height == 0)
            {
                return null;
            }

            Texture2D native = null;
            if (dynamic)
            {
                native = Texture2D.New2D(GraphicsDevice, width, height, PixelFormat.R8G8B8A8_UNorm, usage: GraphicsResourceUsage.Dynamic);
            }
            else
            {
                native = Texture2D.New2D(GraphicsDevice, width, height, PixelFormat.R8G8B8A8_UNorm);
            }

            XenkoTexture texture = new XenkoTexture(native);
            return texture;
        }

        public override GeometryBuffer CreateGeometryBuffer()
        {
            return new XenkoGeometryBuffer();
        }

        public override void DrawGeometryColor(GeometryBuffer buffer, PointF position, ColorW color, float opacity, float depth)
        {
            XenkoGeometryBuffer xenkoBuffer = buffer as XenkoGeometryBuffer;

            Color4 nativeColor = new Color4(color.PackedValue) * opacity;
            xenkoBuffer.EffectInstance.Parameters.Set(SpriteEffectKeys.Color, nativeColor);
            xenkoBuffer.EffectInstance.Parameters.Set(TexturingKeys.Texture0, GraphicsDevice.GetSharedWhiteTexture());
            DrawGeometry(buffer, position, depth);
        }

        public override void DrawGeometryTexture(GeometryBuffer buffer, PointF position, TextureBase texture, float opacity, float depth)
        {
            XenkoGeometryBuffer paradoxBuffer = buffer as XenkoGeometryBuffer;
            Texture2D nativeTexture = texture.GetNativeTexture() as Texture2D;
            paradoxBuffer.EffectInstance.Parameters.Set(SpriteEffectKeys.Color, Color.White * opacity);
            paradoxBuffer.EffectInstance.Parameters.Set(TexturingKeys.Texture0, nativeTexture);
            DrawGeometry(buffer, position, depth);
        }

        private void UpdateProjection(CommandList commandList)
        {
            bool sameViewport = activeViewportSize.Width == commandList.Viewport.Width && activeViewportSize.Height == commandList.Viewport.Height;
            if (!sameViewport)
            {                
                activeViewportSize = new Size(commandList.Viewport.Width, commandList.Viewport.Height);
                projection = Matrix.OrthoOffCenterRH(0, commandList.Viewport.Width, commandList.Viewport.Height, 0, 1.0f, 1000.0f);
            }
        }

        private void DrawGeometry(GeometryBuffer buffer, PointF position, float depth)
        {
            if (isSpriteRenderInProgress)
            {
                spriteBatch.End();
            }

            Matrix world = Matrix.Translation(position.X, position.Y, 0);            

            Matrix worldView;
            Matrix.MultiplyTo(ref world, ref view, out worldView);
            
            Matrix worldViewProjection;
            UpdateProjection(graphicsContext.CommandList);
            Matrix.MultiplyTo(ref worldView, ref projection, out worldViewProjection);            

            XenkoGeometryBuffer paradoxBuffer = buffer as XenkoGeometryBuffer;            
            paradoxBuffer.EffectInstance.Parameters.Set(SpriteBaseKeys.MatrixTransform, worldViewProjection);
            paradoxBuffer.EffectInstance.Apply(graphicsContext);
            
            if (isClipped)
            {
                geometryPipelineState.State.RasterizerState = scissorRasterizerStateDescription;
            }
            else
            {
                geometryPipelineState.State.RasterizerState = geometryRasterizerStateDescription;
            }

            switch (buffer.PrimitiveType)
            {
                case GeometryPrimitiveType.TriangleList:
                    geometryPipelineState.State.PrimitiveType = PrimitiveType.TriangleList;
                    break;
                case GeometryPrimitiveType.TriangleStrip:
                    geometryPipelineState.State.PrimitiveType = PrimitiveType.TriangleStrip;
                    break;
                case GeometryPrimitiveType.LineList:
                    geometryPipelineState.State.PrimitiveType = PrimitiveType.LineList;
                    break;
                case GeometryPrimitiveType.LineStrip:
                    geometryPipelineState.State.PrimitiveType = PrimitiveType.LineStrip;
                    break;
                default:
                    break;
            }

            geometryPipelineState.State.RootSignature = paradoxBuffer.EffectInstance.RootSignature;
            geometryPipelineState.State.EffectBytecode = paradoxBuffer.EffectInstance.Effect.Bytecode;
            geometryPipelineState.State.InputElements = paradoxBuffer.InputElementDescriptions;            
            geometryPipelineState.State.Output.CaptureState(graphicsContext.CommandList);
            geometryPipelineState.Update();
            graphicsContext.CommandList.SetPipelineState(geometryPipelineState.CurrentState);            

            graphicsContext.CommandList.SetVertexBuffer(0, paradoxBuffer.VertexBufferBinding.Buffer, 0, paradoxBuffer.VertexBufferBinding.Stride);            
            graphicsContext.CommandList.Draw(paradoxBuffer.PrimitiveCount);

            if (isSpriteRenderInProgress)
            {
                if (isClipped)
                {
                    spriteBatch.Begin(graphicsContext, SpriteSortMode.Deferred, null, null, null, scissorRasterizerStateDescription, null, 0);                    
                }
                else
                {
                    spriteBatch.Begin(graphicsContext);
                }
            }
        }

        public override FontBase CreateFont(object nativeFont)
        {
            return new XenkoFont(nativeFont);
        }

        public override void ResetNativeSize()
        {
        }

        public override bool IsClipped(PointF position, Size renderSize)
        {
            if (isClipped)
            {
                testRectangle.X = (int)position.X;
                testRectangle.Y = (int)position.Y;
                testRectangle.Width = (int)renderSize.Width;
                testRectangle.Height = (int)renderSize.Height;

                if (!currentScissorRectangle.Intersects(testRectangle))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
