using Unity.MLAgents.Demonstrations;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// The entry point of a game instance.
/// Note that there may exist more than one instance since we would like to speed up training process.
/// </summary>
public class Main : MonoBehaviour
{
    #region Editor data
    [SerializeField] private GameView _gameView;
    // DRL stands for "Deep Reinforce Learning"
    [SerializeField] private DRLAgent _drlAgent;
    [SerializeField] private BehaviorParameters _behaviorParameters;
    [SerializeField] private DemonstrationRecorder _demonstrationRecorder;
    [SerializeField] private BufferSensorComponent _bufferSensorComponent;
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

        _gameManager = new GameManager(
            _gameView,
            _drlAgent,
            _defaultTrainingMode,
            _behaviorParameters.Model != null,
            _bufferSensorComponent.MaxNumObservables
        );
    }

    void FixedUpdate()
    {
        _gameManager.Update();
    }
    #endregion
}
