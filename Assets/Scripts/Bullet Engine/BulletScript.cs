using Ink.Parsed;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public interface IBulletScript
{

    IEnumerator Execute(Transform transform = null); //Can choose to just not use it

}

public abstract class BulletScript
{

    public bool IsRunning { get; protected set; } = false;
    protected List<IEnumerator> coroutines;

    public BulletScript(List<IEnumerator> coroutines) => this.coroutines = coroutines;

    public abstract void RunNext();

}

public class BulletScriptRandom : BulletScript
{

    public BulletScriptRandom(List<IEnumerator> coroutines) : base(coroutines) => unused = coroutines.Copy();
    List<IEnumerator> unused = new();

    public override void RunNext()
    {
        if(unused.Count == 0) unused = coroutines.Copy();

        int i = UnityEngine.Random.Range(0, unused.Count);
        IsRunning = true;
        BulletCommandGradualAPI.StartCommand(unused[i], () => IsRunning = false);
        unused.RemoveAt(i);
    }

}

public class BulletScriptSequential : BulletScript
{

    public BulletScriptSequential(List<IEnumerator> coroutines) : base(coroutines) { }
    int i = 0;

    public override void RunNext()
    {
        IsRunning = true;
        BulletCommandGradualAPI.StartCommand(coroutines[i], () => IsRunning = false);
        i = (i + 1) % coroutines.Count;
    }

}

public class SpiralDodgingScript
{



}