using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Section4
{
    [CreateAssetMenu(fileName ="LevelsData", menuName ="Section4/create Levels Data")]

    public class LevelsData : ScriptableObject
    {
        [SerializeField] private string[] levels;

        public string GetNextLevel()
        {
            Scene curScene = SceneManager.GetActiveScene();
            for(int i = 0; i < levels.Length - 1; i++)
            {
                if (levels[i] == curScene.name)
                {
                    return levels[i + 1];
                }
            }
            return "";
        }

    }
}
