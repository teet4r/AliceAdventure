﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetResolutionTest : MonoBehaviour
{
    void Start()
    {
        Screen.SetResolution(720, 960, false);
    }
}