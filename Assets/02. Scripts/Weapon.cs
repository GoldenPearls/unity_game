using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 총 무기 스크립트
public class Weapon : MonoBehaviour
{
    public enum Type{ Melee, Range };
    public Type type; //무기타입
    public int damage; //데미지
    public float rate; //공속
    public int maxAmmo;
    public int curAmmo;

    public BoxCollider meleeArea; //범위
    public TrailRenderer trailEffect; //효과
    public Transform bulletPos;
    public GameObject bullet; // 프리팹 저장할 함수
    public Transform bulletCasePos;
    public GameObject bulletCase;

    public AudioClip fireSfx; // 총소리에 사용될 오디오 음원(총발사 소리)
    private new AudioSource audio; // AudioSource  컴포넌트를 저장할 변수(총 발사 소리)


    private void Start(){
        audio = GetComponent<AudioSource>();
    }

    public void Use(){
        if(type == Type.Melee){
            StopCoroutine("Swing");
            StartCoroutine("Swing"); //코루틴 함수 불러냄
        }
        else if(type == Type.Range && curAmmo > 0){ //현재 탄약을 조건에 추가하고, 발사했을 때 감소하도록 작성
            curAmmo--;
            StartCoroutine("Shot"); //코루틴 함수 불러냄
        }
    }

    IEnumerator Swing(){ //IEnumerator : 열거형 함수 클래스
    //1
    yield return new WaitForSeconds(0.1f); //0.1초 대기
    meleeArea.enabled = true;
    trailEffect.enabled = true;
    //2
    yield return new WaitForSeconds(0.3f); //1프레임 대기
    meleeArea.enabled = false;
    //3
    yield return new WaitForSeconds(0.3f); //1프레임 대기
    trailEffect.enabled = false;    
    }

    IEnumerator Shot(){
        // # 1. 총알 발사
        audio.PlayOneShot(fireSfx, 1.0f); // 총소리 발생
        GameObject intantBaullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intantBaullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50;
       
        yield return null;
        // #2. 탄피 배출
        GameObject intantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = intantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3) ;
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
    }
}

    // Use() 메인루틴 -> Swing() 서브루틴 -> Use() 메인루틴 (교차실행) => 원래
    // Use() 메인루틴 + Swing() 코루틴 (Co-Op)

