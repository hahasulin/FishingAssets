
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Player : MonoBehaviour
{

    public enum State
    {
        None,
        Dead,
        Action,
        Idle,
        Escape,
        Move,
        Patrol,
        Damage,
        End,
    }
    public State state = State.None;

    public float moveSpeed = 0.01f;

    private void Start()
    {
        SetControll();
    }

    //================================================================================================================================================
    // ��Ʈ�� ����
    //================================================================================================================================================

    void SetMouse()
    {
        //Singleton_Controller.INSTANCE.key_MouseLeft += InputMousetLeft;
        //Singleton_Controller.INSTANCE.key_MouseRight += InputMouseRight;
        //Singleton_Controller.INSTANCE.key_MouseWheel += InputMouseWheel;
    }

    void RemoveMouse()
    {
        //Singleton_Controller.INSTANCE.key_MouseLeft -= InputMousetLeft;
        //Singleton_Controller.INSTANCE.key_MouseRight += InputMouseRight;
        //Singleton_Controller.INSTANCE.key_MouseWheel += InputMouseWheel;
    }

    //================================================================================================================================================
    // ��Ʈ��
    //================================================================================================================================================


    public void StateMachine(State _state)
    {
        state = _state;

        if (stateAction != null)
            StopCoroutine(stateAction);

        switch (state)
        {
            case State.None:
                break;
            case State.Dead:
                //RemoveKeyCode();
                //DeadState();
                //OutOfControll(true);
                break;
            case State.Action:
                break;
            case State.Idle:
                if (dirction.x != 0f || dirction.y != 0f)
                    StateMachine(State.Move);
                break;
            case State.Move:
                stateAction = StartCoroutine(Moving());
                break;
            case State.Escape:
                //stateAction = StartCoroutine(MoveEscape());
                break;
            case State.Damage:
                break;
        }
    }

    //================================================================================================================================================
    // �̵�
    //================================================================================================================================================

    public Vector2 dirction;
    public void StateMove(Vector2 _dirction)
    {
        dirction = _dirction;
        //SetDirection();

        //if (outOfControll == true)
        //    return;

        if (state == State.Idle)
        {
            StateMachine(State.Move);
        }
        if (state == State.Move)// �����̳� ȸ�ǰ� ���� �� ������
        {
            if (dirction.x == 0f && dirction.y == 0f)
                StateMachine(State.Idle);
        }
    }

    IEnumerator Moving()
    {
        while (state == State.Move)
        {
            SetMoving();
            yield return null;
            CheckClosestUnit();
        }
    }

    void CheckClosestUnit()// �������̳� ä�� ������ �ϱ� ���� üũ
    {
        if (triggerGameObject.Count == 0)
            return;

        closestDistance = float.MaxValue;
        Trigger_Setting tempTarget = null;
        for (int i = 0; i < triggerGameObject.Count; i++)
        {
            float offsetDist = (triggerGameObject[i].transform.position - transform.position).sqrMagnitude;
            if (closestDistance > offsetDist)
            {
                closestDistance = offsetDist;
                tempTarget = triggerGameObject[i];
            }
        }

        if (closestTarget != tempTarget)
        {
            closestTarget = tempTarget;
        }
        Game_Manager.current.followManager.AddClosestTarget(closestTarget);
    }
    public Reflection_Manager reflection_Manager;
    public float shipHight, waveSpeed = 2f;
    float runningTime;
    public GameObject playerObject;

    Quaternion prevAngle, setAngle;
    public float targetAngle = 10f;
    float randomTime, runningRandomTime;

    public AnimationCurve rotateCurve;

    private void Update()
    {
        SetOceanRenderer();
    }

    void SetOceanRenderer()
    {
        runningTime += Time.deltaTime * waveSpeed;

        float moveHight = (Mathf.Sin(runningTime) + 1f) * 0.5f;// ���Ʒ� ������
        Vector3 localPosition = Vector3.up * moveHight * shipHight;
        playerObject.transform.localPosition = localPosition;

        if (runningTime > runningRandomTime)
        {
            randomTime = UnityEngine.Random.Range(5f, 3f);
            runningRandomTime = runningTime + randomTime;
            prevAngle = playerObject.transform.localRotation;
            setAngle = Quaternion.Euler(RandomAngle(targetAngle));
        }

        float curve = rotateCurve.Evaluate(1f - (runningRandomTime - runningTime) / randomTime);
        playerObject.transform.localRotation = Quaternion.Slerp(prevAngle, setAngle, curve / randomTime);// ���� ȸ��

        string shipPosition = "_ShipPosition";
        reflection_Manager.GetMaterial.SetVector(shipPosition, playerObject.transform.position);
        reflection_Manager.GetMaterial.SetFloat("_WaveSpeed", waveSpeed);
    }

    Vector3 RandomAngle(float _maxAngle)
    {
        float x = UnityEngine.Random.Range(-_maxAngle, _maxAngle);
        float y = UnityEngine.Random.Range(-_maxAngle, _maxAngle);
        float z = UnityEngine.Random.Range(-_maxAngle, _maxAngle);
        return new Vector3(x, y, z);
    }

    void SetMoving()
    {
        float speed = moveSpeed * Time.deltaTime;
        Game_Manager.current.cameraManager.transform.position = transform.position;
        Vector3 dir = new Vector3(dirction.x, 0f, dirction.y);
        Vector3 target = transform.position + Game_Manager.current.cameraManager.transform.TransformDirection(dir).normalized;
        transform.position = Vector3.Lerp(transform.position, target, speed);

        Vector3 offset = (target - transform.position).normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(offset), speed * 5f);
    }

    void RotateMousePosition()
    {
        float speed = moveSpeed * Time.deltaTime;
        Vector3 playerPosition = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 mousePosition = Input.mousePosition;

        Vector3 uiOffset = (mousePosition - playerPosition).normalized;
        Vector3 dir = new Vector3(uiOffset.x, 0f, uiOffset.y);

        Vector3 target = transform.position + Game_Manager.current.cameraManager.transform.TransformDirection(dir).normalized;
        Vector3 offset = (target - transform.position).normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(offset), speed * 5f);
    }

    //================================================================================================================================================
    // �浹
    //================================================================================================================================================

    public List<Trigger_Setting> triggerGameObject = new List<Trigger_Setting>();
    public Trigger_Setting closestTarget;
    public float closestDistance;

    private void OnTriggerEnter(Collider other)
    {
        Trigger_Setting fishing = other.GetComponent<Trigger_Setting>();
        if (fishing == null)
            return;
        triggerGameObject.Add(fishing);
    }

    private void OnTriggerExit(Collider other)
    {
        Trigger_Setting fishing = other.GetComponent<Trigger_Setting>();
        if (fishing == null)
            return;

        triggerGameObject.Remove(fishing);
        if (triggerGameObject.Count == 0)
        {
            closestTarget = null;
            Game_Manager.current.followManager.AddClosestTarget(null);
        }
    }

    //================================================================================================================================================
    // ȸ��
    //================================================================================================================================================

    public void StateEscape()
    {
        StateMachine(State.Escape);
    }

    //IEnumerator MoveEscape()// Ż�� (ȸ��)
    //{
    //    float normalize = 0f;
    //    float actionTime = unitAnimation.PlayAnimation(4);// �ִϸ��̼� ���̸�ŭ ���
    //    OutOfControll(actionTime + 0.5f);// ��� �ð� 0.5f
    //    Vector3 dir = new Vector3(dirction.x, 0f, dirction.y);
    //    while (normalize < actionTime)
    //    {
    //        normalize += Time.deltaTime;
    //        float escapeSpeed = Mathf.Lerp(0.3f, 0f, normalize * 5f);
    //        SetMoveEscape(dir, escapeSpeed);
    //        yield return null;
    //    }
    //    StateMachine(State.Idle);
    //}

    void SetMoveEscape(Vector3 _dir, float _escapeSpeed)
    {
        Game_Manager.current.cameraManager.transform.position = transform.position;
        Vector3 target = transform.position + Game_Manager.current.cameraManager.transform.TransformDirection(_dir).normalized;
        transform.position = Vector3.Lerp(transform.position, target, _escapeSpeed);

        Vector3 offset = (target - transform.position).normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(offset), _escapeSpeed * 5f);
    }

    //================================================================================================================================================
    // ����
    //================================================================================================================================================

    Coroutine stateAction;
    bool action = false;

    public void EventAction()// ���� �̺�Ʈ
    {

    }

    public void State_Action(bool _input)// Ŭ�� �̺�Ʈ
    {
        if (_input == true)
        {
            //if (outOfControll == true)
            //    return;

            StateMachine(State.Action);
            stateAction = StartCoroutine(State_Acting());
        }
        else
        {
            if (closestTarget != null)
            {
                triggerGameObject.Remove(closestTarget);
                switch (closestTarget.triggerType)
                {
                    case Trigger_Setting.TriggerType.Fishing:
                        // ���� ����
                        Game_Manager.current.fishingManager.StartGame(closestTarget);
                        Destroy(closestTarget.gameObject);
                        OutOfControll(true);
                        break;

                    case Trigger_Setting.TriggerType.Landing:
                        Game_Manager.current.OutOfControll(true);
                        closestTarget.GetTriggerLanding.SetLanding(this);
                        break;
                }
                closestTarget = null;
                Game_Manager.current.followManager.AddClosestTarget(null);// �ȷο� ������ ����
            }
            stateAction = StartCoroutine(State_StopActing());
        }
    }

    IEnumerator State_Acting()
    {
        action = true;
        while (action == true)
        {
            float coolingTime = 1f;
            //float castingTime = currentSkill.skillStruct.castingTime;
            //if (castingTime > 0f)
            //    yield return StartCoroutine(SkillCasting(castingTime));// ĳ����

            //float coolingTime = currentSkill.skillStruct.coolingTime;
            //currentSkill.startTime = Time.time + coolingTime;
            //Debug.LogWarning("State_Attacking!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            //float actionTime = unitAnimation.PlayAnimation(3);// �ִϸ��̼�
            //OutOfControll(actionTime);// �����ϴ� ���� ���
            yield return new WaitForSeconds(coolingTime);

            //float coolingTime = currentSkill.skillStruct.coolingTime;
            //yield return new WaitForSeconds(coolingTime);
        }
    }

    IEnumerator State_StopActing()
    {
        action = false;
        while (state == State.Action)
        {
            //if (outOfControll == false)
                StateMachine(State.Idle);
            yield return null;
        }
    }
    //================================================================================================================================================
    // Ȧ��
    //================================================================================================================================================

    //bool outOfControll = false;

    public void OutOfControll(bool _isOn)
    {
        //outOfControll = _isOn;
    }

    //void OutOfControllTimer(float _time)
    //{
    //    StartCoroutine(HoldControllTimer(_time));
    //}

    //IEnumerator HoldControllTimer(float _time)
    //{
    //    outOfControll = true;
    //    yield return new WaitForSeconds(_time);
    //    outOfControll = false;
    //}
















    //================================================================================================================================================
    // ����
    //================================================================================================================================================

    public void SetControll()
    {
        state = State.Idle;
        //dirction = Vector2Int.zero;

        //SetKeyCode();
    }

    public void RemoveControll()
    {
        state = State.Idle;
        //dirction = Vector2Int.zero;

        //RemoveKeyCode();
    }
}