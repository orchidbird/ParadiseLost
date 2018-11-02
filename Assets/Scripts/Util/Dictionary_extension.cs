using System;
using System.Collections.Generic;
using UnityEngine;

public static class Dictionary_SmartAdd
{
	public static void SmartAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value)
	{
		try
		{
			dic.Add(key,value);
		}
		catch (Exception e)
		{
			Debug.LogError("Dictionary has error with " + key.ToString());
			throw;
		}
	}

	public static TValue SmartGet<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key)
	{
		try
		{
			return dic[key];
		}
		catch (Exception e)
		{
			Debug.LogError("Dictionary has error with key " + key.ToString());
			throw;
		}
	}
}
