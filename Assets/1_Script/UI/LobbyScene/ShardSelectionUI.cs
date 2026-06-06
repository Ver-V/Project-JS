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
    }
}
