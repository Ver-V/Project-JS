using ProjectJS.PStats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectJS.UI.LobbyScene
{
    public class WeaponSelectionUI : MonoBehaviour
    {
        [SerializeField] private WeaponData[] weaponDatas;

        [SerializeField] private Image currentWeaponImage;
        [SerializeField] private TMP_Text currentWeaponText;

        private int _currentSelectedWeaponIndex;
        private int _weaponCount;
        public int CurrentSelectedWeaponIndex => _currentSelectedWeaponIndex;


        [SerializeField] private Button nextWeaponButton;
        [SerializeField] private Button prevWeaponButton;

        private void Awake()
        {
            if (weaponDatas == null || weaponDatas.Length == 0)
            {
                Debug.LogWarning("[WeaponSelectionUI] : WeaponSprites is null or length zero");
            }
            _weaponCount = weaponDatas.Length;
            _currentSelectedWeaponIndex = 0;
            SetWeaponInfo();

            nextWeaponButton.onClick.AddListener(ShowNextWeapon);
            prevWeaponButton.onClick.AddListener(ShowPrevWeapon);
        }

        private void ShowNextWeapon()
        {
            _currentSelectedWeaponIndex = _currentSelectedWeaponIndex + 1 >= _weaponCount ? 0 : _currentSelectedWeaponIndex + 1;
            SetWeaponInfo();
        }
        private void ShowPrevWeapon()
        {
            _currentSelectedWeaponIndex = _currentSelectedWeaponIndex - 1 < 0 ? _weaponCount-1 : _currentSelectedWeaponIndex - 1;
            SetWeaponInfo();
        }
        
        private void SetWeaponInfo()
        {
            currentWeaponImage.sprite = weaponDatas[_currentSelectedWeaponIndex].WeaponSprite;
            currentWeaponText.text = weaponDatas[_currentSelectedWeaponIndex].WeaponName;
        }

        private void OnDestroy()
        {
            nextWeaponButton.onClick.RemoveAllListeners();
            prevWeaponButton.onClick.RemoveAllListeners();
        }
    }
}

