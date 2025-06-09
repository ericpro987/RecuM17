using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : ScriptableObject
{
    [SerializeField] public int hp;
    [SerializeField] public int dmg;
    [SerializeField] public int dmg2;
    [SerializeField] public AnimatorController animator;
    [SerializeField] public AnimationClip clipIdle;
    [SerializeField] public AnimationClip clipAttack;
    [SerializeField] public AnimationClip clipMove;
    [SerializeField] public float rangeAttack;
    [SerializeField] public Color color;
}
