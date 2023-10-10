using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void CompetitionStarter()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
