using Unity.MLAgents.Demonstrations;
using UnityEngine;

public class EnvironmentMain : MonoBehaviour
{
    #region Editor data
    [SerializeField] private GameView _gameView;
    [SerializeField] private DodgeAgent _aiAgent;
    [SerializeField] private DemonstrationRecorder _demonstrationRecorder;
    [SerializeField] private float _defaultTrainingMode;
    [SerializeField] private bool _isMainEnvironment;
    #endregion
    
    private GameManager _gameManager;

    #region Unity functions
    void Start()
    {
        if (Application.isEditor)
        {
            if (!_isMainEnvironment)
            {
                gameObject.SetActive(false);
                return;
            }
        }
        else
        {
            _demonstrationRecorder.Record = false;
        }

        _gameManager = new GameManager(_gameView, _aiAgent, _defaultTrainingMode);
    }

    void FixedUpdate()
    {
        _gameManager.Update();
    }
    #endregion
}
