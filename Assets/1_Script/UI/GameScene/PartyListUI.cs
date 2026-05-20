using System.Collections.Generic;
using UnityEngine;

namespace ProjectJS.UI.GameScene
{
    public class PartyListUI : MonoBehaviour
    {
        [SerializeField] private PartyHPUI partyHPUIPrefab;

        private readonly Dictionary<Player, PartyHPUI> partySlotDict = new();

        public void RegisterPlayer(Player player)
        {
            if (player == null) return;

            if (player.IsOwner) return;

            if (partySlotDict.ContainsKey(player)) return;

            PartyHPUI partyHPUI = Instantiate(partyHPUIPrefab, transform);
            partyHPUI.Bind(player);

            partySlotDict.Add(player, partyHPUI);
        }

        public void UnregisterPlayer(Player player)
        {
            if (player == null) return;

            if (!partySlotDict.TryGetValue(player, out PartyHPUI partyHPUI)) return;

            partyHPUI.Bind(null);
            partySlotDict.Remove(player);

            Destroy(partyHPUI.gameObject);
        }

        public void Clear()
        {
            foreach (PartyHPUI partyHPUI in partySlotDict.Values)
            {
                if (partyHPUI == null) continue;

                partyHPUI.Bind(null);
                Destroy(partyHPUI.gameObject);
            }

            partySlotDict.Clear();
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}
