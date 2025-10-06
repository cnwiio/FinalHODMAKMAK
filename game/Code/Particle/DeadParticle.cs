using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Containers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.Timers;

namespace game
{
    /*
     IMPORTANT NOTE: Currently a bulit-in particle. Cannot custom a particle
    */
    public class DeadParticle : IDisposable, IParticle
    {
        public ParticleEffect ParticleEffect;
        private Texture2D particleTexture;
        private Texture2DRegion textureRegion;
        //private ParticleEmitter emitter;
        public Vector2 Position;
        public int Capacity, Quantity;
        public float LifeSpan;
        public Range<float> Speed;
        public DeadParticle(Game game)
        {
            particleTexture = new Texture2D(game.GraphicsDevice, 1, 1); // particle size(Ex. GraphicsDevice, 1, 1) mean 1x1 square pixel) 
            particleTexture.SetData(new[] { Color.White }); // color

            textureRegion = new Texture2DRegion(particleTexture);
            ParticleEffect = BulitInParticle();
        }
        //public void SetParameter(Vector2 position, int capacity, float lifeSpan, Range<float> speed, int quantity)
        //{
        //    Position = position;
        //    Capacity = capacity;
        //    LifeSpan = lifeSpan;
        //    Speed = speed;
        //    Quantity = quantity;
        //}
        public void Update(float deltaTime)
        {
            ParticleEffect.Update(deltaTime);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(ParticleEffect);
        }
        public void Dispose()
        {
            particleTexture.Dispose();
            ParticleEffect.Dispose();
        }
        public void Trigger(Vector2 position, Vector2 direction, float spread)
        {
            ParticleEffect.Position = position;

            if (ParticleEffect.Emitters.Count > 0)
            {
                var emitter = ParticleEffect.Emitters[0];
                emitter.Profile = Profile.Spray(direction, (float)Math.PI);
            }

            ParticleEffect.Trigger();
        }
        private ParticleEffect BulitInParticle()
        {
            return ParticleEffect = new ParticleEffect()
            {
                Position = Vector2.Zero,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 300, TimeSpan.FromSeconds(1.25), // capacity and life span
                        Profile.Spray(new Vector2(1,0), 0)) // direction and spray cone size
                    {
                        Parameters = new ParticleReleaseParameters()
                        {
                            Speed = new Range<float>(200f, 300),
                            Quantity = 15,
                            Rotation = new Range<float>(-1f, 1f)
                        },
                        Modifiers =
                        {
                            new AgeModifier()
                            {
                                Interpolators = new List<Interpolator>()
                                {
                                    new ScaleInterpolator { StartValue = new Vector2(20f), EndValue = new Vector2(0)}
                                }
                            },
                            //new OpacityFastFadeModifier(),
                            //new RotationModifier {RotationRate = -2.1f},
                            new LinearGravityModifier {Direction = Vector2.UnitY, Strength = 350f},
                        },
                        AutoTrigger = false
                    }
                }
            };
        }
    }
}
