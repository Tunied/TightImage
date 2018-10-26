using UnityEngine;
using UnityEngine.UI;

public class TightImage : Image
{
    private Vector2[] mAllScaledVertices;

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (type == Type.Simple)
        {
            toFill.Clear();
            DoPopulateSimpleImage(toFill);
        }
        else
        {
            base.OnPopulateMesh(toFill);
        }
    }

    private void DoPopulateSimpleImage(VertexHelper toFill)
    {
        var imgRect = GetPixelAdjustedRect();
        var spBounds = overrideSprite.bounds;


        var scaleX = imgRect.width / spBounds.size.x;
        var scaleY = imgRect.height / spBounds.size.y;

        if (preserveAspect)
        {
            var minScale = Mathf.Min(scaleX, scaleY);
            scaleX = scaleY = minScale;
        }

        //Image center offset
        var offsetImgX = rectTransform.rect.center.x;
        var offsetImgY = rectTransform.rect.center.y;

        //Sprite center offset
        var offsetSpX = -scaleX * spBounds.center.x;
        var offsetSpY = -scaleY * spBounds.center.y;

        var imgColor = color;

        mAllScaledVertices = new Vector2[overrideSprite.vertices.Length];

        for (var i = 0; i < overrideSprite.vertices.Length; i++)
        {
            var x = overrideSprite.vertices[i].x * scaleX + offsetImgX + offsetSpX;
            var y = overrideSprite.vertices[i].y * scaleY + offsetImgY + offsetSpY;
            toFill.AddVert(new Vector3(x, y, 0), imgColor, overrideSprite.uv[i]);
            mAllScaledVertices[i] = new Vector2(x, y);
        }

        var triangles = overrideSprite.triangles;
        for (var i = 0; i < triangles.Length; i += 3)
        {
            toFill.AddTriangle(triangles[i], triangles[i + 1], triangles[i + 2]);
        }
    }

    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (type != Type.Simple)
        {
            return base.IsRaycastLocationValid(screenPoint, eventCamera);
        }

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out local);

        var triangles = overrideSprite.triangles;
        for (var i = 0; i < triangles.Length; i += 3)
        {
            if (IsInTriangle(mAllScaledVertices[triangles[i]],
                mAllScaledVertices[triangles[i + 1]],
                mAllScaledVertices[triangles[i + 2]],
                local))
            {
                return true;
            }
        }

        return false;
    }

    // see more info at: http://oldking.wang/20ae1da0-d6d5-11e8-b3e6-29fe0b430026/
    private static bool IsInTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        var v0 = C - A;
        var v1 = B - A;
        var v2 = P - A;

        var dot00 = Vector2.Dot(v0, v0);
        var dot01 = Vector2.Dot(v0, v1);
        var dot02 = Vector2.Dot(v0, v2);
        var dot11 = Vector2.Dot(v1, v1);
        var dot12 = Vector2.Dot(v1, v2);

        var inverDeno = 1 / (dot00 * dot11 - dot01 * dot01);

        var u = (dot11 * dot02 - dot01 * dot12) * inverDeno;
        if (u < 0 || u > 1)
        {
            return false;
        }

        var v = (dot00 * dot12 - dot01 * dot02) * inverDeno;
        if (v < 0 || v > 1)
        {
            return false;
        }

        return u + v <= 1;
    }
}