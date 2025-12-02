using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CoffeeCat.FrameWork {
    [Serializable]
    public class PoolInfo {
        // Staic Default Init Spawn Count
        public const int DEFAULT_COUNT = 10;

        public enum LoadType { 
            None,
            Caching,
            Resources,
            Addressables,
            Custom,
        }

        // Pool Object Load Type (ReadOnly)
        [TitleGroup("Information"), GUIColor(0f, 1f, 0f, 1f)]
        [ReadOnly] public LoadType PoolObjectLoadType = LoadType.None;

        // Pool Object Load Required
        [BoxGroup("Requires"), ShowIf(nameof(PoolObjectLoadType), LoadType.Caching)] 
        public GameObject PoolObject = null;

        [BoxGroup("Requires"), ShowIf(nameof(PoolObjectLoadType), LoadType.Resources), FilePath(ParentFolder = "Assets/Resources")] 
        public string ResourcesPath = string.Empty;

        [BoxGroup("Requires"), ShowIf(nameof(PoolObjectLoadType), LoadType.Addressables)] 
        public string AddressablesName = string.Empty;

        // Object Spawn Options
        [BoxGroup("Requires")] 
        public int InitSpawnCount = DEFAULT_COUNT;

        [BoxGroup("Requires")]
        public Transform CustomRootParent = null;

        public bool HasCustomRootParent => CustomRootParent;

        public bool HasRootParent { get; private set; } = true;

        public bool IsStayEnabling { get; private set; } = false;

        #region INSPECTOR BUTTONS
        
        [ButtonGroup("Information/Buttons", ButtonHeight = 25), HideInPlayMode]
        private void Caching()
        {
            ClearData();
            PoolObjectLoadType = LoadType.Caching;
        }

        [ButtonGroup("Information/Buttons", ButtonHeight = 25), HideInPlayMode]
        private void Resources()
        {
            ClearData();
            PoolObjectLoadType = LoadType.Resources;
        }

        [ButtonGroup("Information/Buttons", ButtonHeight = 25), HideInPlayMode]
        private void Addressables()
        {
            ClearData();
            PoolObjectLoadType = LoadType.Addressables;
        }

        #endregion

        private void ClearData() {
            AddressablesName = string.Empty;
            ResourcesPath = string.Empty;
            PoolObject = null;
            CustomRootParent = null;
        }
        
        #region CONSTRUCTOR
        
        /// <summary>
        /// Default Allowed Constructor
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="spawnCount"></param>
        /// <param name="customParent"></param>
        /// <returns></returns>
        public static PoolInfo Create(GameObject origin, int spawnCount = DEFAULT_COUNT, Transform customParent = null) {
            return new PoolInfo() {
                PoolObjectLoadType = LoadType.Custom,
                PoolObject = origin,
                InitSpawnCount = spawnCount,
                CustomRootParent = customParent,
            };
        }
        
        /// <summary>
        /// Not allowed public Constructor
        /// </summary>
        private PoolInfo() { }
        
        /// <summary>
        /// Bool Implicit Operator 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static implicit operator bool(PoolInfo info) => info != null;
        
        #endregion
    }
}
