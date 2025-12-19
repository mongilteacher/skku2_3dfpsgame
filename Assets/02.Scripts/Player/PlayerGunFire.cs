using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerGunFire : MonoBehaviour
{
    // 목표: 마우스의 왼쪽 버튼을 누르면 바라보는 방향으로 총을 발사하고 싶다. (총알을 날리고 싶다.)
    [SerializeField] private Transform _fireTransform;
    [SerializeField] private ParticleSystem _hitEffect;
    [SerializeField] private List<GameObject> _muzzleEffects;
    [SerializeField] private Animator _animator;

    private EZoomMode _zoomMode = EZoomMode.Normal;
    [SerializeField] private GameObject _normalCrosshair;
    [SerializeField] private GameObject _zoomInCrosshair;
    
    
    private const float ATTACK_SPEED = 0.2f;
    private float _attackTimer = 0.2f;
    
    private void Update()
    {
        ZoomModeCheck();
        
        _attackTimer -= Time.deltaTime;
        
        // 1. 마우슨 왼쪽 버튼이 눌린다면..
        if (Input.GetMouseButton(0) && _attackTimer <= 0f)
        {
            _attackTimer = ATTACK_SPEED;
            
            _animator.SetBool("Fire", true);
            
            Shoot();
            
            StartCoroutine(MuzzleEffect_Coroutine());
        }
        else
        {
            _animator.SetBool("Fire", false);
        }

    }

    private void ZoomModeCheck()
    {
        if (Input.GetMouseButton(1))
        {
            _zoomMode = EZoomMode.ZoomIn;
            _normalCrosshair.SetActive(false);
            _zoomInCrosshair.SetActive(true);
            Camera.main.fieldOfView = 10f;
        }
        else
        {
            _zoomMode = EZoomMode.Normal;
            _normalCrosshair.SetActive(true);
            _zoomInCrosshair.SetActive(false);
            Camera.main.fieldOfView = 60f;
        }
    }

    private IEnumerator MuzzleEffect_Coroutine()
    {
        GameObject muzzleEffect = _muzzleEffects[Random.Range(0, _muzzleEffects.Count)];
        
        muzzleEffect.SetActive(true);
        
        yield return new WaitForSeconds(0.06f);
        
        muzzleEffect.SetActive(false);
    }
    

    private void Shoot()
    {
        // 2. Ray를 생성하고 발사할 위치, 방향, 거리를 설정한다. (쏜다.)
        Ray ray = new Ray(_fireTransform.position, Camera.main.transform.forward);
            
        // 3. RayCastHit(충돌한 대상의 정보)를 저장할 변수를 생성한다.
        RaycastHit hitInfo = new RaycastHit();
            
        // 4. 발사하고,
        bool isHit = Physics.Raycast(ray, out hitInfo);
        if (isHit)
        {
            // 5.  충돌했다면... 피격 이펙트 표시
            Debug.Log(hitInfo.transform.name);
                
            // 파티클 생성과 플레이 방식
            // 1. Instantiate 방식 (+ 풀링) -> 한 화면에 여러가지 수정 후 여러개 그릴경우
            // 2. 하나를 캐싱해두고 Play     -> 인스펙터 설정 그대로 그릴 경우
            // 3. 하나를 캐싱해두고 Emit     -> 인스펙터 설정을 수정 후 그릴 경우
                
            _hitEffect.transform.position = hitInfo.point;
            _hitEffect.transform.forward = hitInfo.normal;

            _hitEffect.Play();

            // 1. 플레이어: 소정씨 개발
            Damage damage = new Damage()
            {
                Value = hitInfo.distance,
                HitPoint = hitInfo.point,
                Normal =  hitInfo.normal,
                Who = gameObject,
                Critical = false,
            };
            
            // 단점
            // - [0] 같은 기능인데 중복된 코드가 많다. (다형성)
            // - [O] 같은 기능인데 기능의 이름과 매개변수가 다르다.
            
            // 인터페이스를 사용하면 된다. -> 약속을 강제한다.
            // 인터페이스를 설계 한다는 것은 중급 개발의 시작
            
            // 플레이어 - 약속 - IDamageable - (몬스터, 드럼통, 나무, 장애물)
            
            IDamageable damageable = hitInfo.collider.GetComponent<IDamageable>();
            if (damageable != null) // 몬스터 or 드럼통 or 새 or 나무 판자 ..
            {
                damageable.TryTakeDamage(damage);
            }
        }
    }

}
