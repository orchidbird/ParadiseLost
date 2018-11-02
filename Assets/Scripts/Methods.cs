using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using GameData;
using Enums;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UtilityMethods{
	public static class Language{
		public static string Select(string kor, string eng){
			return VolatileData.language == Lang.Kor ? kor : eng;
		}

		public static string Find(string codeName){
			string[] row = Parser.FindRowOf(TranslateUI.Table, codeName);
			Debug.Assert(row != null, "번역 실패: " + codeName + "을 찾을 수 없음!");
			if (row == null) return "";
			return row[(int) VolatileData.language + 1].Replace("^", "\n");
		}
	}

	public static class Calculate{
		public static int DistanceToUnit(Vector2Int pos, Unit unit){
			return unit == null ? -1 : Distance(pos, unit.Pos);
		}
		public static int Distance(Tile tile1, Tile tile2, int lowerTileFactor = 0){
			var result = Math.Abs(tile1.Location.x - tile2.Location.x) + Math.Abs(tile1.Location.y - tile2.Location.y);
			var calcType = (LowerTileDistCalcType) lowerTileFactor;
			if (calcType == LowerTileDistCalcType.Flat || SceneManager.GetActiveScene().name == "BattleReady")
				return result;
	    
			var heightDiff = (tile1.GetHeight() - tile2.GetHeight()) / 2;
			if (calcType == LowerTileDistCalcType.Bonus)
				result -= heightDiff;
			else if(calcType == LowerTileDistCalcType.Penalty)
				result += Math.Abs(heightDiff);
			
			return Math.Max(result, 0);
		}
		public static int Distance(Entity ent1, Entity ent2, int lowerTileFactor = 0){
			return Distance(ent1.Pos, ent2.Pos, (LowerTileDistCalcType)lowerTileFactor);
		}
		public static int Distance(Vector2Int pos1, Vector2Int pos2, LowerTileDistCalcType calcType = LowerTileDistCalcType.Flat){ 
			var result = Math.Abs(pos1.x - pos2.x) + Math.Abs(pos1.y - pos2.y);
			if (calcType == LowerTileDistCalcType.Flat || SceneManager.GetActiveScene().name == "BattleReady")
				return result;

			//lowerTileFactor가 -1이면 낮은 타일의 거리를 더 가깝게 적용(지정범위), +1이면 더 멀게 적용(유효범위), 0이면 무시(단순 거리)
			var tile1 = TileManager.Instance.GetTile(pos1);
			var tile2 = TileManager.Instance.GetTile(pos2);
			if (tile1 == null || tile2 == null) return result;
	    
			var heightDiff = (tile1.GetHeight() - tile2.GetHeight()) / 2;
			if (calcType == LowerTileDistCalcType.Bonus)
				result -= heightDiff;
			else if(calcType == LowerTileDistCalcType.Penalty)
				result += Math.Abs(heightDiff);
			
			return Math.Max(result, 0);
		}
		
		public static Vector2Int NormalizeV2I(Vector2Int input){ //직선인 경우(돌진/당기기 등), 즉 x와 y 중 하나가 0인 경우에만 사용 가능!
			return new Vector2Int(ScaleOfOne(input.x), ScaleOfOne(input.y));
		}
		static int ScaleOfOne(int input){
			if (input == 0) return 0;
			if (input > 0) return 1;
			return -1;
		}
		
		public static Direction FinalDirOfPath(List<Tile> path, Direction originalDirection){
			return path.Count < 2 ? 
				originalDirection : _Convert.Vec2ToDir(path.Last().Pos - path[path.Count-2].Pos);
		}
		
		public static Direction GetMouseDirFromUnit(Unit unit, Direction originDir){
			Debug.Assert(unit != null);
			Vector2 unitPosition = unit.realPosition;

			string directionString = "";
			Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

			if (unit.TileUnderUnit.isMouseOver) { return originDir; }

			if (mousePosition.x < unitPosition.x)
				directionString += "Left";
			else
				directionString += "Right";

			if (mousePosition.y > unitPosition.y)
				directionString += "Up";
			else
				directionString += "Down";

			return ChangeWithAspect((Direction)Enum.Parse(typeof(Direction), directionString));
		}

		public static Direction GetDirectionTo(Entity depart, Entity dest, Direction originDir){
			Direction dir = originDir;
			if (dest == null) return dir;
			
			var xDiff = dest.Pos.x - depart.Pos.x;
			var yDiff = dest.Pos.y - depart.Pos.y;
			
			if (xDiff == 0 && yDiff == 0) return dir;
			
			if (Math.Abs(xDiff) == Math.Abs(yDiff)){
				if (xDiff > 0 && yDiff > 0)
					dir = dir == Direction.RightUp || dir == Direction.LeftUp
						? Direction.RightUp : Direction.RightDown;
				else if (xDiff > 0 && yDiff < 0)
					dir = dir == Direction.RightUp || dir == Direction.RightDown
						? Direction.RightDown : Direction.LeftDown;
				else if (xDiff < 0 && yDiff < 0)
					dir = dir == Direction.RightUp || dir == Direction.LeftUp
						? Direction.LeftUp : Direction.LeftDown;
				else if (xDiff < 0 && yDiff > 0)
					dir = dir == Direction.RightUp || dir == Direction.RightDown
						? Direction.RightUp : Direction.LeftUp;
			}else{
				if (Math.Abs(xDiff) > Math.Abs(yDiff) && xDiff > 0)
					dir = Direction.RightDown;
				if (Math.Abs(xDiff) > Math.Abs(yDiff) && xDiff < 0)
					dir = Direction.LeftUp;
				if (Math.Abs(xDiff) < Math.Abs(yDiff) && yDiff > 0)
					dir = Direction.RightUp;
				if (Math.Abs(xDiff) < Math.Abs(yDiff) && yDiff < 0)
					dir = Direction.LeftDown;	
			}

			return dir;
		}

		static Direction ChangeWithAspect(Direction input){
			return (Direction)(((int)input - (int)BattleData.aspect + 4) % 4);
		}
		
		public static Vector3 Lerp(Vector3 A, Vector3 B, float t) {
			return new Vector3(Mathf.Lerp(A.x, B.x, t), Mathf.Lerp(A.y, B.y, t), Mathf.Lerp(A.z, B.z, t));
		}

		public static float StatLevel(Stat statType, float input){
			if (statType == Stat.MaxHealth){
				var valueInRoot = 906 + 4.08f * (input - 387);
				int signOfValueInRoot = valueInRoot > 0 ? 1 : -1;
				return (-30.1f + (float)Math.Sqrt(Math.Abs(valueInRoot))*signOfValueInRoot) / 2.04f;
			}
			if (statType == Stat.Power){
				var valueInRoot = 25.8f + 0.688f * (input - 72);
				int signOfValueInRoot = valueInRoot > 0 ? 1 : -1;
				return (-5.08f + (float)Math.Sqrt(Math.Abs(valueInRoot))*signOfValueInRoot) / 0.344f;
			}
			if(statType == Stat.Defense || statType == Stat.Resistance)
				return (input - 64) / 3.5f;
			if(statType == Stat.Agility)
				return (input - 50) / 0.85f;
			if (statType == Stat.Will)
				return -100;
			
			Debug.LogError("잘못된 능력치 종류!: " + statType);
			return 0;
		}
	}

	public static class _Convert{
		public static Direction Vec2ToDir(Vector2 vector){
			if (vector == new Vector2(1, 0))
				return Direction.RightDown;
			else if (vector == new Vector2(-1, 0))
				return Direction.LeftUp;
			else if (vector == new Vector2(0, 1))
				return Direction.RightUp;
			else // vector == new Vector2 (0, -1)
				return Direction.LeftDown;
		}
		
		public static string IntToChar(int input){
			if (input == 1) return "A";
			if (input == 2) return "B";
			if (input == 3) return "C";
			return "X";
		}
	}

	public static class _String{
		public static string GeneralName(string input){//유닛명 앞에 붙은 'PC'나 뒤에 붙은 숫자 또는 _(underbar)를 떼서 범용 codeName으로 변환
			if (input.StartsWith("?") || input.StartsWith("~")) input = input.Substring(1);
			if (input.StartsWith("PC")) input = input.Substring(2);
			if (input.EndsWith("~")) input = input.Substring(0, input.Length - 1);
			if (input.EndsWith("--")) input = input.Substring(0, input.Length - 2);
			if (input.Contains("_")) input = input.Substring(0, input.IndexOf('_'));
			if (input.Contains("0")) input = input.Substring(0, input.IndexOf('0'));
			
			return input;
		}

		public static string Fraction(int upper, int under){
			return upper + " / " + under;
		}
		
		public static string FromDifficulty(Difficulty input){
			if (input == Difficulty.Intro)
				return Language.Find("Difficulty_Easy");
			if (input == Difficulty.Adventurer)
				return Language.Find("Difficulty_Normal");
			if (input == Difficulty.Tactician)
				return Language.Find("Difficulty_Hard");
			return Language.Find("Difficulty_Legend");
		}
		
		public static string NumberToSprite(int input){
			if (input < 10) return "<sprite=" + input + ">";
			if (input < 100) return "<sprite=" + input/10 + "><sprite=" + input%10 + ">";
			return "<sprite=" + input % 1000 / 100 + "><sprite=" + input % 100 / 10 + "><sprite=" + input % 10 + ">";
		}
		
		static List<string[]> ColorTextTable;
		public static string ColorExplainText(string input){//아이콘을 끼워넣고 색 바꾸는 작업을 모두 실행
			input = Regex.Replace(input, @"([0-9]+)\s*턴", "<color=#ff9999>$0</color>"); // ex) "3페이즈", "3 페이즈"
			input = Regex.Replace(input, @"([0-9]+)\s*Turns?", "<color=#ff9999>$0</color>"); // ex) "3페이즈", "3 페이즈"
			input = Regex.Replace(input, @"([\-0-9.]*)×?공격력([\+\-0-9.]*)", "<color=red>$0</color>"); // ex) "0.8공격력", "0.8x공격력+34"
			input = Regex.Replace(input, @"([\-0-9.]*)×?Power([\+\-0-9.]*)", "<color=red>$0</color>"); // ex) "0.8공격력", "0.8x공격력+34"
			input = Regex.Replace(input, @"회피율?\s*([\+0-9.%]*)", "<color=orange>$0</color>"); // ex) "회피 +10%", "회피율"
			input = Regex.Replace(input, @"Evasion?\s*([\+0-9.%]*)", "<color=orange>$0</color>"); // ex) "회피 +10%", "회피율"
			input = Regex.Replace(input, @"반경?\s*([\+0-9.%]*타일)", "<color=#00FFFF>$0</color>"); // ex) "반경 N타일"
			input = Regex.Replace(input, @"range of ([\+0-9.%]* tiles? radius)", "<color=cyan>$0</color>"); // ex) "반경 N타일"
			input = Regex.Replace(input, @"내상\s*([\+0-9.%]*)", "<color=#b2ff66>$0</color>");// ex) "내상 +10%"
			input = Regex.Replace(input, @"Scar\s*([\+0-9.%]*)", "<color=#b2ff66>$0</color>");// ex) "내상 +10%"
			input = input.Replace("^", Environment.NewLine);
	    
			foreach (var convertSet in ColorTextTable ?? Parser.GetMatrixTableFrom("Data/ColorTextTable"))
				input = input.Replace(convertSet[0], convertSet[1]);
			return input;
		}
		
		public static bool Match(string a, string b){
			if (a.StartsWith("~") && a.EndsWith("~"))
				return b.Contains(a.Substring(1, a.Length - 2));
			if (a.EndsWith("~"))
				return b.StartsWith(a.Substring(0, a.Length - 1));
			if (b.StartsWith("~") && b.EndsWith("~"))
				return a.Contains(a.Substring(1, b.Length - 2));
			if (b.EndsWith("~"))
				return a.StartsWith(b.Substring(0, b.Length - 1));
			return a == b;
		}
	}

	public static class _Sprite{
		static Dictionary<string, Sprite> iconDict = new Dictionary<string, Sprite>();
		public static Sprite GetIcon(string address){
			if(!iconDict.ContainsKey(address))
				iconDict.Add(address, Resources.Load<Sprite>("Icon/" + address));
			return iconDict[address];
		}
	}

	public static class UI{
		public static void DestroyAllChilds(Transform transform){
			List<Transform> childs = new List<Transform>();
			foreach (Transform child in transform)	
				childs.Add(child);
			// Cannot remove child when iterating transform.
			foreach (Transform child in childs)
				GameObject.Destroy(child.gameObject);
		}
		
		public static IEnumerator SlideObject(Transform transform, Vector3 finalPos, float duration, bool from = false, bool relative = false) {
			if (relative)
				finalPos += transform.position;
			if (from)
				yield return transform.DOMove(finalPos, duration).From().WaitForCompletion();
			else
				yield return transform.DOMove(finalPos, duration).WaitForCompletion();
		}
		public static IEnumerator SlideRect(RectTransform rect, Vector3 finalPos, float duration, bool from = false, bool relative = false) {
			if (relative)
				finalPos += rect.anchoredPosition3D;
			if (from)
				yield return rect.DOAnchorPos3D(finalPos, duration).From().WaitForCompletion();
			else
				yield return rect.DOAnchorPos3D(finalPos, duration).WaitForCompletion();
		}

		public static IEnumerator MoveWithAcceleration(RectTransform rect, Vector2 movement, float duration){
			Vector2 startPoint = rect.anchoredPosition;
			float movedTime = 0;
			while (movedTime < duration){
				movedTime += Time.deltaTime;
				rect.anchoredPosition = startPoint + movement * (movedTime * movedTime / duration / duration);
				yield return null;
			}

			rect.anchoredPosition = startPoint + movement;
		}
		
		//특정한 값을 넘지 않도록 맞춰줌. 기본 알고리즘대로 할 때 maxWidth보다 작으면 놔두고, 크면 maxWidth만큼으로 깎아준다
		public static void SetHorizontalFit(RectTransform rectTransform, int maxWidth){
			var sizeFitter = rectTransform.GetComponent<ContentSizeFitter>();
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)sizeFitter.transform);
			
			if (rectTransform.rect.width <= maxWidth) return;
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
			rectTransform.sizeDelta = new Vector2(maxWidth, rectTransform.rect.height);
		}

		public static void SetIllustPosition(Image image, string codeName){
			image.sprite = VolatileData.GetSpriteOf(SpriteType.Illust, codeName);
			float upperBorder = image.sprite.border.w * image.rectTransform.sizeDelta.y * image.rectTransform.localScale.y / image.sprite.rect.size.y;
			image.rectTransform.anchoredPosition3D = new Vector3(0, upperBorder, 0);
		}
	}


	public static class _Color{
		public static Color FromUSE(UnitStatusEffect use){
			if(use.IsBuff) return Color.green;
			if(use.IsDebuff) return Color.red;
			return Color.gray;
		}
		
		//현재 CMY 또는 흑백 둘 중 하나로만 적용한다는 전제로 작성됨(18.10.12)  
		public static IEnumerator CycleColor(ColorList listType, float interval, SpriteRenderer renderer = null, Image image = null){
			while (true) {
				foreach(var color in EnumUtil.GetColorList(listType)){
					Tween tw = null;
					if(renderer != null)
						tw = renderer.material.DOColor (color, interval).SetEase (Ease.Linear);
					else if(image != null)
						tw = image.material.DOColor (color, interval).SetEase (Ease.Linear);
					yield return tw.WaitForCompletion ();
				}
			}
		}
	}
	
	public static class Generic{
		public static T PickRandom<T>(List<T> list){
			Debug.Assert(list.Count > 0);
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
	}
}
