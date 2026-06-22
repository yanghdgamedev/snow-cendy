using UnityEngine;

using GameDevelopmentKit.Scripts;
using HDG.EventDispatcher;
using UnityEngine;

public class HomeCanvas : BaseBox
{
    private static HomeCanvas _instance;

    public static HomeCanvas GetInstance()
    {
        if (_instance == null)
        {
            _instance = Instantiate(Resources.Load<HomeCanvas>("UI/HomeCanvas"));
        }
        return _instance;
    }

    public void PlayGame()
    {
        this.PostEvent(EventID.StartGame);
        Close();
    }
}
