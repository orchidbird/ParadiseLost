using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class StringParser{
	public string[] origin = null;
	public int index = 0;

	public StringParser(string line, char separator){
		origin = line.Split(separator);
	}

	public void ResetIndex(){
		index = 0;
	}

	public void CancelLastParse() {
		index --;
	}

	public int Remain {get { return origin.Length - index;}} //index 기준으로 아직 읽지 않은 Array가 앞으로 몇 칸이나 남았는지.

	public string ConsumeString(){
		index += 1;
		try{
			return origin[index - 1];
		}catch{
			Debug.LogError(index + "번째 항목을 읽을 수 없습니다!");
			Debug.LogError("행의 맨 앞 항목 : " + origin[0] + " / 마지막 항목 : " + origin[index-2]);
			return "";
		}
	}

	public bool ConsumeBool(){
		string strValue = ConsumeString();
		return ConvertToBool (strValue);
	}
	bool ConvertToBool(string strValue){
		if(strValue == "O") return true;
		else if(strValue == "X") return false;

		try{
			return bool.Parse(strValue);
		}catch (FormatException e) {
			Debug.LogError(index + "번째 항목 " + strValue + "을 boolean으로 읽을 수 없습니다!");
			Debug.LogError("행의 맨 앞 항목 : " + origin[0] + " / 마지막 항목 : " + origin[index - 2]);
			return false;
		}
	}

	 // true / false / none 처리용. 디폴트로 인식할 스트링과 그 결과값을 받는다.
	public bool ConsumeBool(string defaultValue, bool result){
		string strValue = ConsumeString();
		try{
			if (strValue == defaultValue) return result;
			return bool.Parse(strValue);
		}catch {
            Debug.LogError(index + "번째 항목을 boolean으로 읽을 수 없습니다!");
            Debug.LogError("행의 맨 앞 항목 : " + origin[0] + " / 마지막 항목 : " + origin[index - 2]);
            return false;
		}
	}

	public int ConsumeInt(){
		string strValue = ConsumeString();
		return ConvertToInt (strValue);
	}
	int ConvertToInt(string strValue){
		try{
			return Int32.Parse(strValue);
		}catch {
			Debug.LogError(index + "번째 항목 " + strValue + "을 정수형으로 읽을 수 없습니다!");
			Debug.LogError("행의 맨 앞 항목 : " + origin[0] + " / 마지막 항목 : " + origin[index - 2]);
			return -1;
		}
	}

	public float ConsumeFloat(){
		string strValue = ConsumeString();
		return ConvertToFloat (strValue);
	}
	float ConvertToFloat(string strValue){
		try{
			return Single.Parse(strValue);
		}catch {
			Debug.LogError(index + "번째 항목을 실수형으로 읽을 수 없습니다!");
			Debug.LogError("행의 맨 앞 항목 : " + origin[0] + " / 마지막 항목 : " + origin[index - 2]);
			return -1;
		}
	}

	// 기본값이 있는 float 변수 파싱용. ConsumeString과 동일.
	public float ConsumeFloat(string defaultValue, float f){
		string strValue = ConsumeString();
		try
		{	
			if (strValue == defaultValue) return f;
			return Single.Parse(strValue);
		}catch {
            Debug.LogError(index + "번째 항목을 실수형으로 읽을 수 없습니다!");
            Debug.LogError("행의 맨 앞 항목 : " + origin[0] + " / 마지막 항목 : " + origin[index - 2]);
            return -1;
		}
	}

	public Vector2Int ConsumeVector2(){
		string strValue = ConsumeString();
		return ConvertToVector2 (strValue);
	}
	Vector2Int ConvertToVector2(string strValue){
		string[] parsed = strValue.Split('/');
		return new Vector2Int(int.Parse(parsed[0]), int.Parse(parsed[1]));
	}
    CameraMoveLog ConvertToCameraMoveLog(string str) {
        string[] parsed = str.Split('/');
        if(parsed.Length == 1)
            return new CameraMoveLog(null, Single.Parse(parsed[0]));
        else return new CameraMoveLog(new Vector2(Single.Parse(parsed[0]), Single.Parse(parsed[1])), 
                    Single.Parse(parsed[2]), isTilePos : true);
    }
	ZoomLog ConvertToZoomLog(string str) {
		string[] parsed = str.Split('/');
		if(parsed.Length == 1)
			return new ZoomLog(0, Single.Parse(parsed[0]), false);
		else {
			if (parsed[0].StartsWith("x"))
				return new ZoomLog(Single.Parse(parsed[0].Substring(1)), Single.Parse(parsed[1]), true);
			else return new ZoomLog(Single.Parse(parsed[0]), Single.Parse(parsed[1]), false);
		}
	}

	public T ConvertTo<T>(string strValue, bool throwException = false){
		T value = default(T);
		var ttype = typeof(T);
		if (ttype == typeof(Vector2)) {
			value = (T) (object) ConvertToVector2(strValue);
		} else if (ttype == typeof(int)) {
			value = (T) (object) ConvertToInt(strValue);
		} else if (ttype == typeof(bool)) {
			value = (T) (object) ConvertToBool(strValue);
		} else if (ttype == typeof(string)) {
			value = (T) (object) strValue;
		} else if (ttype == typeof(float)) {
			value = (T) (object) ConvertToFloat(strValue);
		} else if (ttype == typeof(CameraMoveLog)) {
			value = (T) (object) ConvertToCameraMoveLog(strValue);
		} else if (ttype == typeof(ZoomLog)) {
			value = (T) (object) ConvertToZoomLog(strValue);
		} else {
			value = (T) (object) ConvertToEnum<T>(strValue, throwException);
		}
		return value;
	}

	// parsing하기 전에 미리 알맞은 string이 있는지 확인하는 함수.
	public bool PeekList<T>() {
		if(origin.Length <= index)
			return false;
		string strValue = ConsumeString();
		index --;
		if (strValue == "") {
			return false;
		}
		string[] parsed = strValue.Split('|');
		foreach(var token in parsed) {
			try {
				T t = ConvertTo<T>(token, throwException : true);
			} catch {
				return false;
			}
		}
		return true;
	}

	public List<T> ConsumeList<T>(){
		string strValue = ConsumeString();
		string[] parsed = strValue.Split('|');
		List<T> list = new List<T> ();
		foreach (string token in parsed) {
			list.Add (ConvertTo<T> (token));
		}
		return list;
	}

	public T ConsumeEnum<T>(){
		string beforeParsed = ConsumeString();
		return ConvertToEnum<T> (beforeParsed);
	}
	T ConvertToEnum<T>(string beforeParsed, bool throwException = false){
		try{
			if (beforeParsed == "X" || beforeParsed == "O") return (T)Enum.Parse(typeof(T), "None");
			else if (beforeParsed == "-") return (T)Enum.Parse(typeof(T), "Once");
			else return (T)Enum.Parse(typeof(T), beforeParsed);
		}catch (ArgumentException e){
			if(throwException)
				throw new Exception();
			if(beforeParsed == "") {beforeParsed = "(empty)";}
            string log = "잘못된 Enum Parsing " + beforeParsed + " : " + typeof(T).FullName + " / ";
            for(int i = 0; i < Math.Min(3, origin.Length); i++)
                log += origin[i] + ", ";
            log += "... / " + index + "번째";
            Debug.LogError(log);
			return default(T);
		}
	}
}
