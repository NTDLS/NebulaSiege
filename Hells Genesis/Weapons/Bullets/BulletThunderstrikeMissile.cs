﻿using HG.Engine;
using HG.Engine.Types.Geometry;
using HG.Sprites;
using HG.Utility;
using System.Drawing;
using System.IO;

namespace HG.Weapons.Bullets
{
    internal class BulletThunderstrikeMissile : _BulletBase
    {
        private const string imagePath = @"Graphics\Weapon\Missiles\BulletThunderstrikeMissile.png";

        private const string _assetPathHitExplosionAnimation = @"Graphics\Animation\Explode\Hit Explosion 66x66\";
        private readonly int _hitExplosionAnimationCount = 2;
        private int _selectedHitExplosionAnimationIndex = 0;

        public BulletThunderstrikeMissile(EngineCore core, _WeaponBase weapon, _SpriteBase firedFrom,
             _SpriteBase lockedTarget = null, HgPoint xyOffset = null)
            : base(core, weapon, firedFrom, imagePath, lockedTarget, xyOffset)
        {
            _selectedHitExplosionAnimationIndex = HgRandom.Generator.Next(0, 1000) % _hitExplosionAnimationCount;
            _hitExplosionAnimation = new SpriteAnimation(_core, Path.Combine(_assetPathHitExplosionAnimation, $"{_selectedHitExplosionAnimationIndex}.png"), new Size(66, 66));
        }

        public override void ApplyIntelligence(HgPoint displacementVector)
        {
            base.ApplyIntelligence(displacementVector);
        }
    }
}
