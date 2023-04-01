using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCreator : MonoBehaviour
{
    [SerializeField]
    private EnemyController enemyPrefab;
    [SerializeField]
    private float stopsOnEdgeProbability = 0.8f;
    [Tooltip("the probability of an enemy moving given the game difficulity")]
    [SerializeField]
    private AnimationCurve movingCurveOverDifficulty;
    [SerializeField]
    private GameMaster gameMaster;
    [SerializeField]
    private AnimationCurve speedChangeCurveOverDifficulty;
    [SerializeField]
    private AnimationCurve stopBeforeWallOverDifficulty;
    [SerializeField]
    private AnimationCurve timeUntilCaughtOverDifficulty;
    [SerializeField]
    private float stopBeforeWallMin = 1.5f;
    [SerializeField]
    private AudioMixerManager audioMixerManager;
    private List<EnemyController> created = new List<EnemyController>();

    public GameObject GetEnemy() {
        var e = Instantiate(enemyPrefab, transform);
        e.SetFireRadiusModifier(0.9f + Stats.Difficulity / 3.5f);
        e.stopsOnEdge = Random.value < stopsOnEdgeProbability;
        e.moving = Random.value < GetTime(movingCurveOverDifficulty, Stats.Difficulity);
        if(Random.value < 0.5f)
            e.FlipAfterStart();

        e.MyVision.CaughtEvent.AddListener(gameMaster.GameOver);
        e.MyVision.SpottedEvent.AddListener(audioMixerManager.Seen);
        e.MyVision.UnsuspectingEvent.AddListener(audioMixerManager.Unseen);

        e.MyVision.timeUntilCaught = GetTime(timeUntilCaughtOverDifficulty, Stats.Difficulity);
        e.Speed *= GetTime(speedChangeCurveOverDifficulty, Stats.Difficulity);
        e.stopBeforeWall = stopBeforeWallMin + (e.stopBeforeWall - stopBeforeWallMin) 
            * GetTime(stopBeforeWallOverDifficulty, Stats.Difficulity);
        created.Add(e);
        return e.gameObject;
    }

    public void StartMovingAll() {
        foreach(var cre in created) {
            cre.CanGoFirst();
        }
    }

    float GetTime(AnimationCurve curve, float time) {
        
        return curve.Evaluate(
            Mathf.Clamp(
                time,
                curve[0].time,
                curve[curve.length - 1].time));
    }

}
