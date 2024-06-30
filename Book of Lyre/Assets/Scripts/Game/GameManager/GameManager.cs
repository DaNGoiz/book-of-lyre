using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float timeScale = 1f;

    public KeyCode[] vituralKeys;
    public PlayerController player;

    private void Awake()
    {
        player.mInputs = new bool[(int)PlayerController.KeyInput.Count];
        player.mPrevInputs = new bool[(int)PlayerController.KeyInput.Count];

        player.CharacterInit(player.mInputs, player.mPrevInputs);
    }

    public void Update()
    {
        Time.timeScale = timeScale;
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Debug"))
        {
            Utility.Debugging();
        }
    }

    public void FixedUpdate()
    {
        player.UpdatePrevInputs();
        GetInputs();
        player.CharacterUpdate();
    }

    private void GetInputs()
    {
        player.mInputs[(int)PlayerController.KeyInput.GoUp] = Input.GetKey(vituralKeys[(int)PlayerController.KeyInput.GoUp]);
        player.mInputs[(int)PlayerController.KeyInput.GoDown] = Input.GetKey(vituralKeys[(int)PlayerController.KeyInput.GoDown]);
        player.mInputs[(int)PlayerController.KeyInput.GoLeft] = Input.GetKey(vituralKeys[(int)PlayerController.KeyInput.GoLeft]);
        player.mInputs[(int)PlayerController.KeyInput.GoRight] = Input.GetKey(vituralKeys[(int)PlayerController.KeyInput.GoRight]);
        player.mInputs[(int)PlayerController.KeyInput.Jump] = Input.GetKey(vituralKeys[(int)PlayerController.KeyInput.Jump]);
    }
}
