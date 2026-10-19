using System.Collections.Generic;
using UnityEngine;

public static class CalcUtils
{
    
    public static bool SegmentCircleFast(Vector2 a, Vector2 b, Vector2 c, float r, out float tHit)
    {
        tHit = 0;
        float abx = b.x - a.x;
        float aby = b.y - a.y;

        float acx = c.x - a.x;
        float acy = c.y - a.y;

        tHit = acx * acx + acy * acy;

        float abLenSq = abx * abx + aby * aby; //선분 AB의 길이

        if (abLenSq == 0f) //AB의 길이가 0일땐 점과 원간 충돌 체크
        {
            float dx = acx;
            float dy = acy;
            return dx * dx + dy * dy <= r * r;
        }

        float t = (acx * abx + acy * aby) / abLenSq;

        // Clamp 분기 직접 처리 (Mathf.Clamp보다 빠름)
        if (t < 0f) t = 0f;
        else if (t > 1f) t = 1f;

        float closestX = a.x + abx * t;
        float closestY = a.y + aby * t;

        float dx2 = c.x - closestX;
        float dy2 = c.y - closestY;



        return dx2 * dx2 + dy2 * dy2 <= r * r;
    }
    /// <summary>
    /// 히트스캔 결과. out 거리값은 제곱근 취하기 전.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="target"></param>
    /// <param name="r"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static bool HitScan(Vector2 start, Vector2 end, Vector2 target, float r, out float length)
    {
        return SegmentCircleFast(start, end, target, r, out length);
    }
    /// <summary>
    /// 충돌 판정. 점 a에 대해 c에 있는 반지름 r인 원이 닿는가.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="c"></param>
    /// <param name="r"></param>
    /// <returns></returns>
	public static bool SegmentCircle(Vector2 a, Vector2 c, float r)
    {
        return SegmentCircle(a, a, c, r);
    }
    /// <summary>
    /// 충돌 판정. 선분 ab에 대해 c에 있는 반지름 r인 원이 닿는가.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="r"></param>
    /// <returns></returns>
	public static bool SegmentCircle(Vector2 a, Vector2 b, Vector2 c, float r)
    {
        float t = 0;
        return SegmentCircleFast(a, b, c, r, out t);
    }
    public static Vector2 ScreenClamp(Vector2 position, Vector2 size)
    {
        float halfX = size.x * 0.5f;
        float halfY = size.y * 0.5f;

        position.x = Mathf.Clamp(position.x, -960 + halfX, 960 - halfX);
        position.y = Mathf.Clamp(position.y, -540 + halfY, 540 - halfY);

        return position;
    }
    
    /// <summary>
    /// looAt 방향을 바라봄
    /// </summary>
    /// <param name="t"></param>
    /// <param name="lookAt"></param>
    public static void SetAngle(this IMonoObj t, Vector2 lookAt)
    {
        float angle = Mathf.Atan2(lookAt.y, lookAt.x) * Mathf.Rad2Deg;
        t.Obj.transform.eulerAngles =new Vector3(0,0,angle);
    }
    public static float GetAngle(this IMonoObj a, Vector2 objective)
    {
        var lookAt = objective-a.Position;
        return Mathf.Atan2(lookAt.y, lookAt.x) * Mathf.Rad2Deg;
    }
    /// <summary>
    /// targetPos를 바라봄
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="targetPos"></param>
    public static void LookAt(this IMonoObj obj, Vector2 targetPos)
    {
        var vec = targetPos - obj.Position;
        obj.SetAngle(vec);
    }
    
    public static void MoveForward(this IMonoObj obj, float value)
    {
        Vector2 direction = LookDir(obj);
        obj.Position += direction.normalized * value * Time.deltaTime;
    }
    
    public static Vector2 LookDir(this IMonoObj obj)
    {
        return LookDir(obj.Obj.transform.eulerAngles.z);
    }
    public static Vector2 LookDir(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        return new Vector2(
        Mathf.Cos(radian),
        Mathf.Sin(radian)
    );
    }
}