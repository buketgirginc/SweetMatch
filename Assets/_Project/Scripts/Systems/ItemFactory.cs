using System;
using SweetMatch.Model.Items;

namespace SweetMatch.Systems
{
    public class ItemFactory : IItemFactory
    {
        private static readonly SweetType[] AllSweetTypes =
            (SweetType[])Enum.GetValues(typeof(SweetType));

        public GridItem CreateRandomSweet()
        {
            var type = AllSweetTypes[UnityEngine.Random.Range(0, AllSweetTypes.Length)];
            return new SweetItem(type);
        }

        public GridItem CreateCandyBar(CandyBarDirection direction) => new CandyBarItem(direction);

        public GridItem CreateCupcake() => new CupcakeItem();

        public GridItem CreateCroissant() => new CroissantItem();
    }
}