using System.IO;
using Steamworks;
using UnityEngine;

namespace Save
{
	public static class SaveDataPath
	{
		public static string GetBasicPath()
		{
			if (!SteamManager.Initialized) return Application.persistentDataPath + "/save.json";
			var path = Application.persistentDataPath + "/" + SteamUser.GetSteamID() + "/save.json";
			CreatePathIfNeeded(path);
			return path;
		}

		public static string GetPathWithIndex(int i)
		{
			if (!SteamManager.Initialized) return Application.persistentDataPath + "/save" + i + ".txt";
			var path = Application.persistentDataPath + "/" + SteamUser.GetSteamID() + "/save" + i + ".txt";
			CreatePathIfNeeded(path);
			return path;
		}

		public static string GetPlayLogPathWithIndex(int i) {
			
			if (!SteamManager.Initialized) return Application.persistentDataPath + "/playlog" + i + ".txt";
			var path = Application.persistentDataPath + "/" + SteamUser.GetSteamID() + "/playlog" + i + ".txt";
			CreatePathIfNeeded(path);
			return path;
		}

		static void CreatePathIfNeeded(string path)
		{
			var directoryPath = Path.GetDirectoryName(path);
			if (!Directory.Exists(directoryPath))
				Directory.CreateDirectory(directoryPath);
		}
	}
}
