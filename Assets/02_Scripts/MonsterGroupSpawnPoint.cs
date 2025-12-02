using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.Utils;

namespace CoffeeCat {
    public class MonsterGroupSpawnPoint : MonoBehaviour {
        [Title("SpawnPoints")]
        [SerializeField] private Transform[] trs = null;

        /// <summary>
        /// Do Not Use Update Event Method
        /// </summary>
        public Vector3[] SpawnPositions {
            get {
                if (trs != null) 
                    return trs.Select(tr => tr.position).ToArray();
                // Transforms is null
                CatLog.ELog("This SpawnGroup Transform Array is Null !");
                return Array.Empty<Vector3>();
            }
        }
    }
}
