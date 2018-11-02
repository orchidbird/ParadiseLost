using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enums;
using GameData;
using UnityEngine;

public class Formula {
    // 스킬의 데미지 계산 등에 사용되는 변수타입, 계수, 기본값 등을 담는 클래스
    public FormulaVarType type;
    public float coef;
    public float basic;
    public Formula(FormulaVarType type, float coef, float basic) {
        this.type = type;
        this.coef = coef;
        this.basic = basic;
    }
    public float? Substitute(float? x) {
        if(x == null)
            return null;
        return x * coef + basic;
    }
    public float? Result(Unit unit, StatusEffect statusEffect = null) {
        Stat statType = EnumConverter.ToStat(type);
        float? value = null;
        if (statType != Stat.None)
            value = unit.GetStat(statType);
        else {
            if (type == FormulaVarType.LostHpPercent)
                value = 100 - (100 * ((float)unit.GetHP / unit.GetMaxHealth()));
            if(statusEffect != null && type == FormulaVarType.Stack)
                value = statusEffect.Stack;
        }
        return Substitute(value);
    }
    public string Result(string unitName) {
        Stat statType = EnumConverter.ToStat(type);
        if (statType != Stat.None)
            return ((int)Substitute(UnitInfo.GetStatForPC(unitName, statType))).ToString();
        return "";
    }
    public string Result(UnitInfo unitInfo) {
        Stat statType = EnumConverter.ToStat(type);
        if (statType != Stat.None) {
            if (statType == Stat.Level)
                return ((int)Substitute(RecordData.level)).ToString();
            else
                return ((int)Substitute(unitInfo.baseStats[statType])).ToString();
        }
        return "";
    }
}
