using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.IO;

public class DataCollector : MonoBehaviour
{
    //Variables
    float timeForGuardsToBeAlerted;
    float timeUntilPlayerCaught;
    float timeUntilPlayerEscape;
    float timeUntilFirstCrime;

    bool guardsAlerted = false;
    bool playerCaught = false;
    public bool firstCrimeComitted = false;

    public List<GameObject> civillians;
    public List<GameObject> Guards;

    public bool playerEscaped = false;

    public List<NPC_SHOPKEEP_STATES> shopkeepStates;

    public int numberOfCrimes = 0;
    public int numberOfShopkeeps = 0;

    public int numberOfCivsInfluenced = 0;

    public int numberOfClothesChange = 0;

    public Player_Descriptions player_Description;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    public void AddShopKeepState(NPC_SHOPKEEP_STATES sks)
    {
        shopkeepStates.Add(sks);
    }

    // Update is called once per frame
    void Update()
    {
        if(!guardsAlerted)
        {
            timeForGuardsToBeAlerted += Time.deltaTime;
        }
        if(!playerCaught)
        {
            timeUntilPlayerCaught += Time.deltaTime;
        }
        if(!firstCrimeComitted)
        {
            timeUntilFirstCrime += Time.deltaTime;
        }
        if(!playerEscaped)
        {
            timeUntilPlayerCaught += Time.deltaTime;
        }
        else
        {
            PrintDataToFile();
        }
    }

    public void SetPlayerCaught(bool _bool)
    {
        if(!playerCaught)
            playerCaught = _bool;

        PrintDataToFile();
    }

    public void SetGuardsAlert(bool _bool)
    {
        if(!guardsAlerted)
            guardsAlerted = _bool;
    }

    public void PrintDataToFile()
    {
        using (StreamWriter writer = new StreamWriter(Application.dataPath + Path.AltDirectorySeparatorChar + "Data.json"))
        {
            writer.WriteLine("Time until the first crime was comitted: " + Mathf.Round(timeUntilFirstCrime) + " Seconds.");
            writer.WriteLine("Time until a guard was alerted: " +  Mathf.Round(timeForGuardsToBeAlerted) +" Seconds.");     
            if(playerEscaped)
            {
                writer.WriteLine("Time until player escaped: " + Mathf.Round(timeUntilPlayerEscape) + " Seconds.");
            }
            else
            {
                writer.WriteLine("Time until player was caught: " + Mathf.Round(timeUntilPlayerCaught) + " Seconds.");
            }
           
            writer.WriteLine("");
            writer.WriteLine(numberOfCivsInfluenced + " Civillian npcs were influenced by other Civillian npcs behaviours.");
            writer.WriteLine("");
            writer.WriteLine("Player comitted crime: " + numberOfCrimes + " times.");
            writer.WriteLine("");
            writer.WriteLine("There is: " + civillians.Count + " Civillian npcs.");
            writer.WriteLine("There were: " + GetHowManyWitnesses() + " Civillian npcs who witnessed the player commit a crime.");
            writer.WriteLine("There were: " + GetStateIdle() + " Civillian npcs who were in state Idle");
            writer.WriteLine("There were: " + GetStateInterfere() + " Civillian npcs who were in state Interfere.");
            writer.WriteLine("There were: " + GetStateInvolved() + " Civillian npcs who were in state Involved.");
            writer.WriteLine("There were: " + GetStateLeaving() + " Civillian npcs who were in state Leaving.");
            writer.WriteLine("There were: " + GetStatelurking() + " Civillian npcs who were in state Lurking.");
            writer.WriteLine("There were: " + GetStateSemi_Involved() + " Civillian npcs who were in state Semi_Involved.");
            writer.WriteLine("");
            writer.WriteLine("There were: " + GetSubStateChasingPlayer() + " Civillian npcs who were in sub state Chase_player.");
            writer.WriteLine("There were: " + GetSubStateProtectShopKeep() + " Civillian npcs who were in sub state Protect_shopkeep.");
            writer.WriteLine("There were: " + GetSubStateInformGuards() + " Civillian npcs who were in sub state Inform_Guards.");
            writer.WriteLine("");
            writer.WriteLine("There is: " + numberOfShopkeeps + " Shopkeeper npcs.");
            writer.WriteLine("There were: " + GetShopkeepStateSeekHelp() + " Shopkeeper npcs who entered Seek_Help state at one point.");
            writer.WriteLine("There were: " + GetShopkeepStateShouting() + " Shopkeeper npcs who entered Shouting state at one point.");
            writer.WriteLine("There were: " + GetShopkeepStateChaseOfPlayer() + " Shopkeeper npcs who entered Chase_Of_Player state at one point.");
            writer.WriteLine("There were: " + GetShopkeepStateBackupFromPlayer() + " Shopkeeper npcs who entered Back_Up_From_Player state at one point.");
            writer.WriteLine("");
            writer.WriteLine("The player changed their apperance "+numberOfClothesChange+" times.");
            writer.WriteLine("The players final apperance was: "+player_Description+".");
            writer.WriteLine("");
            writer.WriteLine("There were " + GetPlayerApperanceBlackClothes()+" Civillian npcs who thought Black_Clothes was player apperance.");
            writer.WriteLine("There were " + GetPlayerApperancePurpleClothes() + " Civillian npcs who thought Purple_Clothes was player apperance.");
            writer.WriteLine("There were " + GetPlayerApperanceYellowClothes() + " Civillian npcs who thought Yellow_Clothes was player apperance.");
            writer.WriteLine("There were " + GetPlayerApperanceGreenClothes() + " Civillian npcs who thought Green_Clothes was player apperance.");
            writer.WriteLine("");
            writer.WriteLine("There is: " + Guards.Count + " Guard npcs.");
            writer.WriteLine("There were " + GetPlayerApperanceBlackClothesGuard() + " Guard npcs who thought Black_Clothes was player apperance.");
            writer.WriteLine("There were " + GetPlayerApperancePurpleClothesGuard() + " Guard npcs who thought Purple_Clothes was player apperance.");
            writer.WriteLine("There were " + GetPlayerApperanceYellowClothesGuard() + " Guard npcs who thought Yellow_Clothes was player apperance.");
            writer.WriteLine("There were " + GetPlayerApperanceGreenClothesGuard() + " Guard npcs who thought Green_Clothes was player apperance.");

        }
    }

