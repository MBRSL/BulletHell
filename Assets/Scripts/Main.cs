using UnityEngine;

public class Main : MonoBehaviour
{
    #region Editor data
    [SerializeField] private GameView _gameView;
    #endregion
    
    private GameManager _gameManager;

    #region Unity functions
    void Start()
    {
        Application.targetFrameRate = 60;
        _gameManager = new GameManager(_gameView);
    }

    void Update()
    {
        _gameManager.Update();
    }
    #endregion
}
