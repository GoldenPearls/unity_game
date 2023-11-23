using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//수류탄 스크립트
public class Grenade : MonoBehaviour
{
    public GameObject meshObj;
    public GameObject effectObj;
    public Rigidbody rigid;

    void Start()
    {
        StartCoroutine(Explosion());
    }

    IEnumerator Explosion(){
        yield return new WaitForSeconds(3f);
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        meshObj.SetActive(false); //수류탄 매쉬는 비활성화
        effectObj.SetActive(true); //폭발을 활성화

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, 15, Vector3.up, 0f, LayerMask.GetMask("Enemy")); //수류탄 범위에 모든 것들을 폭파시킴

        foreach(RaycastHit hitObj in rayHits){ // 수류탄 범위 적들의 피격 함수를 호출
            hitObj.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }
        Destroy(gameObject, 5); //파티클이 사라지는 시간까지 고려
    }
}
