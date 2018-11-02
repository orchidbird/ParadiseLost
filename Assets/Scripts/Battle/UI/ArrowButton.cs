using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Enums;

public class ArrowButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler{
	public Direction direction;
	public Image realImage;
	public Sprite activated;
	public Sprite deactivated;
	public Button button {get {return GetComponent<Button> ();}}

	void Awake(){
		InitializeEvents ();
	}

	void InitializeEvents(){
		BattleManager battleManager = FindObjectOfType<BattleManager>();
		UnityAction UserSelectDirection = () => battleManager.CallbackDirection();
		UnityAction UserLongSelectDirection= () => {
			battleManager.CallbackDirectionLong(direction);
		};
		LeftClickEnd.AddListener (UserSelectDirection);
		LongLeftClickEnd.AddListener (UserLongSelectDirection);
	}

	float durationThreshold = 1.0f;
	bool clickStarted;
	float timeClickStarted;
	public UnityEvent LeftClickEnd;
	public UnityEvent LongLeftClickEnd;

	void IPointerDownHandler.OnPointerDown(PointerEventData pointerData){
		if (!GetComponent<Button>().interactable || pointerData.button != PointerEventData.InputButton.Left) return;
		clickStarted = true;
		timeClickStarted = Time.time;
	}
	void IPointerUpHandler.OnPointerUp(PointerEventData pointerData){
		if (!clickStarted || pointerData.button != PointerEventData.InputButton.Left
		    || TutorialController.Instance.IsScenarioOf(TutorialScenario.Mission.SelectDirection) && longClickTimeEntered) return;
		clickStarted = false;
		LeftClickEnd.Invoke ();
	}

	private bool longClickTimeEntered{get { return Time.time - timeClickStarted > durationThreshold; }}
	void OnEnable(){//초기화하는 역할
		realImage.sprite = deactivated;
		clickStarted = false;
	}
	void IPointerExitHandler.OnPointerExit(PointerEventData pointerData){
		OnEnable();
	}
	void IPointerEnterHandler.OnPointerEnter(PointerEventData pointerData){
		if(button.interactable)
			realImage.sprite = activated;
	}
	void Update(){
		if (!clickStarted || !longClickTimeEntered || TutorialController.Instance.IsScenarioOf(TutorialScenario.Mission.SelectDirection)) return;
		clickStarted = false;
		LongLeftClickEnd.Invoke ();
	}
}
