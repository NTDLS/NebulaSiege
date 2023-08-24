﻿using HG.Engine.Situations;
using System.Collections.Generic;
using static HG.Engine.Situations.BaseSituation;
using static System.Windows.Forms.AxHost;

namespace HG.Engine.Managers
{
    internal class EngineSituationManager
    {
        public Core _core { get; private set; }
        public BaseSituation CurrentSituation { get; private set; }

        public List<BaseSituation> Situations = new();

        public EngineSituationManager(Core core)
        {
            _core = core;
        }

        public void ClearScenarios()
        {
            lock (Situations)
            {
                foreach (var obj in Situations)
                {
                    obj.EndSituation();
                }
            }

            CurrentSituation = null;
            Situations.Clear();
        }

        public void Reset()
        {
            lock (Situations)
            {
                ClearScenarios();

                Situations.Add(new SituationDebuggingGalore(_core));
                Situations.Add(new SituationScinzadSkirmish(_core));
                Situations.Add(new SituationIrlenFormations(_core));
                Situations.Add(new SituationAvvolAmbush(_core));
            }
        }

        /// <summary>
        /// Returns true of the situation is advanced, returns FALSE if we have have no more situations in the queue.
        /// </summary>
        /// <returns></returns>
        public bool AdvanceSituation()
        {
            lock (Situations)
            {
                if (CurrentSituation != null)
                {
                    Situations.Remove(CurrentSituation);
                }

                if (Situations.Count > 0)
                {
                    _core.Actors.HidePlayer();
                    CurrentSituation = Situations[0];
                    CurrentSituation.BeginSituation();
                }
                else
                {
                    CurrentSituation = null;
                    return false;
                }
            }
            return true;
        }

        public void AdvanceSituationIfReady()
        {
            if (CurrentSituation?.State == BaseSituation.ScenarioState.Ended)
            {
                if (AdvanceSituation() == false)
                {
                    _core.Events.QueueTheDoorIsAjar();
                }
            }
        }
    }
}
