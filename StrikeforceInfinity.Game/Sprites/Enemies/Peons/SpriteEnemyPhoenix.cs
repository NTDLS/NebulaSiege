﻿using StrikeforceInfinity.Game.AI.Logistics;
using StrikeforceInfinity.Game.Engine;
using StrikeforceInfinity.Game.Engine.Types.Geometry;
using StrikeforceInfinity.Game.Loudouts;
using StrikeforceInfinity.Game.Sprites.Enemies.Peons.BaseClasses;
using StrikeforceInfinity.Game.Utility;
using StrikeforceInfinity.Game.Weapons;
using System;
using System.Drawing;
using System.IO;

namespace StrikeforceInfinity.Game.Sprites.Enemies.Peons
{
    internal class SpriteEnemyPhoenix : SpriteEnemyPeonBase
    {
        public const int hullHealth = 10;
        public const int bountyMultiplier = 15;

        private const string _assetPath = @"Graphics\Enemy\Phoenix\";
        private readonly int imageCount = 6;
        private readonly int selectedImageIndex = 0;

        public SpriteEnemyPhoenix(EngineCore gameCore)
            : base(gameCore, hullHealth, bountyMultiplier)
        {
            selectedImageIndex = HgRandom.Generator.Next(0, 1000) % imageCount;
            SetImage(Path.Combine(_assetPath, $"{selectedImageIndex}.png"), new Size(32, 32));

            ShipClass = HgEnemyClass.Phoenix;

            if (ControlledBy == HgControlledBy.Server)
            {
                //If this is a multiplayer drone then we need to skip most of the initilization. This is becuase
                //  the reaminder of the ctor is for adding weapons and initializing AI, none of which we need.
                return;
            }

            //Load the loadout from file or create a new one if it does not exist.
            EnemyShipLoadout loadout = LoadLoadoutFromFile(ShipClass);
            if (loadout == null)
            {
                loadout = new EnemyShipLoadout(ShipClass)
                {
                    Description = "→ Phoenix ←\n"
                       + "TODO: Add a description\n",
                    MaxSpeed = 3.5,
                    MaxBoost = 1.5,
                    HullHealth = 20,
                    ShieldHealth = 10,
                };

                loadout.Weapons.Add(new ShipLoadoutWeapon(typeof(WeaponVulcanCannon), 5000));
                loadout.Weapons.Add(new ShipLoadoutWeapon(typeof(WeaponFragMissile), 42));
                loadout.Weapons.Add(new ShipLoadoutWeapon(typeof(WeaponThunderstrikeMissile), 16));

                SaveLoadoutToFile(loadout);
            }

            ResetLoadout(loadout);

            //AddAIController(new HostileEngagement(_gameCore, this, _gameCore.Player.Sprite));
            AddAIController(new Taunt(_gameCore, this, _gameCore.Player.Sprite));
            //AddAIController(new Meander(_gameCore, this, _gameCore.Player.Sprite));

            //if (HgRandom.FlipCoin())
            //{
            SetCurrentAIController(AIControllers[typeof(Taunt)]);
            //}
            //else
            //{
            //    SetDefaultAIController(AIControllers[typeof(Meander)]);
            //}

            behaviorChangeThresholdMiliseconds = HgRandom.Between(2000, 10000);
        }

        #region Artificial Intelligence.

        DateTime lastBehaviorChangeTime = DateTime.Now;
        double behaviorChangeThresholdMiliseconds = 0;

        public override void ApplyIntelligence(SiPoint displacementVector)
        {
            if (ControlledBy == HgControlledBy.Server)
            {
                //If this is a multiplayer drone then we need to skip most of the initilization. This is becuase
                //  the reaminder of the ctor is for adding weapons and initializing AI, none of which we need.
                return;
            }

            double distanceToPlayer = HgMath.DistanceTo(this, _gameCore.Player.Sprite);

            base.ApplyIntelligence(displacementVector);

            if ((DateTime.Now - lastBehaviorChangeTime).TotalMilliseconds > behaviorChangeThresholdMiliseconds)
            {
                behaviorChangeThresholdMiliseconds = HgRandom.Between(2000, 10000);

                /*
                if (HgRandom.ChanceIn(2))
                {
                    SetDefaultAIController(AIControllers[typeof(HostileEngagement)]);
                }
                if (HgRandom.ChanceIn(2))
                {
                */
                SetCurrentAIController(AIControllers[typeof(Taunt)]);
                /*
                }
                else if (HgRandom.ChanceIn(2))
                {
                    SetDefaultAIController(AIControllers[typeof(Meander)]);
                }
                */
            }

            if (IsHostile)
            {
                if (distanceToPlayer < 1000)
                {
                    if (distanceToPlayer > 500 && HasWeaponAndAmmo<WeaponDualVulcanCannon>())
                    {
                        bool isPointingAtPlayer = IsPointingAt(_gameCore.Player.Sprite, 2.0);
                        if (isPointingAtPlayer)
                        {
                            FireWeapon<WeaponDualVulcanCannon>();
                        }
                    }
                    else if (distanceToPlayer > 0 && HasWeaponAndAmmo<WeaponVulcanCannon>())
                    {
                        bool isPointingAtPlayer = IsPointingAt(_gameCore.Player.Sprite, 2.0);
                        if (isPointingAtPlayer)
                        {
                            FireWeapon<WeaponVulcanCannon>();
                        }
                    }
                }
            }

            CurrentAIController?.ApplyIntelligence(displacementVector);
        }

        #endregion
    }
}
