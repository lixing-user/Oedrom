public class PopularityContainer : ResourceContainerBase
{
    protected override VariableType ResourceType => VariableType.popularity;
    protected override float MaxValue => _resourceManager.GetMaxValue(ResourceType);
}