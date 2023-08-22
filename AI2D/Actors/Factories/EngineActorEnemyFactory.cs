﻿using AI2D.Actors.Items.Enemies;
using AI2D.Engine;
using AI2D.Engine.Managers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AI2D.Actors.Factories
{
    internal class EngineActorEnemyFactory
    {
        private readonly Core _core;
        private readonly EngineActorManager _manager;

        public EngineActorEnemyFactory(Core core, EngineActorManager manager)
        {
            _core = core;
            _manager = manager;
        }

        public void DeleteAll()
        {
            lock (_manager.Collection)
            {
                _manager.OfType<EnemyBase>().ForEach(c => c.QueueForDelete());
            }
        }

        public List<T> VisibleOfType<T>() where T : class
        {
            return (from o in _manager.OfType<EnemyBase>()
                    where o is T
                    && o.Visable == true
                    select o as T).ToList();
        }

        public void Insert(EnemyBase obj)
        {
            lock (_manager.Collection)
            {
                _manager.Collection.Add(obj);
            }
        }

        public T Create<T>() where T : EnemyBase
        {
            lock (_manager.Collection)
            {
                object[] param = { _core };
                EnemyBase obj = (EnemyBase)Activator.CreateInstance(typeof(T), param);

                obj.Location = _core.Display.RandomOffScreenLocation();
                obj.Velocity.MaxSpeed = Utility.Random.Next(Constants.Limits.MinSpeed, Constants.Limits.MaxSpeed);
                obj.Velocity.Angle.Degrees = Utility.Random.Next(0, 360);

                obj.BeforeCreate();
                _manager.Collection.Add(obj);
                obj.AfterCreate();

                return (T)obj;
            }
        }

        public void Delete(EnemyBase obj)
        {
            lock (_manager.Collection)
            {
                obj.Cleanup();
                _manager.Collection.Remove(obj);
            }
        }
    }
}
