using UnityEngine;
using Enums;
using System;
using GameData;

namespace Camerawork {
	public class CameraWork {
		public CameraWorkType type;
		public float size;
		public bool relative;
		public float duration;
		public Vector2 pos;
		public string scriptName;

		public CameraWork(string data) {
			StringParser parser = new StringParser(data, '\t');
			this.type = (CameraWorkType) Enum.Parse(typeof(CameraWorkType), parser.ConsumeString());
			if (type == CameraWorkType.Zoom) {
				StringParser parser2 = new StringParser(parser.ConsumeString(), '/');
				string sizeStr = parser2.ConsumeString();
				if (sizeStr.StartsWith("x")) {
					this.relative = true;
					this.size = float.Parse(sizeStr.Substring(1));
				} else this.size = float.Parse(sizeStr);

				this.duration = parser2.ConsumeFloat();
			}
			else if(type == CameraWorkType.MoveCamera) {
				StringParser parser2 = new StringParser(parser.ConsumeString(), '/');
				this.pos = new Vector2(parser2.ConsumeInt(), parser2.ConsumeInt());
				this.duration = parser2.ConsumeFloat();
			}
			else if(type == CameraWorkType.Script) {
				this.scriptName = "stage" + (int)VolatileData.progress.stageNumber + "_" + parser.ConsumeString();
			}
			else if(type == CameraWorkType.ZoomDefault) {
				this.duration = parser.ConsumeFloat();
			}
		}
	}
}
