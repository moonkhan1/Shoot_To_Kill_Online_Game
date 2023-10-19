using Zenject;

public class ZenjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Container.Bind<PlayerSpawner>().FromComponentInChildren().AsSingle();
        // Container.Bind<NetworkInGameMessagesManager>().FromComponentInHierarchy().AsSingle();
    }
}
