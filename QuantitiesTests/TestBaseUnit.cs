namespace AstroMultimedia.Quantities.Tests;

[TestClass]
public class TestBaseUnit
{
    [TestMethod]
    public void TestBaseUnitConstructor()
    {
        BaseUnit bu = new("m");
    }

    [TestMethod]
    public void TestUniqueSymbols()
    {
        IEnumerable<string> clashes = BaseUnit.Clashes();
        Assert.AreEqual(0, clashes.Count());
    }
}
