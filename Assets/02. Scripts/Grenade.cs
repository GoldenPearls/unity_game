using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//����ź ��ũ��Ʈ
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
        meshObj.SetActive(false); //����ź �Ž��� ��Ȱ��ȭ
        effectObj.SetActive(true); //������ Ȱ��ȭ

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, 15, Vector3.up, 0f, LayerMask.GetMask("Enemy")); //����ź ������ ��� �͵��� ���Ľ�Ŵ

        foreach(RaycastHit hitObj in rayHits){ // ����ź ���� ������ �ǰ� �Լ��� ȣ��
            hitObj.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }
        Destroy(gameObject, 5); //��ƼŬ�� ������� �ð����� ���
    }
}