    int GetHowManyWitnesses()
    {
        int countOfWitnesses = 0;
        for(int i = 0; i < civillians.Count; i++)
        {
            if(civillians[i].GetComponent<Npc_Civillian>().isWitness)
            {
                countOfWitnesses++;
            }
        }

        return countOfWitnesses;
    }

    int GetStateIdle()
    {
        int countOfIdles = 0;
        for (int i = 0; i < civillians.Count; i++)
        {
            if (civillians[i].GetComponent<Npc_Civillian>().currentState == NPC_States.idle)
            {
                countOfIdles++;
            }
        }

        return countOfIdles;
    }
    int GetStateInterfere()
    {
        int countOfState = 0;
        for (int i = 0; i < civillians.Count; i++)
        {
            if (civillians[i].GetComponent<Npc_Civillian>().currentState == NPC_States.Interfere)
            {
                countOfState++;
            }
        }

        return countOfState;
    }
    int GetStateInvolved()
    {
        int countOfState = 0;
        for (int i = 0; i < civillians.Count; i++)
        {
            if (civillians[i].GetComponent<Npc_Civillian>().currentState == NPC_States.Involved)
            {
                countOfState++;
            }
        }

        return countOfState;
    }
    int GetStateSemi_Involved()
    {
        int countOfState = 0;
        for (int i = 0; i < civillians.Count; i++)
        {
            if (civillians[i].GetComponent<Npc_Civillian>().currentState == NPC_States.Semi_Involved)
            {
                countOfState++;
            }
        }

        return countOfState;
    }
    int GetStateLeaving()
    {
        int countOfState = 0;
        for (int i = 0; i < civillians.Count; i++)
        {
            if (civillians[i].GetComponent<Npc_Civillian>().currentState == NPC_States.Leaving)
            {
                countOfState++;
            }
        }

        return countOfState;
    }
    int GetStatelurking()
    {
        int countOfState = 0;
        for (int i = 0; i < civillians.Count; i++)
        {
            if (civillians[i].GetComponent<Npc_Civillian>().currentState == NPC_States.lurking)
            {
                countOfState++;
            }
        }

        return countOfState;
    }

    int GetSubStateChasingPlayer()
    {
        int countOfState = 0;
        for (int i = 0; i < civillians.Count; i++)
        {
            if (civillians[i].GetComponent<Npc_Civillian>().currentSubState == NPC_Civ_SubStates.Chase_Player)
            {
                countOfState++;
            }
        }

        return countOfState;
    }

    int GetSubStateProtectShopKeep()
    {
        int countOfState = 0;
        for (int i = 0; i < civillians.Count; i++)
        {
            if (civillians[i].GetComponent<Npc_Civillian>().currentSubState == NPC_Civ_SubStates.Protect_Shopkeeper)
            {
                countOfState++;
            }
        }

        return countOfState;
    }

