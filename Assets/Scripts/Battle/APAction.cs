public class APAction{
	public Action action;
	public int requiredAP;

	public enum Action{
		Move,
		Skill,
		Rest,
	}

	public APAction(Action action, int requiredAP){
		this.action = action;
		this.requiredAP = requiredAP;
	}
}
