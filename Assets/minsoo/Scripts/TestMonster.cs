using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField] float speed = 2f;
    IReadOnlyList<Vector2> path;
    int index;   // 다음에 향할 점

    public void Init(IReadOnlyList<Vector2> path)
    {
        this.path = path;
        index = 1;   // 0번은 생성 위치라 1번부터
    }

    void Update()
    {
        if (path == null) return;

        float move = speed * Time.deltaTime;
        while (move > 0f && index < path.Count)
        {
            Vector3 target = path[index];
            float dist = Vector3.Distance(transform.position, target);

            if (dist <= move)
            {
                transform.position = target;
                move -= dist;   // 남은 이동량으로 다음 구간까지 이어서 이동
                index++;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, target, move);
                move = 0f;
            }
        }

        if (index >= path.Count) ReachGoal();
    }

    void ReachGoal()
    {
        // 플레이어 체력 감소 등 처리
        Destroy(gameObject);
    }
}