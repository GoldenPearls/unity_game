using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �� ���� ��ũ��Ʈ
public class Weapon : MonoBehaviour
{
    public enum Type{ Melee, Range };
    public Type type; //����Ÿ��
    public int damage; //������
    public float rate; //����
    public int maxAmmo;
    public int curAmmo;

    public BoxCollider meleeArea; //����
    public TrailRenderer trailEffect; //ȿ��
    public Transform bulletPos;
    public GameObject bullet; // ������ ������ �Լ�
    public Transform bulletCasePos;
    public GameObject bulletCase;

    public AudioClip fireSfx; // �ѼҸ��� ���� ����� ����(�ѹ߻� �Ҹ�)
    private new AudioSource audio; // AudioSource  ������Ʈ�� ������ ����(�� �߻� �Ҹ�)


    private void Start(){
        audio = GetComponent<AudioSource>();
    }

    public void Use(){
        if(type == Type.Melee){
            StopCoroutine("Swing");
            StartCoroutine("Swing"); //�ڷ�ƾ �Լ� �ҷ���
        }
        else if(type == Type.Range && curAmmo > 0){ //���� ź���� ���ǿ� �߰��ϰ�, �߻����� �� �����ϵ��� �ۼ�
            curAmmo--;
            StartCoroutine("Shot"); //�ڷ�ƾ �Լ� �ҷ���
        }
    }

    IEnumerator Swing(){ //IEnumerator : ������ �Լ� Ŭ����
    //1
    yield return new WaitForSeconds(0.1f); //0.1�� ���
    meleeArea.enabled = true;
    trailEffect.enabled = true;
    //2
    yield return new WaitForSeconds(0.3f); //1������ ���
    meleeArea.enabled = false;
    //3
    yield return new WaitForSeconds(0.3f); //1������ ���
    trailEffect.enabled = false;    
    }

    IEnumerator Shot(){
        // # 1. �Ѿ� �߻�
        audio.PlayOneShot(fireSfx, 1.0f); // �ѼҸ� �߻�
        GameObject intantBaullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intantBaullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50;
       
        yield return null;
        // #2. ź�� ����
        GameObject intantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = intantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3) ;
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
    }
}

    // Use() ���η�ƾ -> Swing() �����ƾ -> Use() ���η�ƾ (��������) => ����
    // Use() ���η�ƾ + Swing() �ڷ�ƾ (Co-Op)

