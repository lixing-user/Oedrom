using ChessPieceCardProduction;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "CardRegistry", menuName = "Installers/CardRegistry")]
public class CardRegistryInstaller : ScriptableObjectInstaller<CardRegistryInstaller>
{
    public CardConfig[] cardConfigs;

    public override void InstallBindings()
    {
        Container.BindInstance(cardConfigs).IfNotBound();
    }
}