    int GetSubStateInformGuards()
    {
        int countOfState = 0;
        for (int i = 0; i < civillians.Count; i++)
        {
            if (civillians[i].GetComponent<Npc_Civillian>().currentSubState == NPC_Civ_SubStates.Inform_Guards)
            {
                countOfState++;
            }
        }

        return countOfState;
    }

    int GetShopkeepStateIdle()
    {
        int countOfState = 0;
        for (int i = 0; i < shopkeepStates.Count; i++)
        {
            if (shopkeepStates[i] == NPC_SHOPKEEP_STATES.Idle)
            {
                countOfState++;
            }
        }

        return countOfState;
    }

    int GetShopkeepStateSeekHelp()
    {
        int countOfState = 0;
        for (int i = 0; i < shopkeepStates.Count; i++)
        {
            if (shopkeepStates[i] == NPC_SHOPKEEP_STATES.SeekHelp)
            {
                countOfState++;
            }
        }

        return countOfState;
    }
    int GetShopkeepStateShouting()
    {
        int countOfState = 0;
        for (int i = 0; i < shopkeepStates.Count; i++)
        {
            if (shopkeepStates[i] == NPC_SHOPKEEP_STATES.Shouting)
            {
                countOfState++;
            }
        }

        return countOfState;
    }
    int GetShopkeepStateChaseOfPlayer()
    {
        int countOfState = 0;
        for (int i = 0; i < shopkeepStates.Count; i++)
        {
            if (shopkeepStates[i] == NPC_SHOPKEEP_STATES.ChaseOfPlayer)
            {
                countOfState++;
            }
        }

        return countOfState;
    }
    int GetShopkeepStateBackupFromPlayer()
    {
        int countOfState = 0;
        for (int i = 0; i < shopkeepStates.Count; i++)
        {
            if (shopkeepStates[i] == NPC_SHOPKEEP_STATES.BackUpFromPlayer)
            {
                countOfState++;
            }
        }

        return countOfState;
    }

    int GetPlayerApperanceBlackClothes()
    {
        int countOfState = 0;
        for (int i = 0; i < civillians.Count; i++)
        {
            if (civillians[i].GetComponent<Npc_Civillian>().player_Description == Player_Descriptions.black_Clothes)
            {
                countOfState++;
            }
        }

        return countOfState;
    }
    int GetPlayerApperancePurpleClothes()
    {
        int countOfState = 0;
        for (int i = 0; i < civillians.Count; i++)
        {
            if (civillians[i].GetComponent<Npc_Civillian>().player_Description == Player_Descriptions.purple_Clothes)
            {
                countOfState++;
            }
        }

        return countOfState;
    }
    int GetPlayerApperanceYellowClothes()
    {
        int countOfState = 0;
        for (int i = 0; i < civillians.Count; i++)
        {
            if (civillians[i].GetComponent<Npc_Civillian>().player_Description == Player_Descriptions.yellow_Clothes)
            {
                countOfState++;
            }
        }

        return countOfState;
    }
    int GetPlayerApperanceGreenClothes()
    {
        int countOfState = 0;
        for (int i = 0; i < civillians.Count; i++)
        {
            if (civillians[i].GetComponent<Npc_Civillian>().player_Description == Player_Descriptions.green_Clothes)
            {
                countOfState++;
            }
        }

        return countOfState;
    }

    int GetPlayerApperanceBlackClothesGuard()
    {
        int countOfState = 0;
        for (int i = 0; i < Guards.Count; i++)
        {
            if (Guards[i].GetComponent<Npc_Guard>().player_Description == Player_Descriptions.black_Clothes)
            {
                countOfState++;
            }
        }

        return countOfState;
    }
    int GetPlayerApperancePurpleClothesGuard()
    {
        int countOfState = 0;
        for (int i = 0; i < Guards.Count; i++)
        {
            if (Guards[i].GetComponent<Npc_Guard>().player_Description == Player_Descriptions.purple_Clothes)
            {
                countOfState++;
            }
        }

        return countOfState;
    }
    int GetPlayerApperanceYellowClothesGuard()
    {
        int countOfState = 0;
        for (int i = 0; i < Guards.Count; i++)
        {
            if (Guards[i].GetComponent<Npc_Guard>().player_Description == Player_Descriptions.yellow_Clothes)
            {
                countOfState++;
            }
        }

        return countOfState;
    }
    int GetPlayerApperanceGreenClothesGuard()
    {
        int countOfState = 0;
        for (int i = 0; i < Guards.Count; i++)
        {
            if (Guards[i].GetComponent<Npc_Guard>().player_Description == Player_Descriptions.green_Clothes)
            {
                countOfState++;
            }
        }

        return countOfState;
    }
}
