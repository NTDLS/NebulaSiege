﻿using HG.Engine;
using HG.Engine.Types.Geometry;
using HG.Sprites;

namespace HG.Weapons.Bullets
{
    internal class BulletPulseMeson : _BulletBase
    {
        private const string imagePath = @"Graphics\Weapon\BulletPulseMeson.png";

        public BulletPulseMeson(EngineCore core, _WeaponBase weapon, _SpriteBase firedFrom,
             _SpriteBase lockedTarget = null, HgPoint xyOffset = null)
            : base(core, weapon, firedFrom, imagePath, lockedTarget, xyOffset)
        {
            Initialize(imagePath);
        }
    }
}
