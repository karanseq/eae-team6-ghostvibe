using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GhostVibe
{
    // For more information about Particle Engine, please visit
    // http://rbwhitaker.wikidot.com/2d-particle-engine-1
    public class ParticleEngine
    {
        private Random random;
        public Vector2 EmitterLocation { get; set; }
        private List<Particle> particles;
        private List<Texture2D> notetextures;
        private Texture2D cloudtextures;

        public ParticleEngine(List<Texture2D> notetextures, Texture2D cloudtexture, Vector2 location)
        {
            EmitterLocation = location;
            this.notetextures = notetextures;
            this.cloudtextures = cloudtexture;
            this.particles = new List<Particle>();
            random = new Random();
        }

        private Particle GenerateNewParticle(int i)
        {
            /* Texture2D texture = textures[random.Next(textures.Count)];
             Vector2 position = EmitterLocation;
             Vector2 velocity = new Vector2(
                     1f * (float)(random.NextDouble() * 2 - 1),
                     1f * (float)(random.NextDouble() * 2 - 1));
             float angle = 0;
             float angularVelocity = 0.1f * (float)(random.NextDouble() * 2 - 1);
             Color color = new Color(
                     (float)random.NextDouble(),
                     (float)random.NextDouble(),
                     (float)random.NextDouble());
             float size = (float)random.NextDouble();
             int ttl = 20 + random.Next(40);

             return new Particle(texture, position, velocity, angle, angularVelocity, color, size, ttl);*/
            Texture2D notetexture = notetextures[random.Next(notetextures.Count)];
            float radius = 80;
            Vector2 position = EmitterLocation + new Vector2((float)(radius * Math.Sin(Math.PI / 6 * i)), (float)(radius * Math.Cos(Math.PI / 6 * i)));
            float RandomVelocity = 3 + random.Next(3);
            Vector2 velocity = new Vector2((float)(RandomVelocity * Math.Sin(Math.PI / 6 * i)), (float)(RandomVelocity * Math.Cos(Math.PI / 6 * i)));
            float angle = random.Next(360);
            float angularVelocity = 0.1f * (float)(random.NextDouble() * 2 - 1);
            Color color = new Color(
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble());
            float size = (float)random.NextDouble();
            int ttl = 8 + random.Next(5);
            return new Particle(notetexture, position, velocity, angle, angularVelocity, color, size, ttl);
        }
        private Particle GenerateNewCloud(int i)
        {
            /* Texture2D texture = textures[random.Next(textures.Count)];
             Vector2 position = EmitterLocation;
             Vector2 velocity = new Vector2(
                     1f * (float)(random.NextDouble() * 2 - 1),
                     1f * (float)(random.NextDouble() * 2 - 1));
             float angle = 0;
             float angularVelocity = 0.1f * (float)(random.NextDouble() * 2 - 1);
             Color color = new Color(
                     (float)random.NextDouble(),
                     (float)random.NextDouble(),
                     (float)random.NextDouble());
             float size = (float)random.NextDouble();
             int ttl = 20 + random.Next(40);

             return new Particle(texture, position, velocity, angle, angularVelocity, color, size, ttl);*/
            Texture2D cloudtexture = cloudtextures;
            float radius = 50;
            Vector2 position = EmitterLocation + new Vector2((float)(radius * Math.Sin(Math.PI / 5 + Math.PI / 2 * i)), (float)(radius * Math.Cos(Math.PI / 6 + Math.PI / 2 * i)));
            float RandomVelocity = 0.5f;
            Vector2 velocity = new Vector2((float)(RandomVelocity * Math.Sin(Math.PI / 5 + Math.PI / 2 * i)), (float)(RandomVelocity * Math.Cos(Math.PI / 5 + Math.PI / 2 * i)));
            float angle = 0;
            if (i == 0)
                angle = 160;
            else if (i == 1)
                angle = 170;
            else if (i == 2)
                angle = 150.1f;
            else if (i == 3)
                angle = 142;
            float angularVelocity = 0.1f * (float)(random.NextDouble() * 2 - 1);
            float size = 0.08f;
            int ttl = 8 + random.Next(5);
            return new Particle(cloudtexture, position, velocity, angle, 0, Color.Red, size, ttl);
        }
        public void Update()
        {

            for (int particle = 0; particle < particles.Count; particle++)
            {
                particles[particle].Update();
                if (particles[particle].TTL <= 0)
                {
                    particles.RemoveAt(particle);
                    particle--;
                }
            }
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                if (particles.Count != 0)
                {
                    return;
                }
                int totalParticle = 12;
                int totalCloud = 4;

                for (int i = 0; i < totalParticle; i++)
                {
                    particles.Add(GenerateNewParticle(i));
                }

                for (int i = 0; i < totalCloud; ++i)
                {
                    particles.Add(GenerateNewCloud(i));
                }


            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            for (int index = 0; index < particles.Count; index++)
            {
                particles[index].Draw(spriteBatch);
            }
            spriteBatch.End();
        }
    }
}