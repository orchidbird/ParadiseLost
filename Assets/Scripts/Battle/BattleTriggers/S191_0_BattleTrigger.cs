using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;

public class S191_0_BattleTrigger : BattleTrigger {

    public S191_0_BattleTrigger(TrigResultType resultType, StringParser commaParser) : base(resultType, commaParser) {
        TriggerAction = () => {
            TileManager tileManager = TileManager.Instance;
            UnitManager unitManager = UnitManager.Instance;
            Tile targetTile = tileManager.GetTile(((MoveLog)logs.Last()).dest);
            var positions = new List<Vector2Int>();
            var directions = new List<Direction>();

            if(targetTile.location.x == 7) {
                positions.Add(new Vector2Int(5, 23));
                directions.Add(Direction.LeftDown);
                positions.Add(new Vector2Int(9, 23));
                directions.Add(Direction.LeftDown);
            }
            else if(targetTile.location.x == 22) {
                positions.Add(new Vector2Int(20, 25));
                directions.Add(Direction.LeftUp);
                positions.Add(new Vector2Int(22, 23));
                directions.Add(Direction.LeftDown);
                positions.Add(new Vector2Int(17, 24));
                directions.Add(Direction.LeftUp);
                positions.Add(new Vector2Int(19, 23));
                directions.Add(Direction.LeftUp);
                 positions.Add(new Vector2Int(20, 22));
                directions.Add(Direction.LeftDown);
                positions.Add(new Vector2Int(21, 20));
                directions.Add(Direction.LeftDown);
            }
            else if(targetTile.location.x == 18) {
                positions.Add(new Vector2Int(16, 7));
                directions.Add(Direction.RightUp);
                positions.Add(new Vector2Int(20, 7));
                directions.Add(Direction.RightUp);
                positions.Add(new Vector2Int(16, 5));
                directions.Add(Direction.RightUp);
                positions.Add(new Vector2Int(20, 5));
                directions.Add(Direction.RightUp);
                positions.Add(new Vector2Int(18, 3));
                directions.Add(Direction.RightUp);
            }
            else if(targetTile.location.x == 12) {
                positions.Add(new Vector2Int(14, 8));
                directions.Add(Direction.LeftUp);
                positions.Add(new Vector2Int(14, 9));
                directions.Add(Direction.LeftUp);
                positions.Add(new Vector2Int(15, 10));
                directions.Add(Direction.LeftUp);
                positions.Add(new Vector2Int(15, 11));
                directions.Add(Direction.LeftUp);
            }

            unitManager.GenerateUnitsAtPosition("snowHippo", positions, directions);
            targetTile.APAtStandardHeight *= 999;
            targetTile.gameObject.SetActive(false);
        };
    }
}
