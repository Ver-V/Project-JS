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
        public int GetCurrentSelectedWeaponIndex() => _currentSelectedWeaponIndex;

        private int weaponCount;

        [SerializeField] private Button nextWeapon;
        [SerializeField] private Button prevWeapon;

        private void Awake()
        {
            if (weaponDatas == null || weaponDatas.Length == 0)
            {
                Debug.LogWarning("[WeaponSelectionUI] : WeaponSprites is null or length zero");
            }
            weaponCount = weaponDatas.Length;
            _currentSelectedWeaponIndex = 0;
            SetWeaponSelectionUI();

            nextWeapon.onClick.AddListener(ShowNextWeapon);
            prevWeapon.onClick.AddListener(ShowPrevWeapon);
        }

        private void ShowNextWeapon()
        {
            _currentSelectedWeaponIndex = _currentSelectedWeaponIndex + 1 >= weaponCount ? 0 : _currentSelectedWeaponIndex + 1;
            SetWeaponSelectionUI();
        }
        private void ShowPrevWeapon()
        {
            _currentSelectedWeaponIndex = _currentSelectedWeaponIndex - 1 < 0 ? weaponCount-1 : _currentSelectedWeaponIndex - 1;
            SetWeaponSelectionUI();
        }
        
        private void SetWeaponSelectionUI()
        {
            currentWeaponImage.sprite = weaponDatas[_currentSelectedWeaponIndex].WeaponSprite;
            currentWeaponText.text = weaponDatas[_currentSelectedWeaponIndex].WeaponName;
        }

        private void OnDestroy()
        {
            nextWeapon.onClick.RemoveAllListeners();
            prevWeapon.onClick.RemoveAllListeners();
        }
    }
}

