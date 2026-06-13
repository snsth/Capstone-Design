using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public enum InfoType {Exp, Level, Kill, Time, Health}

    public InfoType type;

    Text mytext;
    private Slider mySlider;

    void Awake()
    {
        mytext=GetComponent<Text>();
        mySlider=GetComponent<Slider>();
    }

    void LateUpdate()
    {
        switch ((type))
        {  
            case InfoType.Exp:
                float curExp=Gamemanager.instance.exp;
                float maxExp=Gamemanager.instance.nextExp[Mathf.Min(Gamemanager.instance.level, Gamemanager.instance.nextExp.Length - 1)];
                mySlider.value=curExp/maxExp;
                break;
            case InfoType.Level:
                mytext.text=string.Format("Lv.{0:F0}",Gamemanager.instance.level);
                break;
            case InfoType.Kill:
                mytext.text=string.Format("{0:F0}",Gamemanager.instance.kill);
                break;
            case InfoType.Time:
                float remainTime = Gamemanager.instance.maxGameTime - Gamemanager.instance.gameTime;
                int minutes=Mathf.FloorToInt(remainTime/60);
                int seconds=Mathf.FloorToInt(remainTime%60);
                mytext.text=string.Format("{0:D2}:{1:D2}",minutes,seconds);
                break;
            case InfoType.Health:
                float curHealth=Gamemanager.instance.health;
                float maxHealth=Gamemanager.instance.maxHealth;
                mySlider.value=curHealth/maxHealth;

                break;
            
            
        }
        

        
    }
}
