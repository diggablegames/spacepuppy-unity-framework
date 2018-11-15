﻿#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Scenario;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Spawn
{
    public class i_SpawnWeighted : AutoTriggerableMechanism, IObservableTrigger, ISpawner
    {

        public const string TRG_ONSPAWNED = "OnSpawned";

        #region Fields

        [SerializeField()]
        private SelfTrackingSpawnerMechanism _spawnMechanism = new SelfTrackingSpawnerMechanism();

        [SerializeField]
        private Transform _spawnedObjectParent;

        [SerializeField()]
        [WeightedValueCollection("Weight", "Prefab")]
        [Tooltip("Objects available for spawning. When spawn is called with no arguments a prefab is selected at random.")]
        private List<PrefabEntry> _prefabs;

        [SerializeField()]
        private Trigger _onSpawnedObject = new Trigger();

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            _spawnMechanism.Init(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _spawnMechanism.Active = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _spawnMechanism.Active = false;
        }

        #endregion

        #region Properties

        public SpawnPool SpawnPool
        {
            get { return _spawnMechanism.SpawnPool; }
        }

        public List<PrefabEntry> Prefabs
        {
            get { return _prefabs; }
        }

        public Trigger OnSpawnedObject
        {
            get { return _onSpawnedObject; }
        }

        #endregion

        #region Methods

        public GameObject Spawn()
        {
            if (!this.CanTrigger) return null;

            if (_prefabs == null || _prefabs.Count == 0) return null;

            if (_prefabs.Count == 1)
            {
                return this.Spawn(_prefabs[0].Prefab);
            }
            else
            {
                return this.Spawn(_prefabs.PickRandom((o) => o.Weight).Prefab);
            }
        }

        public GameObject Spawn(int index)
        {
            if (!this.enabled) return null;

            if (_prefabs == null || index < 0 || index >= _prefabs.Count) return null;
            return this.Spawn(_prefabs[index].Prefab);
        }

        public GameObject Spawn(string name)
        {
            if (!this.enabled) return null;

            if (_prefabs == null) return null;
            for (int i = 0; i < _prefabs.Count; i++)
            {
                if (_prefabs[i].Prefab != null && _prefabs[i].Prefab.CompareName(name)) return this.Spawn(_prefabs[i].Prefab);
            }
            return null;
        }

        private GameObject Spawn(GameObject prefab)
        {
            if (prefab == null) return null;
            
            var go = _spawnMechanism.Spawn(prefab, this.transform.position, this.transform.rotation, _spawnedObjectParent);

            if (_onSpawnedObject != null && _onSpawnedObject.Count > 0)
                _onSpawnedObject.ActivateTrigger(this, go);

            return go;
        }

        #endregion


        #region ITriggerable Interface

        public override bool CanTrigger
        {
            get { return base.CanTrigger && _prefabs != null && _prefabs.Count > 0; }
        }

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            if (arg is string)
            {
                return this.Spawn(arg as string) != null;
            }
            else if (ConvertUtil.ValueIsNumericType(arg))
            {
                return this.Spawn(ConvertUtil.ToInt(arg)) != null;
            }
            else
            {
                return this.Spawn() != null;
            }
        }

        #endregion

        #region IObserverableTarget Interface

        Trigger[] IObservableTrigger.GetTriggers()
        {
            return new Trigger[] { _onSpawnedObject };
        }

        #endregion

        #region ISpawner Interface

        public SelfTrackingSpawnerMechanism Mechanism { get { return _spawnMechanism; } }

        public int TotalCount { get { return _spawnMechanism.TotalCount; } }

        public int ActiveCount { get { return _spawnMechanism.ActiveCount; } }

        void ISpawner.Spawn()
        {
            this.Spawn();
        }

        #endregion

        #region INotificationDispatcher Interface

        [System.NonSerialized]
        private NotificationDispatcher _observers;

        protected virtual void OnDespawn()
        {
            if (_observers != null) _observers.PurgeHandlers();
        }

        public NotificationDispatcher Observers
        {
            get
            {
                if (_observers == null) _observers = new NotificationDispatcher(this);
                return _observers;
            }
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public struct PrefabEntry
        {
            public float Weight;
            public GameObject Prefab;
        }

        #endregion

    }
}
