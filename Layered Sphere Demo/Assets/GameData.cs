
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;


[System.Serializable]
public class GameData
{

    // Data container for all data that will need to be serialized by the Unity JSON handler.
    // Will be following the same format as created for Cosmo
    // Should remove so we just pass settings for the demo

    public GameSettings settings;

    public GameData(GameSettings in_settings)
    {
        settings = in_settings;

    }

       
}


