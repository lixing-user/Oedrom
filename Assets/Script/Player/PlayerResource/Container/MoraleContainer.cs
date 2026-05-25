public class MoraleContainer : ResourceContainerBase
{
    protected override VariableType ResourceType => VariableType.morale;
    protected override float MaxValue => _resourceManager.GetMaxValue(ResourceType);
}