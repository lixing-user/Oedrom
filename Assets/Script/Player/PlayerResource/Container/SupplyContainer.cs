public class SupplyContainer : ResourceContainerBase
{
    protected override VariableType ResourceType => VariableType.supply;
    protected override float MaxValue => _resourceManager.GetMaxValue(ResourceType);
}