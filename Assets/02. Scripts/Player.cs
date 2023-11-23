using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//플레이어 스크립트
public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public int hasGrenades;
    public GameObject grenadeObj; //프리팹 저장
    public Camera followCamera;
    public GameManger manger;
    public AudioSource jumpSound;

    public int ammo;
    public int coin;
    public int health;
    public int score;

    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

    float hAxis;
    float vAxis;

    bool wDown;
    bool jDown;
    bool fDown;
    bool gDown;
    bool rDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;
    
    bool isJump;
    bool isDodge;
    bool jdodge;
    bool isSwap;
    bool isReload;
    bool isFireReady = true; //준비완료
    bool isBorder; //경계선에
    bool isDamage;
    bool isShop;
    bool isDead;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid;
    Animator anim;
    MeshRenderer[] meshs;

    GameObject nearObject;
    public Weapon equipWeapon;

    int equipWeaponIndex = -1;
    float fireDelay;

    void Awake(){
        rigid = GetComponent<Rigidbody>(); // 물리 효과를 위해 변수 선언 후, 초기화
        anim = GetComponentInChildren<Animator>();
        //Animator 변수를 GetComponentInChildren()으로 초기화
        meshs = GetComponentsInChildren<MeshRenderer>();

        PlayerPrefs.SetInt("MaxScore", 112500);
    }

    // Update is called once per frame
    void Update()
    {
       GetInput();
       Move();
       Turn();
       Jump();
       Grenade();//수류탄
       Attack();
       Reload();
       Dodge();
       Swap();
       Interaction();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        // GetAxisRaw() : Axis 값을 정수로 반환하는 함수
        wDown = Input.GetButton("Walk");
        // shift는 누를 때만 작동되록 GetButton() 함수 사용
        jDown = Input.GetButtonDown("Jump");
        // bool 선언 후 GetButtonDown()으로 점프 입력 받기
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload"); //r키
        jdodge = Input.GetButtonDown("Dodge"); // ctrl키
        iDown = Input.GetButtonDown("Interation"); // e키
        sDown1 = Input.GetButtonDown("Swap1"); //1
        sDown2 = Input.GetButtonDown("Swap2"); //2
        sDown3 = Input.GetButtonDown("Swap3"); //3
    }

    void Move(){
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        //normalized : 방향 값이 1로 보정된 벡터

        if(isDodge)
            moveVec = dodgeVec;

        if(isSwap || isReload || !isFireReady || isDead) // 움직이는 도중에 공격 못함
            moveVec = Vector3.zero;


        if(!isBorder)
            transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;
        
        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn(){
        // #1. 키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);

        // #2. 마우스에 의한 회전
        if(fDown && !isDead){
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition); // ScreenPointToRay() : 스크린에서 월드로 Ray를 쏘는 함수
            RaycastHit rayHit;
            if(Physics.Raycast(ray, out rayHit, 100)){
                Vector3 nextVec = rayHit.point - transform.position; //RayCastHit의 마우스 클릭 위치를 활용하여 회전을 구현
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
        }
        
        }
    }

    void Jump(){
        if(jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap && !isDead){
            rigid.AddForce(Vector3.up * 10, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;

            jumpSound.Play();

        }
    }

    void Grenade(){
        if(hasGrenades == 0){
            return;
        }
        if(gDown && !isReload && !isSwap &&!isDead){
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition); // ScreenPointToRay() : 스크린에서 월드로 Ray를 쏘는 함수
            RaycastHit rayHit;
            if(Physics.Raycast(ray, out rayHit, 100)){
                Vector3 nextVec = rayHit.point - transform.position; //RayCastHit의 마우스 클릭 위치를 활용하여 회전을 구현
                nextVec.y = 5;

                GameObject instantGrade = Instantiate(grenadeObj, transform.position, transform.rotation); //프리팹은 인스턴트화
                Rigidbody rigidGrenade = instantGrade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);  // 생성된 수류탄 리지드바디를 활용해 던지는 로직
        }
        }
    }

    void Attack(){
        if(equipWeapon == null)
            return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(fDown && isFireReady && !isDodge && !isSwap && !isShop){
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0; //공격딜레이를 0으로 돌려서 다음 공격까지 기다리도록 작성
        }
    }

    void Reload(){
        // 제한사항을 걸어줌
        if(equipWeapon == null)
            return;
        if(equipWeapon.type == Weapon.Type.Melee)
            return;
        if(ammo == 0 )
            return;
        if(rDown && !isJump && !isDodge && !isSwap && isFireReady && !isShop){
            anim.SetTrigger("doReload");
            isReload = true;
            Invoke("ReloadOut", 3f);
             
        }
    }

    void ReloadOut(){
       int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        if (ammo < equipWeapon.maxAmmo)
        {
            ammo -= (equipWeapon.maxAmmo - equipWeapon.curAmmo);
            equipWeapon.curAmmo = equipWeapon.maxAmmo;
        }
        else
        {
            ammo -= (reAmmo - equipWeapon.curAmmo);
            equipWeapon.curAmmo = reAmmo;
        }
        isReload = false;
    }

    void Dodge(){
        if(jdodge && !isSwap && !isShop && isDead){ //ctrl키
            dodgeVec = moveVec;
            speed *=2;
            anim.SetTrigger("doDodge");
            isDodge = true;
            
            Invoke("DodgeOut", 0.4f);
            
        }
    }

     void DodgeOut(){
        speed *=0.5f;
        isDodge = false;
    }

    void Swap(){
        
        if(sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if(sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if(sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;


        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;


        if((sDown1 || sDown2 || sDown3) && !isJump && !isDodge){
            if(equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
            
        }
    }

     void SwapOut(){
        isSwap = false;
    }

    void Interaction(){
        if(iDown && nearObject != null && !isJump && !isDodge && !isDead){
            if(nearObject.tag == "Weapon"){
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;
                // 아이템 정보를 가져와서 해당 무기 입수 체크

                Destroy(nearObject);
            }

            else if(nearObject.tag == "Shop"){
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
        }
    }
    }

    void FreezeRoation(){
        rigid.angularVelocity = Vector3.zero; // 스스로 도는 현상 제거
    }

    void StopToWall(){
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, moveVec, 5, LayerMask.GetMask("Wall"));
    }

    void FixedUpdate() {
        FreezeRoation();
        StopToWall();
    }


    void OnCollisionEnter(Collision collision) {
        //착지 구현
        if(collision.gameObject.tag == "Floor"){
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other){ //아이템들 먹기
        if(other.tag == "Item"){
            Item item = other.GetComponent<Item>();
           switch (item.type) {
               case Item.Type.Anmo: 
                   ammo += item.value;
                   if(ammo > maxAmmo)
                    ammo = maxAmmo;
                   break;
                case Item.Type.Coin: 
                   coin += item.value;
                   if(coin > maxCoin)
                    coin = maxCoin;
                   break;
                case Item.Type.Heart: 
                   health += item.value;
                   if(health > maxHealth)
                    health = maxHealth;
                   break;
                case Item.Type.Grenade: //필살기 => 겉으로 보여줄 필요
                   grenades[hasGrenades].SetActive(true);
                   hasGrenades += item.value;
                   if(hasGrenades > maxHasGrenades)
                    hasGrenades = maxHasGrenades;
                   break;      
           }
           Destroy(other.gameObject);
        }
        else if(other.tag == "EnemyBullet"){
            if(!isDamage){
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                bool isBossAtk = other.name == "Boss Melee Area";
                StartCoroutine(OnDamage(isBossAtk));
            }
            if(other.GetComponent<Rigidbody>() !=null)
                    Destroy(other.gameObject);
        }
    }

    IEnumerator OnDamage(bool isBossAtk){
        isDamage = true;
        foreach(MeshRenderer mesh in meshs){
            mesh.material.color = Color.red;
        }

        if(isBossAtk)
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);

        if(health <=0 && !isDead)
            OnDie();
        yield return new WaitForSeconds(1f);

        isDamage = false;
        foreach(MeshRenderer mesh in meshs){
            mesh.material.color = Color.white;
        }
         if(isBossAtk)
            rigid.velocity = Vector3.zero;

        
    }

    void OnDie(){
        anim.SetTrigger("doDie");
        isDead = true;
        manger.GameOver();
    }
    void OnTriggerStay(Collider other) {
        if(other.tag == "Weapon" || other.tag == "Shop")
            nearObject = other.gameObject;


    }
    void OnTriggerExit(Collider other) {
        if(other.tag == "Weapon")
            nearObject = null;
        else if(other.tag == "Shop"){
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            isShop= false;
            nearObject = null;
    }
    }

}
