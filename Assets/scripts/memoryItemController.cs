using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class memoryItemController : MonoBehaviour
{
    private float activation = 0;
    private float asymptote = 1;
    public TMP_Text itemIndexText;

    public int inFoA;  // =1: in FoA; =0: not in FoA
    public float currentProcessDuration = 0;
    private float currentDerivation;

    // Start is called before the first frame update
    void Start()
    {
        activation = 0;
        inFoA = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (GlobalManager.timerStart)
        {
            GetDerivation();
            ChangeActivation(currentDerivation);

            Vector3 currentHeight = transform.position;
            currentHeight.y = activation;
            transform.position = currentHeight;
        }
        else
        {
            activation = 0;
        }
    }

    public (float, float) Encoding(float duration)
    {
        float encodingStrength = DetermineIncreasingStrength(duration);
        float activationIncreasing = (asymptote - activation) * encodingStrength;
        float derivation;
        if(duration > 0)
        {
            derivation = activationIncreasing / duration;  // linear
        }
        else
        {
            derivation = 0;
        }
        
        return (activationIncreasing, derivation);
    }

    /*
    public (float, float) Refreshing(float duration)
    {
        float refreshingStrength = DetermineIncreasingStrength(duration);
        float activationIncreasing = (asymptote - activation) * refreshingStrength;
        float derivation;
        if (duration > 0)
        {
            derivation = activationIncreasing / duration;  // linear
        }
        else
        {
            derivation = 0;
        }
        return (activationIncreasing, derivation);
    }
    */

    public void Decay(float duration)
    {
        activation *= Mathf.Exp(-GlobalManager.DecayRate * duration);
    }


    /// <summary>
    /// 编码或刷新的时间越长，编码强度越高，激活值上升得越多
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    public float DetermineIncreasingStrength(float duration)
    {
        float encodingStrength = 1 - Mathf.Exp(-GlobalManager.EncodingRate*duration);
        return encodingStrength;
    }

    public bool Retrieval()
    {
        float retrieveProbability = 
            1 / (1 + Mathf.Exp(-(activation - GlobalManager.RecallThreshold) / GlobalManager.RecallNoise));
        if (Random.value < retrieveProbability)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void DetermineItemIndex(int currentIndex)
    {
        itemIndexText.text = currentIndex.ToString();
    }


    /// <summary>
    /// get the derivation of the changing activation value
    /// </summary>
    private void GetDerivation()
    {
        float totalIncreasing;
        currentDerivation = 0;
        HandleCurrentProcessDuration();

        // get encoding
        if (inFoA == 1)
        {
            (totalIncreasing, currentDerivation) = Encoding(currentProcessDuration);
        }        

        // get forgetting
        currentDerivation += -GlobalManager.DecayRate * activation;
    }

    private void ChangeActivation(float derivation)
    {
        activation += derivation * Time.deltaTime;
    }

    private void HandleCurrentProcessDuration()
    {
        if (inFoA == 1)
        {
            currentProcessDuration = GlobalManager.internalStateTime;
        }
        else
        {
            currentProcessDuration = 0;
        }
    }
}
