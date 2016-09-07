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
        public List<Particle> particles;
        private List<Texture2D> notetextures;
        private List<Texture2D> cloudtextures;

        public ParticleEngine(List<Texture2D> notetextures, List<Texture2D> cloudtexture, Vector2 location)
        {
            EmitterLocation = location;
            this.notetextures = notetextures;
            this.cloudtextures = cloudtexture;
            this.particles = new List<Particle>();
            random = new Random();
        }

        public Particle GenerateNewParticle(int i, int lanenumber)
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
            float radius = 70;
            Vector2 position = EmitterLocation + new Vector2((float)(radius * Math.Sin(Math.PI / 12 * i)), (float)(radius * Math.Cos(Math.PI / 12 * i)));
            float RandomVelocity = 3 + random.Next(3);
            Vector2 velocity = new Vector2((float)(RandomVelocity * Math.Sin(Math.PI / 12 * i)), (float)(RandomVelocity * Math.Cos(Math.PI / 12 * i)));
            float angle = random.Next(360);
            float angularVelocity = 0.1f * (float)(random.NextDouble() * 2 - 1);
            Color color = new Color(
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble());
            float size = 1.0f;
            float opacity =1.0f;
            int ttl = 25 + random.Next(10);
            if (lanenumber == 0)
            {            
                position += new Vector2(80, -40);
            }
            else if (lanenumber == 1)
            { 
                position += new Vector2(30, -15);
            }
            else if (lanenumber == 2)
            {   
                position += new Vector2(-15, -15);
            }
            else
            {
                position += new Vector2(-60, -30);
            }
            return new Particle(notetexture, position, velocity, angle, angularVelocity, color, size,opacity, ttl);
        }
        public Particle GenerateNewCloud( int i ,int lanenumber)
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
            Texture2D cloudtexture = cloudtextures[random.Next(notetextures.Count)]; ;
            float radius = 50;
            Vector2 position = EmitterLocation + new Vector2((float)(radius * Math.Sin(Math.PI / 5 + Math.PI / 2 * i)), (float)(radius * Math.Cos(Math.PI / 6 + Math.PI / 2 * i)));
          
            float RandomVelocity = 1.5f;
            Vector2 velocity = new Vector2((float)(RandomVelocity * Math.Sin(Math.PI / 5 + Math.PI / 2 * i)), (float)(RandomVelocity * Math.Cos(Math.PI / 5 + Math.PI / 2 * i)));

            float angle = 0;
            float angularVelocity = 0.1f * (float)(random.NextDouble() * 2 - 1);
            if (i == 0)
            { 
                cloudtexture = cloudtextures[0];
             //   angle = 160;
            }
            else if (i == 1)
            {
                cloudtexture = cloudtextures[2];
            //    angle = 170;
            }
            else if (i == 2)
            {
                cloudtexture = cloudtextures[1];
              //  angle = 160;
            }
            else if (i == 3)
            {
                cloudtexture = cloudtextures[3];
                angularVelocity = 0;
              //  angle = 160;
            }
           
            float size = 0.12f;
            float opacity = 1.0f;
            int ttl = 25 + random.Next(10);
            Color cloudcolor;
            if (lanenumber == 0)
            {
                cloudcolor = Color.LightGreen;
                position += new Vector2(80, -40);
            }
            else if (lanenumber == 1)
            {
                cloudcolor = Color.Red;
                position += new Vector2(30, -15);
            }
            else if (lanenumber == 2)
            {
                cloudcolor = Color.LightBlue;
                position += new Vector2(-15, -15);
            }
            else
            {
                cloudcolor = Color.Yellow;
                position += new Vector2(-60, -30);
            }
            return new Particle(cloudtexture, position,velocity, angle, 0 , cloudcolor, size,opacity, ttl);
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
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int index = 0; index < particles.Count; index++)
            {
                particles[index].Draw(spriteBatch);
            }

        }
    }
}