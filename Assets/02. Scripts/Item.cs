using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type{ Anmo, Coin, Grenade, Heart, Weapon } //enum : 열거형 타입
    public Type type;
    public int value;

    Rigidbody rigid;
    SphereCollider sphereCollider;

    void Awake(){
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    } 
    void Update() {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision){ //아이템과 충돌을 막기 위함
        if(collision.gameObject.tag == "Floor"){
            rigid.isKinematic = true; 
            sphereCollider.enabled = false;
        }
    }
}
