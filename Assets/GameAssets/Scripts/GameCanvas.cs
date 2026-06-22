using UnityEngine;

using GameDevelopmentKit.Scripts;
using HDG.EventDispatcher;
using UnityEngine;

public class GameCanvas : BaseBox
{
    private static GameCanvas _instance;

    public static GameCanvas GetInstance()
    {
        if (_instance == null)
        {
            _instance = Instantiate(Resources.Load<GameCanvas>("UI/GameCanvas"));
        }
        return _instance;
    }

    public void ReplayGame()
    {
        this.PostEvent(EventID.ReplayGame);
    }

    public void BackHome()
    {
        this.PostEvent(EventID.CancelGame);
        Close();
        HomeCanvas.GetInstance().Show();
    }
}
