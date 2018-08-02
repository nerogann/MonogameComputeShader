using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace computeShader
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    /// 
    public struct ResultData
    {
        public float functionResult;
        public int x;
        public float unused;
        public float unused2;

        public override string ToString()
        {
            return string.Format("X: {0} Y: {1}", x, functionResult);
        }
    }
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        string debug;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";

        }
        static double FACT(int n)
        {
            double tot = 1.0;
            while (n > 1)
            {
                tot *= (float)n;
                n--;
            }
            return tot;
        }

        static float MacLaurin(float x)
        {
            float tot = 1;
            for (int i = 0; i < 10; i++)
            {
                tot += (float)(Math.Pow(x, i) / FACT(i));
            }
            return tot;
        }
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            int repetition = (64 * 32) * (64 * 32);
            SharpComputeShader<ResultData> computer = new SharpComputeShader<ResultData>(graphics.GraphicsDevice, "HLSL.txt", "CS", repetition);

            Console.WriteLine("Executing Mac Laurin Series of Sin(x)");
            Console.WriteLine("With x that go from 0 to " + repetition);
            Console.WriteLine();

            //Start Compute Shader Algorithm
            debug += "\nCompute Shader Algorithm";

            //start timer
            Stopwatch st = new Stopwatch();
            st.Start();

            //execute compute shader
            computer.Begin();
            computer.Start(64, 64, 1);
            computer.End();

            //stop timer
            st.Stop();

            //get result
            ResultData[] data = computer.ReadData(repetition);


            int csTime = (int)st.ElapsedMilliseconds;

            debug += "\n"+string.Format("Time: {0} ms", csTime);

            //Start CPU Algorithm
            debug += "\n\nCPU Algorithm";
            float[] values = new float[repetition];
            st.Start();
            for (int i = 0; i < repetition; i++)
            {
                values[i] = MacLaurin(i / 1000.0F);
            }
            st.Stop();
            int cpuTime = (int)st.ElapsedMilliseconds;
            debug += "\n" + string.Format("Time: {0} ms", cpuTime);


            debug += "\n\nCheck Sample Results";

            for (int i = 1; i < 10; i++)
            {
                int x = i * 10000;
                debug += "\n"+string.Format("TEST {0} ComputeShader: {1}    |   CPU: {2} ", i, data[x].functionResult, values[x]);
            }

            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.DrawString(font, debug , new Vector2(5,5), Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
