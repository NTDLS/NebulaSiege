﻿using System.Drawing.Drawing2D;

namespace HG.Engine
{
    internal class EngineSettings
    {
        #region Debug settings.
        public bool HighlightNatrualBounds { get; set; } = true;
        public bool HighlightAllActors { get; set; } = true;
        #endregion

        public bool LockPlayerAngleToNearbyEnemy { get; set; } = false;
        public bool AutoZoomWhenMoving { get; set; } = true;

        public double EnemyThrustRampUp { get; set; } = 0.05;
        public double EnemyThrustRampDown { get; set; } = 0.01;

        public double PlayerThrustRampUp { get; set; } = 0.05;
        public double PlayerThrustRampDown { get; set; } = 0.01;

        public int MaxHitpoints { get; set; } = 100000;
        public int MaxShieldPoints { get; set; } = 100000;

        public double MaxPlayerSpeed { get; set; } = 10;
        public double MaxPlayerBoostSpeed { get; set; } = 5;
        public double MaxPlayerBoost { get; set; } = 50000;

        public double MaxRotationSpeed { get; set; } = 2;

        public int StartingPlayerHitpoints { get; set; } = 250;
        public int StartingPlayerShieldPoints { get; set; } = 25;

        public double MinPlayerThrust { get; set; } = 0; //0.25;

        public int MinSpeed { get; set; } = 3;
        public int MaxSpeed { get; set; } = 7;

        public int MinEnemyHealth { get; set; } = 2;
        public int MaxEnemyHealth { get; set; } = 20;

        public double FrameLimiter { get; set; } = 100; //~80.0 seems to be a good rate.

        public double BulletSceneDistanceLimit { get; set; } = 800; //The distance from the scene that a bullet can travel before it is cleaned up.
        public double EnemySceneDistanceLimit { get; set; } = 5000; //The distance from the scene that a enemy can travel before it is cleaned up.

        public double InfiniteScrollWallX { get; set; } = 200; //The size of the "box" where the player flies and where "infinite scrolling" begins.
        public double InfiniteScrollWallY { get; set; } = 200; //The size of the "box" where the player flies and where "infinite scrolling" begins.

        /// <summary>
        /// How much larger than the screen (NatrualScreenSize) that we will make the canvas so we can zoom-out. (1 = 100% larger or 2x).
        /// </summary>
        public double OverdrawScale { get; set; } = 1;

        public InterpolationMode GraphicsScalingMode { get; set; } = InterpolationMode.NearestNeighbor;
    }
}
