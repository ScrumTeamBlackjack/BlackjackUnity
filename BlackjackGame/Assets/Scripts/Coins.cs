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
            coinsPositions.Add(GameObject.Find(string.Concat("chipFieldPlayer", (i + 1))).GetComponent<Transform>());
            blackjackCoinsPositions.Add(GameObject.Find(string.Concat("blackjackField", (i + 1))).GetComponent<Transform>());
            i++; ;
        }
    }

    /*
     * It divides the player bet in differents chips --> Olman comente esta vara alv :v 
     */
    public void DealCoins(int positionOfCoins, int betPlayer, string betType)
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
        MakeCoins(positionOfCoins, coinsOfPlayer, betType);
    }

    public void MakeCoins(int positionOfCoins, List<int> coinsOfPlayer, string betType)
    {
        int i = 0;
        float inc100 = 0.04f;
        float inc500 = 0.04f;
        float inc1000 = 0.04f;
        Transform tmpPosition;
        //When the coins are going to the initial bet position
        if(betType == "initial")
        {
            tmpPosition = this.coinsPositions[positionOfCoins];
        }
        //When tha coins are going to the blackjack bt position
        else
        {
            tmpPosition = this.blackjackCoinsPositions[positionOfCoins];
        }

        while (i < coinsOfPlayer.Count)
        {
            if (coinsOfPlayer[i] == 100)
            {
                Vector3 vectTemp100 = new Vector3(tmpPosition.position.x, tmpPosition.position.y + inc100, tmpPosition.position.z);
                Instantiate(Resources.Load("Prefabs/ficha_100"), vectTemp100, tmpPosition.rotation);
                inc100 += 0.04f;
            }
            else if (coinsOfPlayer[i] == 500)
            {
                Vector3 vectTemp500 = new Vector3(tmpPosition.position.x - 0.40f, tmpPosition.position.y + inc500, tmpPosition.position.z);
                GameObject coin = Instantiate(Resources.Load("Prefabs/ficha_500"), vectTemp500, tmpPosition.rotation) as GameObject;
                coin.transform.Rotate(new Vector3(180, 0, 0));
                inc500 += 0.04f;
            }
            else if (coinsOfPlayer[i] == 1000)
            {
                Vector3 vectTemp1000 = new Vector3(tmpPosition.position.x + 0.40f, tmpPosition.position.y + inc1000, tmpPosition.position.z);
                Instantiate(Resources.Load("Prefabs/ficha_1000"), vectTemp1000, tmpPosition.rotation);
                inc1000 += 0.04f;
            }
            i++;
        }
    }
}

