using System;
using UnityEngine;

// 플레이어의 '스탯'을 관리하는 컴포넌트
public class PlayerStats : MonoBehaviour
{
    // 고민해볼 거리
    // 1. 옵저버 패턴은 어떻게 해야지?
    // 2. ConsumableStat의 Regenerate는 PlayerStats에서만 호출 가능하게 하고 싶다. 다른 속성/기능은 다른 클래스에서 사용할 수 있다.
    
    public ConsumableStat Health;
    public ConsumableStat Stamina;
    public ValueStat Damage;
    public ValueStat MoveSpeed;
    public ValueStat RunSpeed;
    public ValueStat JumpPower;

    // 나를 구독하는 구독자 명단(콜백함수 들)
    public static event Action OnDataChanged;
    
    private void Start()
    {
        Debug.Log("PlayerStats initialized");

        // 스타트 호출 시점에서의 OnDataChanged에 구독자가 있을까?
        // 비어있다.
        // 고민해야될것 Action은 참조형인가? 값형이가?
        // Action은 += 를 쓸때마다 새로운 것이다.
        
        // Health.Initialize(OnDataChanged);
        // Stamina.Initialize(OnDataChanged);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        
        Health.Regenerate(deltaTime);
        Stamina.Regenerate(deltaTime);
    } 
}
