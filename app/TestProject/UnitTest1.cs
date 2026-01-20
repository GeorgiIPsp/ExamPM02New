using static Program;
namespace TestProject
{
    /// <summary>
    /// Тест на проверку корретности работы исключений
    /// </summary>
    public class UnitTest
    {
            [Fact]
            public void Test()
            {
            bool proverka = false;
                double[,] matrix = new double[9,9];
                matrix = LoadDistancesFromFile();
            if(matrix == null)
            {
                proverka = true;
            }
                Assert.True(proverka);
            }
    }
}

