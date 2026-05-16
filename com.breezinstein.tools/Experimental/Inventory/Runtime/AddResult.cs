namespace Breezinstein.Tools.Inventory
{
    /// <summary>
    /// Outcome of an <see cref="Inventory.Add(InventoryItemData,int)"/> call.
    /// </summary>
    public readonly struct AddResult
    {
        /// <summary>Number of units that were actually added.</summary>
        public readonly int Added;

        /// <summary>Number of units that did not fit and were rejected.</summary>
        public readonly int Overflow;

        public AddResult(int added, int overflow)
        {
            Added = added;
            Overflow = overflow;
        }

        public bool FullyAdded => Overflow == 0 && Added > 0;
        public bool PartiallyAdded => Added > 0 && Overflow > 0;
        public bool Rejected => Added == 0;

        public override string ToString() => $"AddResult(added={Added}, overflow={Overflow})";
    }
}
