using System;
using System.Collections;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    #region Comment
// 목표: 처음에는 가만히 있지만 플레이어가 다가가면 쫓아오는 좀비 몬스터를 만들고 싶다.
    //       ㄴ 쫓아 오다가 너무 멀어지면 제자리로 돌아간다.
    
    // Idle   : 가만히 있는다.
    //   I  (플레이어가 가까이 오면) (컨디션, 트랜지션)
    // Trace  : 플레이러를 쫒아간다.
    //   I  (플레이어와 너무 멀어지면)
    // Return : 제자리로 돌아가는 상태
    //   I  (제자리에 도착했다면)
    //  Idle
    // 공격
    // 피격
    // 죽음
    
    
    
    // 몬스터 인공지능(AI) : 사람처럼 행동하는 똑똑한 시스템/알고리즘
    // - 규칙 기반 인공지능 : 정해진 규칙에 따라 조건문/반복문등을 이용해서 코딩하는 것
    //                     -> ex) FSM(유한 상태머신), BT(행동 트리)
    // - 학습 기반 인공지능: 머신러닝(딥러닝, 강화학습 .. )
    
    // Finite State Machine(유한 상태 머신)
    // N개의 상태를 가지고 있고, 상태마다 행동이 다르다.
    

    #endregion

    public EMonsterState State = EMonsterState.Idle;

    [SerializeField] private GameObject _player;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private NavMeshAgent _agent;

    public ConsumableStat Health;
    public ValueStat Damage;

    public float DetectDistance = 4f;
    public float AttackDistance = 1.2f;
    
    public float MoveSpeed   = 5f;
    public float AttackSpeed = 2f;
    public float AttackTimer = 0f;


    private Vector3 _jumpStartPosition;
    private Vector3 _jumpEndPosition;


    private void Start()
    {
        _agent.speed            = MoveSpeed;
        _agent.stoppingDistance = AttackDistance;
    }
    
    private void Update()
    {
        if (GameManager.Instance.State != EGameState.Playing)
        {
            return;
        }
        
        
        // 몬스터의 상태에 따라 다른 행동을한다. (다른 메서드를 호출한다.)
        switch (State)
        {
            case EMonsterState.Idle:
                Idle();
                break;
            
            case EMonsterState.Trace:
                Trace();
                break;
            
            case EMonsterState.Jump:
                Jump();
                break;
            
            case EMonsterState.Comeback:
                Comeback();
                break;
            
            case EMonsterState.Attack:
                Attack();
                break;
        }
    }
    
    // 1. 함수는 한 가지 일만 잘해야 한다.
    // 2. 상태별 행동을 함수로 만든다.
    private void Idle()
    {
        // 대기하는 상태
        // Todo. Idle 애니메이션 실행
            
        // 플레이어가 탐지범위 안에 있다면...
        if(Vector3.Distance(transform.position, _player.transform.position) <= DetectDistance)
        {
            State = EMonsterState.Trace;
            Debug.Log("상태 전환: Idle -> Trace");
        }
    }

    private void Trace()
    {
        // 플레이어를 쫓아가는 상태
        // Todo. Run 애니메이션 실행
        
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        _agent.SetDestination(_player.transform.position);
     
        
        // 플레이어와의 거리가 공격범위내라면
        if (distance <= AttackDistance)
        { 
            State = EMonsterState.Attack; 
            Debug.Log("상태 전환: Trace -> Attack");
            return;
        }

        if (_agent.isOnOffMeshLink)
        {
            Debug.Log("링크를 만남");
            OffMeshLinkData linkData = _agent.currentOffMeshLinkData;
            _jumpStartPosition = linkData.startPos;
            _jumpEndPosition   = linkData.endPos;

            if (_jumpEndPosition.y > _jumpStartPosition.y)
            {
                Debug.Log("상태 전환: Trace -> Jump");
                State = EMonsterState.Jump;
                return;
            }
        }
        
    }

    
    private void Jump()
    {
        // 순간이동
        _agent.isStopped = true;
        _agent.ResetPath();
        _agent.CompleteOffMeshLink();

        StartCoroutine(Jump_Coroutine());


        // 1. 점프 거리와 내 이동속를 계산해서 점프 시간을 구한다.
        // 2. 점프 시간동안 포물선으로 이동한다.
        // 3. 이동 후 다시 Trace
    }

    private IEnumerator Jump_Coroutine()
    {
        
        float distance = Vector3.Distance(transform.position, _jumpEndPosition);
        float jumpTime = distance / MoveSpeed;
        float jumpHeight = Mathf.Max(1f, distance * 0.3f);
        
        float elapsedTime = 0f;

        while (elapsedTime < jumpTime)
        {
            float t = elapsedTime / jumpTime;
            
            Vector3 newPosition = Vector3.Lerp(_jumpStartPosition, _jumpEndPosition, t);
            newPosition.y += Math.Sign(t * Mathf.PI) * jumpHeight;
            transform.position = newPosition;

            elapsedTime += Time.deltaTime;
            
            yield return null;
        }

        transform.position = _jumpEndPosition;
        State = EMonsterState.Trace;
    }
    

    private void Comeback()
    {
        // 과제 1. 제자리로 복귀하는 상태
    }

    private void Attack()
    {
        // 플레이어를 공격하는 상태
        
        // 플레이어와의 거리가 멀다면 다시 쫒아오는 상태로 전환
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance > AttackDistance)
        {
            State = EMonsterState.Trace;
            Debug.Log("상태 전환: Attack -> Trace");
            return;
        }

        AttackTimer += Time.deltaTime;
        if (AttackTimer >= AttackSpeed)
        {
            AttackTimer = 0f;
            Debug.Log("플레이어 공격!");
            
            // 과제 2번. 플레이어 공격하기
            Player player = _player.GetComponent<Player>();
            player.TryTakeDamage(Damage.Value);
        }
    }




 
    
    public bool TryTakeDamage(float damage)
    {
        if (State == EMonsterState.Hit || State == EMonsterState.Death)
        {
            return false;
        }
        
        Health.Consume(damage);

        
        _agent.isStopped = true;  // 이동 일시정지
        _agent.ResetPath();       // 경로(=목적지) 삭제 
        
        
        if (Health.Value > 0)
        {
            // 히트상태
            Debug.Log($"상태 전환: {State} -> Hit");
            State = EMonsterState.Hit;

            StartCoroutine(Hit_Coroutine());
        }
        else
        {
            // 데스상태
            Debug.Log($"상태 전환: {State} -> Death");
            State = EMonsterState.Death;
            StartCoroutine(Death_Coroutine());
        }

        return true;
    }
    
    private IEnumerator Hit_Coroutine()
    {
        // Todo. Hit 애니메이션 실행
        
        yield return new WaitForSeconds(0.2f);
        State = EMonsterState.Idle;
    }

    private IEnumerator Death_Coroutine()
    {
        // Todo. Death 애니메이션 실행
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
    
}
