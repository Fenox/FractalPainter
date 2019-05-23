using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace MyFraktalZeichner
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        Matrix worldMatrix;
        Matrix viewMatrix;
        Matrix projectionMatrix;

        BasicEffect basicEffect;
        VertexDeclaration vertexDeclaration;
        VertexPositionTexture[] pointList;
        VertexBuffer vertexBuffer;
        
        RasterizerState rasterizerState;

        int width = 600;
        int height = 600;

        int points = 4;

        Effect effect;

        short[] triangleStripIndices;

        public float MinY { get { return minY; } set { effect.Parameters["minY"].SetValue(minY = value); } }
        float minY = -1.5f;

        public float MaxY { get { return maxY; } set { effect.Parameters["maxY"].SetValue(maxY = value); } }
        float maxY = 1.5f;

        public float MinX { get { return minX; } set { effect.Parameters["minX"].SetValue(minX = value); } }
        float minX = -1.5f;

        public float MaxX { get { return maxX; } set { effect.Parameters["maxX"].SetValue(maxX = value); } }
        float maxX = 1.5f;

        public Vector2 C { get { return c; } set { effect.Parameters["c"].SetValue(c = value); } }
        Vector2 c = new Vector2(-.22f, .75f);

        public int MaxIterations { get { return maxIterations; } set { effect.Parameters["maxIterations"].SetValue(maxIterations = value); } }
        int maxIterations = 1000;

        public float Zoom { get { return maxX - minX; } set { float delta = Zoom - value; delta /= 2; MinX += delta; MaxX = MinX + value; MinY += delta; MaxY = MinY + value; } }

        KeyboardState oldKeyState;

        Fraktal Fraktal = Fraktal.Julia;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = height;
            graphics.PreferredBackBufferWidth = width;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            InitializeTransform();
            InitializeEffect();
            InitializePoints();
            InitializeTriangleStrip();

            rasterizerState = new RasterizerState();
            rasterizerState.FillMode = FillMode.Solid;
            rasterizerState.CullMode = CullMode.None;

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState state = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Add))
            {
                if(keyState.IsKeyDown(Keys.I))
                    C = new Vector2(C.X, C.Y +.001f);
                else if(keyState.IsKeyDown(Keys.R))
                    C = new Vector2(C.X +.001f, C.Y);
                else if (keyState.IsKeyDown(Keys.L))
                    MaxIterations += 5;
                else if (keyState.IsKeyDown(Keys.F))
                    Fraktal = Fraktal == MyFraktalZeichner.Fraktal.Julia ? MyFraktalZeichner.Fraktal.Mandelbrot : MyFraktalZeichner.Fraktal.Julia;
                else
                    Zoom /= 1.02f;
            }
            else if (keyState.IsKeyDown(Keys.Subtract))
            {
                if (keyState.IsKeyDown(Keys.I))
                    C = new Vector2(C.X, C.Y - .001f);
                else if (keyState.IsKeyDown(Keys.R))
                    C = new Vector2(C.X - .001f, C.Y);
                else if (keyState.IsKeyDown(Keys.L))
                    MaxIterations -= 5;
                else if (keyState.IsKeyDown(Keys.F))
                    Fraktal = Fraktal == MyFraktalZeichner.Fraktal.Julia ? MyFraktalZeichner.Fraktal.Mandelbrot : MyFraktalZeichner.Fraktal.Julia;
                else
                    Zoom *= 1.02f;
            }

            if (keyState.IsKeyDown(Keys.Left))
            {
                MaxX -= Zoom / 30;
                MinX -= Zoom / 30;
            }
            else if (keyState.IsKeyDown(Keys.Right))
            {
                MaxX += Zoom / 30;
                MinX += Zoom / 30;
            }

            if (keyState.IsKeyDown(Keys.Up))
            {
                MaxY -= Zoom / 30;
                MinY -= Zoom / 30;
            }
            else if (keyState.IsKeyDown(Keys.Down))
            {
                MaxY += Zoom / 30;
                MinY += Zoom / 30;
            }

            Window.Title = "Zoom: " + Zoom + "MinX: " + MinX + " MaxX: " + MaxX + " MinY: " + MinY + " MaxY: " + MaxY; 

            oldKeyState = keyState;
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SteelBlue); 

            GraphicsDevice.RasterizerState = rasterizerState;
            effect.Techniques["JuliaFraktal"].Passes[(int)Fraktal].Apply();
            DrawTriangleStrip();
            base.Draw(gameTime);
        }

        private void InitializePoints()
        {
            pointList = new VertexPositionTexture[points];

            for (int x = 0; x < points / 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    pointList[(x * 2) + y] = new VertexPositionTexture(
                        new Vector3(x, y, 0), new Vector2(x, y));
                }
            }

            vertexBuffer = new VertexBuffer(graphics.GraphicsDevice, vertexDeclaration, points, BufferUsage.None);
            vertexBuffer.SetData<VertexPositionTexture>(pointList);
        }

        private void InitializeEffect()
        {
            vertexDeclaration = VertexPositionTexture.VertexDeclaration;

            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.VertexColorEnabled = true;

            //worldMatrix = Matrix.Identity;
            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;

            effect = Content.Load<Effect>("Effect1");
            effect.Parameters["World"].SetValue(worldMatrix);
            effect.Parameters["View"].SetValue(viewMatrix);
            effect.Parameters["Projection"].SetValue(projectionMatrix);
        }

        private void InitializeTransform()
        {
            worldMatrix = Matrix.Identity;

            viewMatrix = Matrix.CreateLookAt(
                new Vector3(0.0f, 0.0f, 1.0f),
                Vector3.Zero,
                Vector3.Up
                );

            projectionMatrix = Matrix.CreateOrthographicOffCenter(
                0,
                1,
                1,
                0,
                1.0f, 10.0f);
        }

        private void InitializeTriangleStrip()
        {
            // Initialize an array of indices of type short.
            triangleStripIndices = new short[points];

            // Populate the array with references to indices in the vertex buffer.
            for (int i = 0; i < points; i++)
            {
                triangleStripIndices[i] = (short)i;
            }
        }

        private void DrawTriangleStrip()
        {
            pointList[0].TextureCoordinate.X = minX;
            pointList[0].TextureCoordinate.Y = minY;
            pointList[1].TextureCoordinate.X = minX;
            pointList[1].TextureCoordinate.Y = maxY;
            pointList[2].TextureCoordinate.X = maxX;
            pointList[2].TextureCoordinate.Y = minY;
            pointList[3].TextureCoordinate.X = maxX;
            pointList[3].TextureCoordinate.Y = maxY;

            GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(
                PrimitiveType.TriangleStrip,
                pointList,
                0,  // vertex buffer offset to add to each element of the index buffer
                4,  // number of vertices to draw
                triangleStripIndices,
                0,  // first index element to read
                2   // number of primitives to draw
            );
        }
    }

    enum Fraktal
    {
        Julia, Mandelbrot
    }
}
