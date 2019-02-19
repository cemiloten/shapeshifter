using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetShape : BlockShape
{
   private void Update()
   {
        if (Input.GetKeyDown(KeyCode.Return))
            Truc();
   }


    private void Truc()
    {
        currentShape = new bool[GameManager.Instance.width * GameManager.Instance.height];
        for (int i = 0; i < GameManager.Instance.width * GameManager.Instance.height; ++i)
        {
            if (i > 3 && i < 10)
                currentShape[i] = true;
        }
        ShowCurrentShape();
    }
}