using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempPatternExecutor : MonoBehaviour
{

    [SerializeField] List<APattern> patterns;
    bool playingPattern;
    int i=0;

    void Update()
    {
        if(!playingPattern && i < patterns.Count)
        {
            playingPattern = true;
            patterns[i].Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, 
                BulletEngineManager.Ins.Player.transform, () => {i++; playingPattern = false;});
        }
    }

}
