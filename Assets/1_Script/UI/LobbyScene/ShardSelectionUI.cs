using ProjectJS.Manager;
using ProjectJS.Skills;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectJS.UI.LobbyScene
{
    public class ShardSelectionUI : MonoBehaviour
    {
        [SerializeField] private ShardData[] shardDatas;
        private Dictionary<ShardSpecies, ShardData> _shardDataDict = new();
        [SerializeField] private List<ShardData> ownedShardDatas;

        private int _currentSelectedShardIndex;
        public ShardSpecies CurrentSelectedShardSpecies => ownedShardDatas[_currentSelectedShardIndex].Species;
        private int _ownedShardCount;

        [SerializeField] private Image currentShardImage;
        [SerializeField] private TMP_Text currentShardNameText;
        [SerializeField] private TMP_Text currentDamageMulText;
        [SerializeField] private TMP_Text currentRangeMulText;
        [SerializeField] private TMP_Text currentCooldownMulText;

        [SerializeField] private Button nextShardButton;
        [SerializeField] private Button prevShardButton;

        private void Awake()
        {

            for (int i = 0; i < shardDatas.Length; i++)
            {
                _shardDataDict.Add(shardDatas[i].Species, shardDatas[i]);
            }

            Init();

            if (ownedShardDatas == null || ownedShardDatas.Count == 0)
            {
                Debug.LogWarning("[WeaponSelectionUI] : WeaponSprites is null or length zero");
                gameObject.SetActive(false);
                return;
            }

            _currentSelectedShardIndex = 0;
            SetShardInfo();

            nextShardButton.onClick.AddListener(ShowNextShard);
            prevShardButton.onClick.AddListener(ShowPrevShard);
        }

        private void Init()
        {
            List<ShardSpecies> ownedShardSpecies = new (SteamCloudSave.LoadGame().ownedShards);
/*
            if (ownedShardDatas == null)
            {
                ownedShardDatas = new List<ShardData>();
            }

            ownedShardDatas.Clear();

            if (shardDatas == null)
            {
                _ownedShardCount = 0;
                return;
            }

            foreach (ShardSpecies ownedSpecies in ownedShardSpecies)
            {
                foreach (ShardData shardData in shardDatas)
                {
                    if (shardData != null && shardData.Species == ownedSpecies)
                    {
                        ownedShardDatas.Add(shardData);
                        break;
                    }
                }
            }

            _ownedShardCount = ownedShardDatas.Count;
*/
            _ownedShardCount = ownedShardSpecies.Count;

            for (int i = 0; i < ownedShardSpecies.Count; i++)
            {
                ownedShardDatas.Add(_shardDataDict[ownedShardSpecies[i]]);
            }
        }

        private void SetShardInfo() 
        {
            ShardData tempData = ownedShardDatas[_currentSelectedShardIndex];
            if (tempData == null)
            {
                Debug.LogWarning("[ShardSelectionUI] : 선택된 샤드 없음");
                return;
            }

            currentShardImage.sprite = tempData.ShardSprite;
            currentShardNameText.text = tempData.ShardName;
            currentDamageMulText.text = $"추가 데미지 +{tempData.DamageMultiplier*100}%";
            currentRangeMulText.text = $"공격 사거리 +{tempData.RangeMultiplier * 100}%";
            currentCooldownMulText.text = $"쿨타임 감소 +{tempData.CooldownMultiplier * 100}%";
        }

        public ShardSpecies GetSelectedShardSpecies()
        {
            if (ownedShardDatas == null ||
                _currentSelectedShardIndex < 0 ||
                _currentSelectedShardIndex >= ownedShardDatas.Count)
            {
                return ShardSpecies.None;
            }

            ShardData selectedShard = ownedShardDatas[_currentSelectedShardIndex];
            return selectedShard != null ? selectedShard.Species : ShardSpecies.None;
        }

        private void ShowNextShard()
        {
            if (_ownedShardCount <= 0) return;
            _currentSelectedShardIndex = _currentSelectedShardIndex + 1 >= _ownedShardCount ? 0 : _currentSelectedShardIndex + 1;
            SetShardInfo();
        }
        private void ShowPrevShard()
        {
            if (_ownedShardCount <= 0) return;
            _currentSelectedShardIndex = _currentSelectedShardIndex - 1 < 0 ? _ownedShardCount - 1 : _currentSelectedShardIndex - 1;
            SetShardInfo();
        }

        private void OnDestroy()
        {
            nextShardButton.onClick.RemoveAllListeners();
            prevShardButton.onClick.RemoveAllListeners();
        }
        public void SetButtonsInteractive(bool isInteractive)
        {
            nextShardButton.interactable = isInteractive;
            prevShardButton.interactable = isInteractive;
        }
    }
}
