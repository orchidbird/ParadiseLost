using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Log {
    public bool executed = false;
    public LogDisplay logDisplay;
	public List<BattleTrigger> countedTriggers = new List<BattleTrigger>();
	public virtual string GetText() {
        return "";
    }
    public virtual void CreateExplainObjects() {}
	public virtual void Initialize() {}
    public virtual IEnumerator Execute() {
        yield return null;
    }
	public virtual void Rewind() {
	}
}
