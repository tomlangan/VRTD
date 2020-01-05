using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using VRTD.Gameplay;



public class HUDUI : MonoBehaviour
{
    public string WavePosition = "";
    public string CurrentWave = "";
    public string NextEnemy = "";
    public string Coin = "";

    private Text WavePositionText;
    private Text CurrentWaveText;
    private Text NextEnemyText;
    private Text CoinText;

    // Start is called before the first frame update
    void Start()
    {
        Text[] textFields = gameObject.GetComponentsInChildren<Text>();
        WavePositionText = textFields[0];
        CurrentWaveText = textFields[2];
        NextEnemyText = textFields[4];
        CoinText = textFields[5];
    }

    // Update is called once per frame
    void Update()
    {
        WavePositionText.text = WavePosition;
        CurrentWaveText.text = CurrentWave;
        NextEnemyText.text = NextEnemy;
        CoinText.text = Coin;
    }
}
