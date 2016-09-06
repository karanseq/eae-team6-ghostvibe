using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace Helper
{
    public class Sprite
    {
        // generic sprite attributes
        protected Texture2D texture;
        protected Vector2 position;
        protected Color color;
        protected float opacity;
        protected float rotation;
        protected Vector2 origin;
        protected float scale;
        protected SpriteEffects spriteEffects;
        protected float layerDepth;

        // animation attributes
        protected bool isAnimation;
        protected bool active;
        protected int frameWidth, frameHeight, frameCount, elapsedTime, frameTime, currentFrame;
        protected bool isAnimationLooping;
        protected Rectangle sourceRect;

        // action attributes
        protected bool isPaused;
        protected HashSet<Action> actionSet;

        public Sprite()
        {
            texture = null;
            position = Vector2.Zero;
            color = Color.White;
            opacity = 1.0f;
            rotation = 0.0f;
            origin = Vector2.Zero;
            scale = 1.0f;
            spriteEffects = SpriteEffects.None;
            layerDepth = 0;

            isAnimation = false;
            active = false;
            frameWidth = frameHeight = frameCount = elapsedTime = frameTime = currentFrame = 0;
            isAnimationLooping = false;
            sourceRect = Rectangle.Empty;

            isPaused = false;
            actionSet = new HashSet<Action>();
        }

        public static Sprite Create(Texture2D texture, Vector2 position = default(Vector2), Color color = default(Color), float rotation = 0.0f, float initialScale = 1.0f, SpriteEffects spriteEffects = SpriteEffects.None, float layerDepth = 0.0f)
        {
            Sprite sprite = new Sprite();
            if (sprite.Initialize(texture, position, color, rotation, initialScale, spriteEffects, layerDepth))
            {
                return sprite;
            }
            sprite = null;
            return null;
        }

        public static Sprite Create(Texture2D texture, int frameWidth, int frameHeight, int frameCount, int frameTime = 60, bool isAnimationLooping = true, Vector2 position = default(Vector2), Color color = default(Color), float rotation = 0.0f, float initialScale = 1.0f, SpriteEffects spriteEffects = SpriteEffects.None, float layerDepth = 0.0f)
        {
            Sprite sprite = new Sprite();
            if (sprite.Initialize(texture, frameWidth, frameHeight, frameCount, frameTime, isAnimationLooping, position, color, rotation, initialScale, spriteEffects, layerDepth))
            {
                return sprite;
            }
            sprite = null;
            return null;
        }

        protected bool Initialize(Texture2D texture, Vector2 position = default(Vector2), Color color = default(Color), float rotation = 0.0f, float initialScale = 1.0f, SpriteEffects spriteEffects = SpriteEffects.None, float layerDepth = 0.0f)
        {
            // validate required inputs
            if (texture == null)
            {
                Trace.TraceError("Null texture passed to Sprite.Initialize!");
                return false;
            }

            this.texture = texture;
            this.position = position == default(Vector2) ? this.position : position;
            this.color = color == default(Color) ? this.color : color;
            this.rotation = rotation;
            this.origin = new Vector2(texture.Width / 2, texture.Height / 2);
            this.scale = initialScale;
            this.spriteEffects = spriteEffects;
            this.layerDepth = layerDepth;

            return true;
        }

        protected bool Initialize(Texture2D texture, int frameWidth, int frameHeight, int frameCount, int frameTime = 60, bool isAnimationLooping = true, Vector2 position = default(Vector2), Color color = default(Color), float rotation = 0.0f, float initialScale = 1.0f, SpriteEffects spriteEffects = SpriteEffects.None, float layerDepth = 0.0f)
        {
            if (Initialize(texture, position, color, rotation, initialScale, spriteEffects, layerDepth) == false)
            {
                return false;
            }

            this.origin.X = frameWidth / 2;
            this.origin.Y = frameHeight / 2;
            this.isAnimation = true;
            this.active = true;

            this.frameWidth = frameWidth;
            if (frameWidth <= 0)
            {
                Trace.TraceWarning("Invalid frameWidth passed to Sprite.Initialize!");
            }

            this.frameHeight = frameHeight;
            if (frameHeight <= 0)
            {
                Trace.TraceWarning("Invalid frameHeight passed to Sprite.Initialize!");
            }

            this.frameCount = frameCount;
            if (frameCount <= 0)
            {
                Trace.TraceWarning("Invalid frameCount passed to Sprite.Initialize!");
            }

            this.frameTime = frameTime;
            if (frameTime <= 0)
            {
                Trace.TraceWarning("Invalid frameTime passed to Sprite.Initialize!");
            }

            this.isAnimationLooping = isAnimationLooping;
            sourceRect = new Rectangle(0, 0, frameWidth, frameHeight);

            return true;
        }

        public virtual void Update(GameTime gameTime)
        {
            // don't update if inactive
            if (isAnimation == false || active == false)
            {
                return;
            }

            // update elapsed time
            elapsedTime += (int)gameTime.ElapsedGameTime.Milliseconds;

            // update current frame if elapsed time has exceeded frame time
            if (elapsedTime > frameTime)
            {
                // next frame
                ++currentFrame;

                if (currentFrame == frameCount)
                {
                    currentFrame = 0;
                    // deactivate the animation if it is not meant to be looped
                    if (!isAnimationLooping)
                    {
                        active = false;
                    }
                }

                // reset elapsed time
                elapsedTime = 0;
            }

            // update source rect
            sourceRect.X = currentFrame * frameWidth;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            // check if this sprite is an animation
            if (isAnimation)
            {
                // don't animate if inactive
                if (active)
                {
                    spriteBatch.Draw(texture, position, sourceRect, color * opacity, rotation, origin, scale, spriteEffects, layerDepth);
                }
            }
            else
            {
                spriteBatch.Draw(texture, position, null, color * opacity, rotation, origin, scale, spriteEffects, layerDepth);
            }
        }

        // accessors & mutators
        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public float Opacity
        {
            get { return opacity; }
            set { opacity = value; }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public Vector2 Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public SpriteEffects SpriteEffects
        {
            get { return spriteEffects; }
            set { spriteEffects = value; }
        }

        public float LayerDepth
        {
            get { return layerDepth; }
            set { layerDepth = value; }
        }

        public bool Active
        {
            get { return active; }
        }

        public int FrameWidth
        {
            get { return frameWidth; }
            set { frameWidth = value; }
        }

        public int FrameHeight
        {
            get { return frameHeight; }
            set { frameHeight = value; }
        }

        public int FrameCount
        {
            get { return frameCount; }
            set { frameCount = value; }
        }

        public int FrameTime
        {
            get { return frameTime; }
            set { frameTime = value; }
        }

        public int CurrentFrame
        {
            get { return currentFrame; }
            set { currentFrame = value; }
        }

        public bool IsAnimationLooping
        {
            get { return isAnimationLooping; }
            set { isAnimation = value; }
        }

        public bool IsPaused
        {
            get { return isPaused; }
            set { isPaused = value; }
        }

        public Rectangle SourceRect
        {
            get { return sourceRect; }
        }

        public HashSet<Action> ActionSet
        {
            get { return actionSet; }
        }

    } // class Sprite

} // namespace Helper
