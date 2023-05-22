using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Section4
{
    public class Menu : MonoBehaviour
    {
        // Start is called before the first frame update
        public void QuitGame()
        {
            SceneManager.LoadScene("GamePlay");
        }
        public void PlayGame()
        {
            SceneManager.LoadScene("Level_1");
        }
    }
}
