using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class MainGamePacketer : MonoBehaviourPun, IPacketer
{
    public void Awake()
    {
        Instance = this;
    }
    public static MainGamePacketer Instance {get;private set;}

    // 핸들러는 타입 단위로 한 번만 등록한다.
    // 등록하는 쪽은 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)] static 메서드에서 Register를 호출할 것.
    // (SubsystemRegistration은 아래 초기화가 쓰므로, 같은 타이밍에 등록하면 순서가 보장되지 않음)
    private static readonly Dictionary<string, Action<PacketData>> s_rpcEvent = new Dictionary<string, Action<PacketData>>();
    public Dictionary<string, Action<PacketData>> RPCEvent
    {
        get => s_rpcEvent;
        set => throw new NotSupportedException("MainGamePacketer.Register를 사용하세요.");
    }

    // 도메인 리로드를 꺼둔 경우에도 플레이마다 깨끗하게 시작
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRegistry()
    {
        s_rpcEvent.Clear();
    }

    public static void Register(string id, Action<PacketData> handler)
    {
        if(s_rpcEvent.TryGetValue(id, out var exist) && exist != handler)
        {
            Debug.LogWarning($"[MainGamePacketer] '{id}' 패킷 핸들러가 덮어써집니다.");
        }
        s_rpcEvent[id] = handler;
    }

    [PunRPC]
    public void LoadPacket(byte[] data)
    {
        PacketData d = new(data);
        string id = d;
        if(!s_rpcEvent.TryGetValue(id, out var handler))
        {
            Debug.LogWarning($"[MainGamePacketer] 등록되지 않은 패킷 '{id}'");
            return;
        }
        handler(d);
    }
}
