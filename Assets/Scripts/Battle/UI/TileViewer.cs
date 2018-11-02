using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using GameData;
using TMPro;
using UtilityMethods;

namespace BattleUI {
    public class TileViewer : MonoBehaviour {
        Tile tile;
        Image tileImage;
        Text nameText;
        Text WillText;

        public GameObject statusEffectIconPrefab;
        public GameObject statusEffectIconsParent;
        List<StatusEffectIcon> statusEffectIcons = new List<StatusEffectIcon>();

        public void RefreshStatusEffectIconList() {
            foreach (var statusEffectIcon in statusEffectIcons) {
                Destroy(statusEffectIcon.gameObject);
            }
            statusEffectIcons = new List<StatusEffectIcon>();
        }

        public void UpdateTileViewer(Tile tile) {
            this.tile = tile;
	        Debug.Assert(tileImage != null, "TileImage is not exist : " + gameObject.GetInstanceID());
            tileImage.sprite = tile.gameObject.GetComponent<SpriteRenderer>().sprite;
            nameText.text = tile.GetTileName();
	        tileImage.transform.Find("ReqAP").GetComponent<TextMeshProUGUI>().text = tile.GetBaseMoveCost() < 200
			    ? _String.NumberToSprite(tile.GetBaseMoveCost()) : "<sprite=11>";

			WillText.text = VolatileData.OpenCheck(Setting.WillChangeOpenStage) ? Language.Select("의지 저하 ", "Will Decrease ") + -tile.WillDownFormBlood : "";
            UpdateEffect(tile);
            BattleUIManager.Instance.DeactivateStatusEffectDisplayPanel();
        }

        void UpdateEffect(Tile tile) {
            RefreshStatusEffectIconList();
            List<TileStatusEffect> effectList = tile.GetStatusEffectList();
            int numberOfEffects = effectList.Count;
            for (int i = 0; i < numberOfEffects; i++) {
                StatusEffectIcon statusEffectIcon = Instantiate(statusEffectIconPrefab, statusEffectIconsParent.transform).GetComponent<StatusEffectIcon>();
                statusEffectIcon.statusEffect = effectList[i];
                statusEffectIcon.UpdateVisual();

                float width = statusEffectIcon.GetComponent<RectTransform>().sizeDelta.x;
                statusEffectIcon.transform.localPosition = new Vector3(i * 1.25f * width, 0, 0);
                statusEffectIcons.Add(statusEffectIcon);
            }
        }

        void Awake() {
            tileImage = transform.Find("TileImage").GetComponent<Image>();
            nameText = transform.Find("NameText").GetComponent<Text>();
            WillText = transform.Find("WillText").GetComponent<Text>();

            //효과 표시 내용은 BattleReady씬에서 켜면 에러가 생기기 때문에 씬 이름으로 조건 확인하고 실행
            if (SceneManager.GetActiveScene().name == "Battle") {
                statusEffectIcons = new List<StatusEffectIcon>();
            }
        }
        public Tile GetTile() { return tile; }
    }
}
