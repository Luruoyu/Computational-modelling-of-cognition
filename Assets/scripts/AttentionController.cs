using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttentionController : MonoBehaviour
{
    private float currentTotalTime = 0f;
    private float timer;
    private float remainTime = 0f;
    private bool pause = false;

    public string memoryTag;
    public int refreshingIndex = 0;
    private GameObject[] memoryItems;
    private int setSize;
    private GameObject processingItem;

    private int alreadyConsolidatedItemNum = 0;
    private int currentIndex;
    private int shiftOrNot;

    private List<float> timeList = new List<float>();
    private List<taskType> taskList = new List<taskType>();
    private int taskIndex;
    private State attentionState;
    private float currentRefreshTime;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0f;
        float startPos = -2.5f;
        float interItemPos = 0.5f;
        memoryItems = GameObject.FindGameObjectsWithTag(memoryTag);
        // get the memory item order
        int indexTemp = 0;
        foreach (GameObject itemTemp in memoryItems)
        {
            Vector3 newPos = new Vector3(startPos + interItemPos*indexTemp, itemTemp.transform.position.y, 
                itemTemp.transform.position.z);
            itemTemp.transform.position = newPos;
            indexTemp += 1;
            itemTemp.GetComponent<memoryItemController>().DetermineItemIndex(indexTemp);
        }
        processingItem = GameObject.FindGameObjectWithTag("processing");
        (timeList, taskList, setSize) = GlobalManager.ComplexSpan1();
    }

    // Update is called once per frame
    void Update()
    {
        if (GlobalManager.timerStart)
        {
            GetInternalState();
        }
    }

    private void GetInternalState()
    {
        attentionState = GlobalManager.internalState;
    }

    /*
    public void RunTrial()
    {
        taskIndex = 0;
        currentTotalTime = 0f;
        currentIndex = 0;
        alreadyConsolidatedItemNum = 0;
        List<bool> recallCorrect = new List<bool>();
        while (taskIndex < taskList.Count)
        {
            if(taskList[taskIndex] == "memory")
            {
                // encode and consolidate memory item
                GlobalManager.GetEncodingTime(timeList[taskIndex]);
                ChangeFoA(alreadyConsolidatedItemNum, GlobalManager.EncodingTime);                
                memoryItems[currentIndex].GetComponent<memoryItemController>().Encoding(GlobalManager.EncodingTime);
                alreadyConsolidatedItemNum += 1;

                // if there is remained time, refresh memory items
                AttentionalRefreshing(alreadyConsolidatedItemNum, timeList[taskIndex] - GlobalManager.EncodingTime, 0);
                currentTotalTime += timeList[taskIndex] - GlobalManager.EncodingTime;
            }
            if (taskList[taskIndex] == "processing")
            {
                // processing task
                float processingTime = timeList[taskIndex] - GlobalManager.FreeTime;
                foreach (GameObject curItem in memoryItems)
                {
                    // the items with 0 activation can also × decay rate
                    curItem.GetComponent<memoryItemController>().Decay(processingTime);
                }
                ChangeFoA(setSize, processingTime);  // FoA is on the processing task

                // free time (attentional refreshing)
                AttentionalRefreshing(alreadyConsolidatedItemNum, GlobalManager.FreeTime, 0);
            }
            if(taskList[taskIndex] == "recall")
            {
                foreach (GameObject curItem in memoryItems)
                {
                    bool curCorrect = curItem.GetComponent<memoryItemController>().Retrieval();
                    recallCorrect.Add(curCorrect);
                }
            }
            taskIndex += 1;
        }
        Debug.Log(recallCorrect);
    }
    

    /// <summary>
    /// cumulative refreshing
    /// </summary>
    public void AttentionalRefreshing(int setSize, float totalDuration, int startPos)
    {
        bool refreshOver = false;
        
        float refreshRemainTime = totalDuration;  // 注意刷新的剩余总时间
        int currentPosition = startPos;

        if (refreshRemainTime > 0)
        {
            refreshRemainTime -= Time.deltaTime;
            // the being-refreshed item
            remainTime = GlobalManager.RefreshDuration;
            ChangeFoA(currentPosition, GlobalManager.RefreshDuration);
            memoryItems[currentPosition].GetComponent<memoryItemController>().currentProcessDuration
                = GlobalManager.RefreshDuration;
            currentPosition += 1;
        }
        while (refreshOver == false)
        {
            // the being-refreshed item
            ChangeFoA(currentPosition, GlobalManager.RefreshDuration);
            memoryItems[currentPosition].GetComponent<memoryItemController>().Encoding(GlobalManager.RefreshDuration);
            currentRefreshTime += GlobalManager.RefreshDuration;
            currentPosition += 1;

            // other items
            foreach (GameObject curItem in memoryItems)
            {
                // the items with 0 activation can also × decay rate
                curItem.GetComponent<memoryItemController>().Decay(GlobalManager.RefreshDuration);
            }

            // judge if refresh is over
            if (currentPosition >= setSize)
            {
                currentPosition = 0;
            }

            if (currentRefreshTime >= totalDuration)
            {
                refreshOver = true;
            }
        }
    }
    */


    public void ChangeFoA(int positionIndex, float timeStayedInThisPos)
    {
        currentIndex = positionIndex;
        foreach (GameObject curItem in memoryItems)
        {
            // the items with 0 activation can also × decay rate
            curItem.GetComponent<memoryItemController>().inFoA = 0;
        }
        if (currentIndex < setSize)
        {
            memoryItems[currentIndex].GetComponent<memoryItemController>().inFoA = 1;
        }        
        currentTotalTime += timeStayedInThisPos;

        UpdatePosition(positionIndex);
    }


    /// <summary>
    /// change the position of the ball in the scene
    /// </summary>
    /// <param name="itemIndex"></param>
    private void UpdatePosition(int itemIndex)
    {
        Vector3 newPosition = Vector3.zero;
        if (itemIndex >= setSize)
        {
            newPosition = processingItem.transform.position;
        }
        else
        {
            newPosition = memoryItems[itemIndex].transform.position;
        }
        newPosition.y = 2.5f;
        transform.position = newPosition;
    }
}
