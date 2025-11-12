using Game.Systems.CameraSystem;
using Game.Systems.WinConditions;
using UnityEngine;
using Zenject;

public class GameSceneInstaller : MonoInstaller
{
#if UNITY_EDITOR
    [Header("Debug Tools")]
    [field: SerializeField] private bool infinitePower = false;
    [field: SerializeField] private bool infiniteResources = false;
    [field: SerializeField] private bool highIncome = false;
    [field: SerializeField] private bool freeConstruction = false;
#endif

    [Header("Configurations")]
    [field: SerializeField] private GridConfig gridConfig;
    [field: SerializeField] private WinConditionConfig winConditionConfig;

    [Header("Prefabs")]
    [field: SerializeField] private BuildingManager buildingManagerPrefab;
    [field: SerializeField] private PlayerInput playerInputPrefab;
    [field: SerializeField] private SceneLoader sceneLoaderPrefab;
    [field: SerializeField] private WaveManager waveManagerPrefab;
    [field: SerializeField] private GameCyclesManager gameCyclesManagerPrefab;

    public override void InstallBindings()
    {
        DeclareSignals();
        BindSystems();
        BindPrefabs();
    }

    private void DeclareSignals()
    {
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<SecondTickSignal>();
        Container.DeclareSignal<ResourceChangedSignal>();
        Container.DeclareSignal<EnergyUpdateSignal>();
        Container.DeclareSignal<GameCyclesManager>();

        Container.DeclareSignal<OnInsufficientResourcesSignal>().OptionalSubscriber();

        Container.DeclareSignal<OnCoreLevelChangeSignal>();
        Container.DeclareSignal<OnCoreHealthChangeSignal>();

        Container.DeclareSignal<OnWaveTimeUpdateSignal>().OptionalSubscriber();


        Container.DeclareSignal<OnBuildingPlacedSignal>();
        Container.DeclareSignal<OnBuildingClickSignal>();
        Container.DeclareSignal<OnBuildingDeselectSignal>();
        Container.DeclareSignal<OnBuildingSelectionCanceledSignal>();
        Container.DeclareSignal<OnBuildingCreatedSignal>();
        Container.DeclareSignal<OnBuildingPointerEnterSignal>().OptionalSubscriber();
        Container.DeclareSignal<OnBuildingPointerExitSignal>().OptionalSubscriber();
        Container.DeclareSignal<OnBuildingDemolishSignal>();

        Container.DeclareSignal<OnCellClickSignal>();
        Container.DeclareSignal<OnCellPointerEnterSignal>().OptionalSubscriber();
        Container.DeclareSignal<OnCellPointerExitSignal>().OptionalSubscriber();

        Container.DeclareSignal<OnAbilitySlotPointerEnterSignal>();
        Container.DeclareSignal<OnAbilitySlotPointerExitSignal>();

        Container.DeclareSignal<OnWinConditionMet>();
        
        Container.DeclareSignal<OnGameWonSignal>();
        Container.DeclareSignal<OnGameLostSignal>();
    }

    private void BindSystems()
    {
        Container.BindInterfacesAndSelfTo<TickService>().FromNew().AsSingle();
        Container.BindInterfacesAndSelfTo<ResourceManager>().FromNew().AsSingle().WithArguments(infiniteResources, highIncome);

        Container.Bind<UIManager>().AsSingle();
        Container.Bind<Grid>().AsSingle().WithArguments(gridConfig).NonLazy();
        Container.BindInterfacesAndSelfTo<GameManager>().AsSingle().NonLazy();
        Container.Bind<EnergySystem>().AsSingle().WithArguments(infinitePower).NonLazy();
        Container.Bind<SpeedManager>().AsSingle().NonLazy();
        Container.Bind<BuildingRegistry>().AsSingle().NonLazy();
        
        BindWinCondition();
    }

    private void BindPrefabs()
    {
        Container.Bind<BuildingManager>().FromComponentInNewPrefab(buildingManagerPrefab).AsSingle().WithArguments(freeConstruction);
        Container.Bind<PlayerInput>().FromComponentInNewPrefab(playerInputPrefab).AsSingle();
        Container.Bind<SceneLoader>().FromComponentInNewPrefab(sceneLoaderPrefab).AsSingle();
        Container.Bind<WaveManager>().FromComponentInNewPrefab(waveManagerPrefab).AsSingle().NonLazy();
        Container.Bind<GameCyclesManager>().FromComponentInNewPrefab(gameCyclesManagerPrefab).AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<GameCameraProvider>().FromComponentsInHierarchy().AsSingle().NonLazy();
        Container.Bind<GridGO>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<TooltipManager>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<DeckManager>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<GlobalUpgradeManager>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<NotificationSystem>().FromComponentInHierarchy().AsSingle().NonLazy();
    }
    
    private void BindWinCondition()
    {
        if (winConditionConfig is ResourceWinConfigSO config)
        {
            Container.BindInterfacesAndSelfTo<ResourceGoalCondition>().AsSingle().WithArguments(config).NonLazy();
        }
    }
}
