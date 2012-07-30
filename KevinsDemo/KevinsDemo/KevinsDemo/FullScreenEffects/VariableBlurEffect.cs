using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace KevinsDemo.FullScreenEffects
{
    /// <summary>
    /// The post process filter for applying a variable blur via the GPU.
    /// </summary>
    public class VariableBlurEffect
    {
        #region Variables

        //The shader effect object
        private Effect _blurEffect;

        //The render targets for horizontal and vertical blurs
        private RenderTarget2D _blurHorizontalRT;
        private RenderTarget2D _blurVerticalRT;
        
        //The list of parameters for each intensity of blur
        private Tuple<float[], Vector2[]>[] _blurParamsHoriz;
        private Tuple<float[], Vector2[]>[] _blurParamsVert;

        //The current blur intensity
        private float _blurIntensity;
        //The counter allowing us to keep track of where in the pulse cycle we are
        private int _blurCount;
        //The frame counter to determine which blur frame we're on
        private int _blurFrameCount;
        //The amount of time we wait inbetween each pulse
        private int _blurWaitLength;

        #endregion

        #region Constructors

        //Constructor taking in the content manager, graphics device, bounds, and blur pulse parameters
        //Beat speed in frames
        //Beat interval in frames
        public VariableBlurEffect(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice device, Rectangle renderTargetBounds, int beatNumberOfFrames, int beatNumberOfWaitFrames, float beatIntensity)
        {
            //Load the blur effect from the content manager
            _blurEffect = content.Load<Effect>("Effects/Blur");

            //Create the render target
            _blurHorizontalRT = new RenderTarget2D(device, renderTargetBounds.Width, renderTargetBounds.Height, false, device.PresentationParameters.BackBufferFormat, DepthFormat.None);
            _blurVerticalRT = new RenderTarget2D(device, renderTargetBounds.Width, renderTargetBounds.Height, false, device.PresentationParameters.BackBufferFormat, DepthFormat.None);

            //Initialize the blur parameter lists
            _blurParamsHoriz = new Tuple<float[], Vector2[]>[beatNumberOfFrames];
            _blurParamsVert = new Tuple<float[], Vector2[]>[beatNumberOfFrames];

            //Set the default blur intensity (note: 0 produces no image)
            _blurIntensity = 0.000001f;

            //initialize a counter for the blur intensity initialization as well as it's velocity
            float count = 0.000001f;
            float countVelocity = 0.4f;

            //Get the sample count from the shader
            EffectParameter weightsParameter;
            weightsParameter = _blurEffect.Parameters["SampleWeights"];
            int sampleCount = weightsParameter.Elements.Count;

            //For each blur parameter, generate the appropriate gaussian
            for (var i = 0; i < _blurParamsHoriz.Length; i++)
            {
                _blurIntensity = (float)(-Math.Cos(count) * (beatIntensity / 2)) + (beatIntensity / 2);
                if (_blurIntensity == 0) _blurIntensity = 0.000001f;
                _blurParamsHoriz[i] = GenerateBlurEffectParameters(0.001f, 0.0f, sampleCount);
                _blurParamsVert[i] = GenerateBlurEffectParameters(0.0f, 0.001f, sampleCount);
                count += countVelocity;
            }

            //Initialize the counters
            _blurCount = 0;
            _blurFrameCount = 0;
            _blurWaitLength = beatNumberOfWaitFrames;
        }

        #endregion

        #region Update Function

        public float Update(GameTime gameTime)
        {
            //Iterate the blur counter
            _blurCount = (_blurCount + 1) % (_blurParamsHoriz.Length + _blurWaitLength);

            //Determine if we're in a wait state, and what frame to generate
            if (_blurCount >= _blurParamsHoriz.Length) _blurFrameCount = 0;
            else _blurFrameCount = _blurCount;
            return (float)_blurFrameCount/(float)_blurParamsHoriz.Length;
        }

        #endregion

        #region Content Management Functions

        public void UnloadContent()
        {
            //Dispose of the render targets
            _blurVerticalRT.Dispose();
            _blurHorizontalRT.Dispose();
        }

        #endregion

        #region Frame Rendering Function

        //Render a frame using a Texture2D input
        public RenderTarget2D RenderFrame(GraphicsDevice device, SpriteBatch batch, Texture2D preprocessedFrame, GameTime gameTime)
        {
            //Render horizontally
            device.SetRenderTarget(_blurHorizontalRT);
            device.Clear(Color.Black);
            SetBlurEffectParametersIndex(true, _blurFrameCount);
            batch.Begin(0, BlendState.Opaque, null, null, null, _blurEffect);
            batch.Draw(preprocessedFrame, new Rectangle(0, 0, preprocessedFrame.Bounds.Width, preprocessedFrame.Bounds.Height), Color.White);
            batch.End();

            //Render vertically
            device.SetRenderTarget(_blurVerticalRT);
            device.Clear(Color.Black);
            SetBlurEffectParametersIndex(false, _blurFrameCount);
            batch.Begin(0, BlendState.Opaque, null, null, null, _blurEffect);
            batch.Draw(_blurHorizontalRT, new Rectangle(0, 0, preprocessedFrame.Bounds.Width, preprocessedFrame.Bounds.Height), Color.White);
            batch.End();

            return _blurVerticalRT;
        }

        //Render a frame using a RenderTarget2D
        public RenderTarget2D RenderFrame(GraphicsDevice device, SpriteBatch batch, RenderTarget2D preprocessedFrame, GameTime gameTime)
        {
            //Render horizontally
            device.SetRenderTarget(_blurHorizontalRT);
            device.Clear(Color.Black);
            SetBlurEffectParametersIndex(true, _blurFrameCount);
            batch.Begin(0, BlendState.Opaque, null, null, null, _blurEffect);
            batch.Draw(preprocessedFrame, new Rectangle(0, 0, preprocessedFrame.Bounds.Width, preprocessedFrame.Bounds.Height), Color.White);
            batch.End();

            //Render vertically
            device.SetRenderTarget(_blurVerticalRT);
            device.Clear(Color.Black);
            SetBlurEffectParametersIndex(false, _blurFrameCount);
            batch.Begin(0, BlendState.Opaque, null, null, null, _blurEffect);
            batch.Draw(_blurHorizontalRT, new Rectangle(0, 0, preprocessedFrame.Bounds.Width, preprocessedFrame.Bounds.Height), Color.White);
            batch.End();

            return _blurVerticalRT;
        }

        #endregion

        #region Internal Blur Function

        private float ComputeGaussian(float n)
        {
            float theta = _blurIntensity; //10.0f is old default

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }

        private Tuple<float[], Vector2[]> GenerateBlurEffectParameters(float dx, float dy, int sampleCount)
        {
            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
                sampleWeights[i] *= 2.0f;
            }
            return new Tuple<float[], Vector2[]>(sampleWeights, sampleOffsets);
        }

        private void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = _blurEffect.Parameters["SampleWeights"];
            offsetsParameter = _blurEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            Tuple<float[], Vector2[]> sampleParams = GenerateBlurEffectParameters(dx, dy, sampleCount);

            //Pass parameters to shader
            weightsParameter.SetValue(sampleParams.Item1);
            offsetsParameter.SetValue(sampleParams.Item2);
        }

        private void SetBlurEffectParametersIndex(bool horizontal, int index)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = _blurEffect.Parameters["SampleWeights"];
            offsetsParameter = _blurEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;
            Tuple<float[], Vector2[]> sampleParams;
            if (horizontal) //If these are horizontal blur parameters, return the horizontal parameters
                sampleParams = _blurParamsHoriz[index];
            else //If these are vertical blur parameters, return the vertical parameters
                sampleParams = _blurParamsVert[index];

            //Pass parameters to shader
            weightsParameter.SetValue(sampleParams.Item1);
            offsetsParameter.SetValue(sampleParams.Item2);
        }

        #endregion

        #region Properties

        //The intensity of the blurs
        public float BlurIntensity
        {
            get { return _blurIntensity; }
            set { _blurIntensity = value; }
        }

        //The amount of time to wait inbetween blurs
        public int BlurWaitLength
        {
            get { return _blurWaitLength; }
            set { _blurWaitLength = value; }
        }

        #endregion
    }
}