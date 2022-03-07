using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PController10HZ : MonoBehaviour
{
    private bool[] inputs;

    public float jumpSpeed = 500f;
    public int delay = 200;

    public float moveSpeed = 5f;
    Vector3 inputDirection = Vector3.zero;

    private int tk = 0;
    public int globaltk = 0;

    private void Start(){
        inputs = new bool[5];
    }

    private void Update(){
        if(Input.GetKey(KeyCode.W))
            inputs[0] = true;
        if(Input.GetKey(KeyCode.A))
            inputs[1] = true;
        if(Input.GetKey(KeyCode.S))
            inputs[2] = true;
        if(Input.GetKey(KeyCode.D))
            inputs[3] = true;
        if(Input.GetKey(KeyCode.Space))
            inputs[4] = true;
    }
    private void FixedUpdate(){
        string inps = "";
        if (tk==(delay / 20)){
            inputDirection = Vector3.zero;
            if(inputs[0])
                inputDirection.z += 1;
            if(inputs[1])
                inputDirection.x -= 1;
            if(inputs[2])
                inputDirection.z -= 1;
            if(inputs[3])
                inputDirection.x += 1;
            globaltk += 1;
            inps += " tick: " + globaltk.ToString();
            Debug.Log(inps);
            inps = "";
            tk=0;
        }
        tk++;
        Move(inputDirection, inputs[4]);
    }

    private void Move(Vector3 inputDirection, bool jump){
        inputDirection *= (moveSpeed * Time.fixedDeltaTime);

        if(jump){
            gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpSpeed);
        }

        transform.Translate(inputDirection);
        for(int i=0;i<inputs.Length;i++){
            inputs[i]=false;
        }
    }
}
