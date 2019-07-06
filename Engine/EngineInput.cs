﻿using AI2D.Types;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AI2D.Engine
{
    public class EngineInput
    {
        private Core _core;
        private Dictionary<PlayerKey, KeyPressState> _keyStates = new Dictionary<PlayerKey, KeyPressState>();

        public EngineInput(Core core)
        {
            _core = core;
        }

        public bool IsKeyPressed(PlayerKey key)
        {
            if (_keyStates.ContainsKey(key))
            {
                return (_keyStates[key] == KeyPressState.Down);
            }

            return false;
        }

        /// <summary>
        /// Allows the containing window to tell the engine about key press events.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="state"></param>
        public void KeyStateChanged(PlayerKey key, KeyPressState state, Keys? other = null)
        {
            if (_keyStates.ContainsKey(key))
            {
                _keyStates[key] = state;
            }
            else
            {
                _keyStates.Add(key, state);
            }
        }

        public void DebugKeyPress(Keys key)
        {
            if (key == Keys.D0)
            {
                if (_core.Actors.Enemies.Count > 0)
                {
                    _core.Actors.Enemies[0].Explode();
                }
            }
            if (key == Keys.D1)
            {
                _core.Actors.Player.Velocity.Angle.Degree = 0;
                _core.Actors.Player.Invalidate();
            }
            if (key == Keys.D2)
            {
                _core.Actors.Player.Velocity.Angle.Degree = 45;
                _core.Actors.Player.Invalidate();
            }
            if (key == Keys.D3)
            {
                _core.Actors.Player.Velocity.Angle.Degree = 90;
                _core.Actors.Player.Invalidate();
            }
            if (key == Keys.D4)
            {
                _core.Actors.Player.Velocity.Angle.Degree = 135;
                _core.Actors.Player.Invalidate();
            }
            if (key == Keys.D5)
            {
                _core.Actors.Player.Velocity.Angle.Degree = 180;
                _core.Actors.Player.Invalidate();
            }
            if (key == Keys.D6)
            {
                _core.Actors.Player.Velocity.Angle.Degree = 225;
                _core.Actors.Player.Invalidate();
            }
            if (key == Keys.D7)
            {
                _core.Actors.Player.Velocity.Angle.Degree = 270;
                _core.Actors.Player.Invalidate();
            }
            if (key == Keys.D8)
            {
                _core.Actors.Player.Velocity.Angle.Degree = 315;
                _core.Actors.Player.Invalidate();
            }
            if (key == Keys.D9)
            {
                if (_core.Actors.Enemies.Count > 0)
                {
                    _core.Actors.Enemies[0].MoveInDirectionOf(_core.Actors.Player);
                }
            }
            if (key == Keys.Escape)
            {
                //_core.Stop();
            }
        }
    }
}
