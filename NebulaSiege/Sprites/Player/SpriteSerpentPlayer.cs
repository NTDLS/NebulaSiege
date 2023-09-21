﻿using NebulaSiege.Engine;
using NebulaSiege.Loudouts;
using NebulaSiege.Weapons;
using System.Drawing;
using static NebulaSiege.Loudouts.ShipLoadout;

namespace NebulaSiege.Sprites.Player
{
    internal class SpriteSerpentPlayer : _SpritePlayerBase
    {
        public SpriteSerpentPlayer(EngineCore core)
            : base(core)
        {
            ShipClass = HgPlayerClass.Serpent;

            string imagePath = @$"Graphics\Player\Ships\{ShipClass}.png";
            Initialize(imagePath, new Size(32, 32));

            //Load the loadout from file or create a new one if it does not exist.
            ShipLoadout loadout = LoadLoadoutFromFile(ShipClass);
            if (loadout == null)
            {
                loadout = new ShipLoadout(ShipClass)
                {
                    Description = "→ Stealthy Serpent ←\n"
                        + "A stealthy long distance fighter, expert in covert operations\n"
                        + "and ambushing unsuspecting adversaries with deadly precision.",
                    MaxSpeed = 5.0,
                    MaxBoost = 3.5,
                    HullHealth = 100,
                    ShieldHealth = 15000,
                    PrimaryWeapon = new ShipLoadoutWeapon(typeof(WeaponVulcanCannon), 5000)
                };
                loadout.SecondaryWeapons.Add(new ShipLoadoutWeapon(typeof(WeaponDualVulcanCannon), 2500));

                SaveLoadoutToFile(loadout);
            }

            ResetLoadout(loadout);
        }
    }
}
