﻿using NebulaSiege.Engine;
using NebulaSiege.Engine.Types.Geometry;
using NebulaSiege.Sprites;
using NebulaSiege.Weapons.BaseClasses;
using NebulaSiege.Weapons.Munitions;

namespace NebulaSiege.Weapons
{
    internal class WeaponVulcanCannon : WeaponBase
    {
        static new string Name { get; } = "Vulcan Cannon";
        private const string soundPath = @"Sounds\Weapons\VulcanCannon.wav";
        private const float soundVolumne = 0.4f;

        public WeaponVulcanCannon(EngineCore core, _SpriteShipBase owner)
            : base(core, owner, Name, soundPath, soundVolumne) => InitializeWeapon();

        public WeaponVulcanCannon(EngineCore core)
            : base(core, Name, soundPath, soundVolumne) => InitializeWeapon();

        private void InitializeWeapon()
        {
            Damage = 2;
            FireDelayMilliseconds = 25;
            Speed = 20;
            AngleVarianceDegrees = 1.5;
            SpeedVariancePercent = 0.05;
            RecoilAmount = 0.05;
        }

        public override MunitionBase CreateMunition(NsPoint xyOffset, SpriteBase targetOfLock = null)
        {
            return new MunitionVulcanCannon(_core, this, _owner, xyOffset);
        }
    }
}
