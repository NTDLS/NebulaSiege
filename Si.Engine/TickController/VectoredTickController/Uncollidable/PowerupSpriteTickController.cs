﻿using NTDLS.Helpers;
using Si.Engine;
using Si.Engine.Manager;
using Si.Engine.Sprite.PowerUp._Superclass;
using Si.Engine.TickController._Superclass;
using Si.Library.Mathematics;
using System;

namespace Si.GameEngine.TickController.VectoredTickController.Uncollidable
{
    public class PowerupSpriteTickController : VectoredTickControllerBase<SpritePowerupBase>
    {
        public PowerupSpriteTickController(EngineCore engine, SpriteManager manager)
            : base(engine, manager)
        {
        }

        public override void ExecuteWorldClockTick(float epoch, SiVector displacementVector)
        {
            foreach (var sprite in Visible())
            {
                sprite.ApplyIntelligence(epoch, displacementVector);
                sprite.ApplyMotion(epoch, displacementVector);
            }
        }

        public T AddAt<T>(float x, float y) where T : SpritePowerupBase
        {
            object[] param = { Engine };
            var obj = (SpritePowerupBase)Activator.CreateInstance(typeof(T), param).EnsureNotNull();
            obj.Location = new SiVector(x, y);
            SpriteManager.Add(obj);
            return (T)obj;
        }
    }
}
