﻿using NebulaSiege.Engine;
using NebulaSiege.Engine.Types.Geometry;
using NebulaSiege.Managers;
using NebulaSiege.Sprites.PowerUp.BaseClasses;
using NebulaSiege.TickControllers.BaseClasses;
using System;

namespace NebulaSiege.Controller
{
    internal class PowerupSpriteTickController : SpriteTickControllerBase<SpritePowerUpBase>
    {
        public PowerupSpriteTickController(EngineCore core, EngineSpriteManager manager)
            : base(core, manager)
        {
        }

        public override void ExecuteWorldClockTick(NsPoint displacementVector)
        {
            foreach (var sprite in Visible())
            {
                sprite.ApplyIntelligence(displacementVector);
                sprite.ApplyMotion(displacementVector);
            }
        }

        public T Create<T>(double x, double y) where T : SpritePowerUpBase
        {
            lock (SpriteManager.Collection)
            {
                object[] param = { Core };
                var obj = (SpritePowerUpBase)Activator.CreateInstance(typeof(T), param);
                obj.Location = new NsPoint(x, y);
                SpriteManager.Collection.Add(obj);
                return (T)obj;
            }
        }
    }
}
