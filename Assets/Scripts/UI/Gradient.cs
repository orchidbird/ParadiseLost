using UnityEngine;
using UnityEngine.UI;


// 아래 주소에서 복사했습니다. 옛날 버전용 코드라서 새 버전에 맞게 수정했습니다.
// http://forum.unity3d.com/threads/how-to-text-gradient-fill.268509/
[AddComponentMenu("UI/Effects/Gradient")]
public class Gradient : BaseMeshEffect
{
    [SerializeField]
    private Color32 topColor = Color.white;
    [SerializeField]
    private Color32 bottomColor = Color.black;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0) {
            return;
        }

        int count = vh.currentVertCount;
        UIVertex currentUIVertex = new UIVertex();
        vh.PopulateUIVertex(ref currentUIVertex, 0);
        float bottomY = currentUIVertex.position.y;
        float topY = currentUIVertex.position.y;

        for (int i = 1; i < count; i++) {
            vh.PopulateUIVertex(ref currentUIVertex, i);
            float y = currentUIVertex.position.y;
            if (y > topY) {
                topY = y;
            }
            else if (y < bottomY) {
                bottomY = y;
            }
        }

        float uiElementHeight = topY - bottomY;

        for (int i = 0; i < count; i++) {
            vh.PopulateUIVertex(ref currentUIVertex, i);
            currentUIVertex.color = Color32.Lerp(bottomColor, topColor, (currentUIVertex.position.y - bottomY) / uiElementHeight);
            vh.SetUIVertex(currentUIVertex, i);
        }
    }
}
