using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace CoffeeCat
{
    public class Player_Town : MonoBehaviour
    {
        // fields
        [SerializeField] private PlayerAddressablesKey playerName = PlayerAddressablesKey.NONE;
        [SerializeField] private Transform tr = null;
        private PlayerStat stat;
        private Rigidbody2D rigid;

        // properties
        public Transform Tr => tr;

        private void Start()
        {
            UpgradeCharacterView();
            rigid = GetComponent<Rigidbody2D>();
            stat = DataManager.Inst.PlayerStats.DataDictionary[playerName.ToKey()];
            TownEvtManager.AddPlayerUpgradeCompleteListener(UpgradeCharacterView);
        }

        private void OnEnable()
        {
            InputManager.AddEventJoyStickInputFixedUpdate(Move);
            InputManager.AddEventJoyStickInputEnded(MoveEnd);

#if UNITY_EDITOR || UNITY_STANDALONE
            InputManager.AddEventStandaloneInputFixedUpdate(Move);
            InputManager.AddEventStandaloneInputEnded(MoveEnd);
#endif
        }

        private void OnDisable()
        {
            InputManager.RemoveEventJoyStickInputFixedUpdate(Move);
            InputManager.RemoveEventJoyStickInputEnded(MoveEnd);

#if UNITY_EDITOR || UNITY_STANDALONE
            InputManager.RemoveEventStandaloneInputFixedUpdate(Move);
            InputManager.RemoveEventStandaloneInputEnded(MoveEnd);
            TownEvtManager.RemovePlayerUpgradeCompleteListener(UpgradeCharacterView);
#endif
        }
        
        private void UpgradeCharacterView()
        {
            var isComplete_B = UserDataManager.Inst.IsCompleteUpgrade(StatGrade.Beginner);
            if (!isComplete_B) return;
            
            var selectData = DataManager.Inst.CharacterSelectDatas;
            var index = UserDataManager.Inst.SelectedCharacterIndex;
            var dataAssetKey = selectData.GetSkeletonDataAssetKeys(index);

            dataAssetKey = dataAssetKey.IncrementSkeletonDataVersion();

            ResourceManager.Inst.AddressablesAsyncLoad<SkeletonDataAsset>(dataAssetKey, false, (data) =>
            {
                if (!data)
                {
                    CatLog.Log("Skeleton Data is Null");
                    return;
                }

                var anim = GetComponent<SkeletonAnimation>();
                anim.skeletonDataAsset = data;
                anim.initialSkinName = dataAssetKey.RemoveSuffix("_SkeletonData");
                anim.Initialize(true);
            });
        }

        private void Move(float magnitude, Vector2 direction)
        {
            var moveDirection = new Vector2(direction.x, direction.y);
            rigid.velocity = (moveDirection * stat.MoveSpeed) * magnitude;

            if (direction.x != 0f || direction.y != 0f)
            {
                SwitchingPlayerDirection(rigid.velocity.x < 0);
            }
        }

        private void MoveEnd()
        {
            rigid.velocity = Vector2.zero;
        }

        private void SwitchingPlayerDirection(bool isSwitching)
        {
            // Default Direction is Right
            // isSwitching : true -> Left, false -> Right
            var lossyScale = tr.lossyScale;
            tr.localScale = isSwitching switch
            {
                true  => new Vector3(-2f, lossyScale.y, lossyScale.z),
                false => new Vector3(2f, lossyScale.y, lossyScale.z)
            };
        }

        private void UpgradeCharacterView(SkeletonDataAsset dataAsset)
        {
            var anim = GetComponent<SkeletonAnimation>();

            anim.skeletonDataAsset = dataAsset;
            var skinName = dataAsset.name.RemoveSuffix("_SkeletonData");
            anim.initialSkinName = skinName;
            anim.Initialize(true);
        }

        public bool IsWalking() => rigid.velocity.x != 0 || rigid.velocity.y != 0;
    }
}