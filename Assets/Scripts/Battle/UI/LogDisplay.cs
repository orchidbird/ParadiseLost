using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Enums;
using GameData;

public class LogDisplay : MonoBehaviour {
    public Log log;
    public int index;
    public GameObject logExplanationPrefab;
    public GameObject healthBarPrefab;

    public LogExplainText CreateText(string str) {
        LogExplainText obj = Instantiate(logExplanationPrefab, transform).AddComponent<LogExplainText>();
        Text text = obj.gameObject.AddComponent<Text>();
        text.font = VolatileData.GetFont("IropkeBatangM");
        text.fontSize = 24;
        text.color = new Color(1, 1, 1);
        text.text = str;
        obj.text = text;
        obj.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        obj.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return obj;
    }
    public LogExplainImage CreateImage(Sprite sprite) {
        LogExplainImage obj = Instantiate(logExplanationPrefab, transform).AddComponent<LogExplainImage>();
        Image image = obj.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        obj.image = image;
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
        return obj;
    }
    public LogExplainTile CreateTileImage(Tile tile) {
        LogExplainTile obj = Instantiate(logExplanationPrefab, transform).AddComponent<LogExplainTile>();
        Image image = obj.gameObject.AddComponent<Image>();
        image.sprite = tile.sprite.sprite;
        obj.tile = tile;
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
        return obj;
    }
    public LogExplainUnit CreateUnitImage(Unit unit) {
        LogExplainUnit obj = Instantiate(logExplanationPrefab, transform).AddComponent<LogExplainUnit>();
        Image image = obj.gameObject.AddComponent<Image>();
        image.sprite = VolatileData.GetSpriteOf(SpriteType.Portrait, unit.CodeName);
        obj.unit = unit;
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
        return obj;
    }
    public LogExplainSkill CreateSkillImage(Skill skill, Unit caster) {
        LogExplainSkill obj = Instantiate(logExplanationPrefab, transform).AddComponent<LogExplainSkill>();
        Image image = obj.gameObject.AddComponent<Image>();
        image.sprite = skill.icon;
        obj.skill = skill;
        obj.caster = caster;
        obj.casterInfo = caster.myInfo;
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
        return obj;
    }
    public LogExplainStatusEffect CreateStatusEffectImage(StatusEffect statusEffect) {
        LogExplainStatusEffect obj = Instantiate(logExplanationPrefab, transform).AddComponent<LogExplainStatusEffect>();
        Image image = obj.gameObject.AddComponent<Image>();
        image.sprite = statusEffect.GetSprite();
        obj.statusEffect = statusEffect;
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
        return obj;
    }
    public LogExplainHealthBar CreateHealthBar(int originalHealth, int originalShield, int maxHealth, int health, int shield, Unit target) {
        LogExplainHealthBar obj = Instantiate(logExplanationPrefab, transform).AddComponent<LogExplainHealthBar>();
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 10);
        HealthViewer healthBar = Instantiate(healthBarPrefab, obj.transform).GetComponent<HealthViewer>();
        healthBar.Initialize();
        healthBar.SetInitHealth(maxHealth, target);
        healthBar.UpdatecurrentHealth(originalHealth, originalShield, maxHealth);
        healthBar.Preview(health, shield);
        obj.healthBar = healthBar;
        return obj;
    }
}
