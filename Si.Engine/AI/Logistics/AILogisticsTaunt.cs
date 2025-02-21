﻿using Si.Engine.Sprite._Superclass;
using Si.Engine.Sprite._Superclass._Root;
using Si.GameEngine.AI._Superclass;
using Si.Library;
using Si.Library.Mathematics;
using System;
using static Si.Library.SiConstants;
namespace Si.Engine.AI.Logistics
{
    /// <summary>
    /// Keeps an object swooping past an object at an indirect angle.
    /// </summary>
    internal class AILogisticsTaunt : AIStateMachine
    {
        #region Instance parameters.

        private readonly string _boostResourceName = "AILogisticsTaunt_Boost";
        private readonly float _idealMaxDistance = SiRandom.Variance(2500, 0.20f);
        private readonly float _idealMinDistance = SiRandom.Variance(800, 0.10f);

        #endregion

        #region AI States.

        private class AIStateApproaching : AIState
        {
            public float VarianceAngle = SiRandom.Variance(45, 0.2f);
        }

        private class AIStateDeparting : AIState
        {
            public float VarianceAngle = SiRandom.Variance(45, 0.2f);
        }

        private class AIStateTransitionToApproach : AIState
        {
            public float VarianceAngle = SiRandom.Variance(45, 0.2f);
            public float Rotation = SiRandom.PositiveOrNegative();
        }

        private class AIStateTransitionToDepart : AIState
        {
            public float VarianceAngle = SiRandom.Variance(45, 0.2f);
            public float Rotation = SiRandom.PositiveOrNegative();
        }

        private class AIStateTransitionToEvasiveEscape : AIState
        {
            public float VarianceAngle = SiRandom.Variance(45, 0.2f);
            public float Rotation = SiRandom.PositiveOrNegative();
            public SiVector TargetAngle = new();

            public AIStateTransitionToEvasiveEscape(AIStateMachine machine)
            {
                TargetAngle.Degrees = machine.Owner.Orientation.Degrees + 180;
            }
        }

        class AIStateEvasiveEscape : AIState
        {
            public float VarianceAngle = SiRandom.Variance(45, 0.2f);
            public float Rotation = SiRandom.PositiveOrNegative();
        }

        #endregion

        public AILogisticsTaunt(EngineCore engine, SpriteInteractiveShipBase owner, SpriteBase observedObject)
            : base(engine, owner, observedObject)
        {
            owner.OnHit += Owner_OnHit;

            Owner.RenewableResources.Create(_boostResourceName, 800, 0, 10);

            ChangeState(new AIStateDeparting());
            Owner.RecalculateMovementVector();

            OnApplyIntelligence += AILogistics_OnApplyIntelligence;
        }

        private void Owner_OnHit(SpriteBase sender, SiDamageType damageType, int damageAmount)
        {
            if (sender.HullHealth <= 10)
            {
                ChangeState(new AIStateTransitionToEvasiveEscape(this));
            }
        }

        private void AILogistics_OnApplyIntelligence(float epoch, SiVector displacementVector, AIState state)
        {
            var distanceToObservedObject = Owner.DistanceTo(ObservedObject);

            switch (state)
            {
                //----------------------------------------------------------------------------------------------------------------------------------------------------
                //The object is moving towards the observed object.
                case AIStateApproaching approaching:
                    //Attempt to follow the observed object.
                    Owner.RotateMovementVectorIfNotPointingAt(ObservedObject, 1, approaching.VarianceAngle);

                    if (Owner.DistanceTo(ObservedObject) < _idealMinDistance)
                    {
                        ChangeState(new AIStateTransitionToDepart());
                    }
                    break;
                //----------------------------------------------------------------------------------------------------------------------------------------------------
                //The object is rotating away from the observed object.
                case AIStateTransitionToDepart transitionToDepart:
                    //As we get closer, make the angle more aggressive.
                    var rotationRadians = new SiVector((1 - (distanceToObservedObject / _idealMinDistance)) * 2.0f).RadiansSigned;

                    //Rotate as long as we are facing the observed object. If we are no longer facing, then depart.
                    if (Owner.RotateMovementVectorIfPointingAt(ObservedObject, rotationRadians * transitionToDepart.Rotation, transitionToDepart.VarianceAngle) == false)
                    {
                        ChangeState(new AIStateDeparting());
                    }

                    //Once we find the correct angle, we go into departing mode.
                    ChangeState(new AIStateDeparting());
                    break;
                //----------------------------------------------------------------------------------------------------------------------------------------------------
                //The object is moving away from the observed object.
                case AIStateDeparting departing:
                    //Once we are sufficiently far away, we turn back.
                    if (Owner.DistanceTo(ObservedObject) > _idealMaxDistance)
                    {
                        ChangeState(new AIStateTransitionToApproach());
                    }
                    break;
                //----------------------------------------------------------------------------------------------------------------------------------------------------
                //The object is rotating towards the observed object.
                case AIStateTransitionToApproach transitionToApproach:
                    //Once we find the correct angle, we go into approaching mode.
                    if (Owner.RotateMovementVectorIfNotPointingAt(ObservedObject, transitionToApproach.Rotation, transitionToApproach.VarianceAngle) == false)
                    {
                        ChangeState(new AIStateApproaching());
                    }
                    break;
                //----------------------------------------------------------------------------------------------------------------------------------------------------
                //The object is rotating aggressively away from the observed object.
                case AIStateTransitionToEvasiveEscape transitionToEvasiveEscape:
                    if (Owner.RotateMovementVectorIfNotPointingAt(transitionToEvasiveEscape.TargetAngle.Degrees, transitionToEvasiveEscape.Rotation, transitionToEvasiveEscape.VarianceAngle) == false)
                    {
                        ChangeState(new AIStateEvasiveEscape());
                    }
                    break;
                //----------------------------------------------------------------------------------------------------------------------------------------------------
                //The object is quickly moving away from the observed object.
                case AIStateEvasiveEscape evasiveEscape:
                    if (Owner.RenewableResources.Observe(_boostResourceName) > 250)
                    {
                        //Owner.AvailableBoost = Owner.RenewableResources.Consume(_boostResourceName, SiRandom.Variance(250, 0.5f));
                    }

                    if (distanceToObservedObject > _idealMaxDistance)
                    {
                        //the object got away and is now going to transition back to approach.
                        ChangeState(new AIStateTransitionToApproach());
                    }
                    else if (distanceToObservedObject < _idealMinDistance)
                    {
                        //The observed object got close again, aggressively transition away again.
                        ChangeState(new AIStateTransitionToEvasiveEscape(this));
                    }
                    break;
                //----------------------------------------------------------------------------------------------------------------------------------------------------
                default:
                    throw new Exception($"Unknown AI state: {state.GetType()}");
            }
        }
    }
}
