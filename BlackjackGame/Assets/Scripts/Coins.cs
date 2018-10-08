using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class Coins : MonoBehaviour
{
    public List<Transform> coinsPositions;
    public List<Transform> blackjackCoinsPositions;

    void Start()
    {
        coinsPositions = new List<Transform>();
        blackjackCoinsPositions = new List<Transform>();
        //find all the coins positions in the table
        FindCoinsPositions();
    }

    private void FindCoinsPositions()
    {
        int i = 0;
        while (i < 3)
        {
            coinsPositions[i] = GameObject.Find("chipFieldPlayer" + (i + 1)).GetComponent<Transform>();
            blackjackCoinsPositions[i] = GameObject.Find("blackjackField" + (i + 1)).GetComponent<Transform>();
        }
    }

    /*
     * It divides the player bet in differents chips --> Olman comente esta vara alv :v 
     */
    public void DealCoins(int positionOfCoins, int betPlayer)
    {
        List<int> coinsOfPlayer = new List<int>();
        while (betPlayer > 0)
        {
            if (betPlayer >= 2000)
            {
                coinsOfPlayer.Add(1000);
                betPlayer -= 1000;
            }
            else if (betPlayer > 500)
            {
                coinsOfPlayer.Add(500);
                betPlayer -= 500;
            }
            else if (betPlayer >= 100)
            {
                coinsOfPlayer.Add(100);
                betPlayer -= 100;
            }
            else
            {
                betPlayer = 0;
            }
        }
        MakeCoins(positionOfCoins, coinsOfPlayer);
    }

    public void MakeCoins(int positionOfCoins, List<int> coinsOfPlayer)
    {
        int i = 0;
        float inc100 = 0.04f;
        float inc500 = 0.04f;
        float inc1000 = 0.04f;

        while (i < coinsOfPlayer.Count)
        {
            if (coinsOfPlayer[i] == 100)
            {
                Vector3 vectTemp_100 = new Vector3(this.coinsPositions[positionOfCoins].position.x, this.coinsPositions[positionOfCoins].position.y + inc100, this.coinsPositions[positionOfCoins].position.z);
                Instantiate(Resources.Load("Prefabs/ficha_100"), vectTemp_100, this.coinsPositions[positionOfCoins].rotation);
                inc100 += 0.04f;
            }
            else if (coinsOfPlayer[i] == 500)
            {
                Vector3 vectTemp_500 = new Vector3(this.coinsPositions[positionOfCoins].position.x - 0.40f, this.coinsPositions[positionOfCoins].position.y + inc500, this.coinsPositions[positionOfCoins].position.z);
                GameObject coin = Instantiate(Resources.Load("Prefabs/ficha_500"), vectTemp_500, this.coinsPositions[positionOfCoins].rotation) as GameObject;
                coin.transform.Rotate(new Vector3(180, 0, 0));
                inc500 += 0.04f;
            }
            else if (coinsOfPlayer[i] == 1000)
            {
                Vector3 vectTemp_1000 = new Vector3(this.coinsPositions[positionOfCoins].position.x + 0.40f, this.coinsPositions[positionOfCoins].position.y + inc1000, this.coinsPositions[positionOfCoins].position.z);
                Instantiate(Resources.Load("Prefabs/ficha_1000"), vectTemp_1000, this.coinsPositions[positionOfCoins].rotation);
                inc1000 += 0.04f;
            }
            i++;
        }
    }
}

