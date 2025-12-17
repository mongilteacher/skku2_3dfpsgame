using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    [SerializeField] private Monster _monster;

    private void Awake()
    {
        if (_monster == null)
        {
            _monster = GetComponentInParent<Monster>();
        }
    }


    public void PlayerAttack()
    {
        Player player = GameObject.FindAnyObjectByType<Player>();
        player.TryTakeDamage(_monster.Damage.Value);
    }
}