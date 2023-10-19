using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionListHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private GameObject _sessionItemListObject;
    [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;

    private void Awake()
    {
        ClearList();
    }
    public void ClearList()
    {
        foreach (Transform child in _verticalLayoutGroup.transform)        
        {
            Destroy(child.gameObject);
        }
        
        _statusText.gameObject.SetActive(false);
    }

    public void AddToList(SessionInfo sessionInfo)
    {
        SessionListItemHandler addedSessionListItemHandler =
            Instantiate(_sessionItemListObject, _verticalLayoutGroup.transform).GetComponent<SessionListItemHandler>();
        addedSessionListItemHandler.SetInfo(sessionInfo);
        
        addedSessionListItemHandler.OnJoinSession += AddedSessionListItemHandlerOnJoinSession;
    }

    private void AddedSessionListItemHandlerOnJoinSession(SessionInfo sessionInfo)
    {
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();

        networkRunnerHandler.JoinGame(sessionInfo);

        MainMenuUIHandler mainMenuUIHandler = FindObjectOfType<MainMenuUIHandler>();
        mainMenuUIHandler.OnJoiningServer();
    }

    public void OnNoSessionFound()
    {
        ClearList();
        _statusText.text = "No game session found";
        _statusText.gameObject.SetActive(true);
    }

    public void OnLookForSession()
    {
        ClearList();
        _statusText.text = "Looking for sessions...";
        _statusText.gameObject.SetActive(true);
    }
}
