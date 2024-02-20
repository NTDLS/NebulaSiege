﻿using Si.GameEngine.Core;
using Si.GameEngine.Sprites.Powerup._Superclass;
using Si.Library;
using Si.Library.Types.Geometry;

namespace Si.GameEngine.Sprites.Powerup
{
    internal class SpritePowerupRepair : SpritePowerupBase
    {
        private readonly int imageCount = 3;

        public SpritePowerupRepair(GameEngineCore gameEngine)
            : base(gameEngine)
        {
            PowerupAmount = 100;

            int multiplier = SiRandom.Generator.Next(0, 1000) % imageCount;
            SetImage(@$"Graphics\Powerup\Repair\{multiplier}.png");
            PowerupAmount *= multiplier + 1;
        }

        public override void ApplyIntelligence(SiPoint displacementVector)
        {
            if (Intersects(_gameEngine.Player.Sprite))
            {
                _gameEngine.Player.Sprite.AddHullHealth(PowerupAmount);
                Explode();
            }
            else if (AgeInMilliseconds > TimeToLive)
            {
                Explode();
            }
        }
    }
}
