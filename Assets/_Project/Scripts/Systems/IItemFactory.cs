using SweetMatch.Model.Items;

namespace SweetMatch.Systems
{
    public interface IItemFactory
    {
        GridItem CreateRandomSweet();
        GridItem CreateCandyBar(CandyBarDirection direction);
        GridItem CreateCupcake();
        GridItem CreateCroissant();
    }
}