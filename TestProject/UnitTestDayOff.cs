using System.Collections.Generic;
using System.Globalization;
using WebApplicationConges;
using WebApplicationConges.Model;

namespace TestProject
{
    [TestClass]
    public class UnitTestDayoff
    {
        [TestMethod]
        public void TestMethodDayOffFixed()
        {
            Assert.IsFalse(Toolkit.IsWorkingDay(DateTime.ParseExact("2022-12-25", "yyyy-MM-dd", CultureInfo.InvariantCulture)));
            Assert.IsFalse(Toolkit.IsWorkingDay(DateTime.ParseExact("2022-11-01", "yyyy-MM-dd", CultureInfo.InvariantCulture)));
            Assert.IsFalse(Toolkit.IsWorkingDay(DateTime.ParseExact("2022-11-11", "yyyy-MM-dd", CultureInfo.InvariantCulture)));
            Assert.IsFalse(Toolkit.IsWorkingDay(DateTime.ParseExact("2022-08-15", "yyyy-MM-dd", CultureInfo.InvariantCulture)));
            Assert.IsFalse(Toolkit.IsWorkingDay(DateTime.ParseExact("2022-07-14", "yyyy-MM-dd", CultureInfo.InvariantCulture)));
            Assert.IsFalse(Toolkit.IsWorkingDay(DateTime.ParseExact("2022-05-01", "yyyy-MM-dd", CultureInfo.InvariantCulture)));
            Assert.IsFalse(Toolkit.IsWorkingDay(DateTime.ParseExact("2022-05-08", "yyyy-MM-dd", CultureInfo.InvariantCulture)));
            Assert.IsFalse(Toolkit.IsWorkingDay(DateTime.ParseExact("2022-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture)));
        }

        [TestMethod]
        public void TestMethodDayOffVar()
        {
            // Lundi de pâques
            Assert.IsFalse(Toolkit.IsWorkingDay(DateTime.ParseExact("2022-04-18", "yyyy-MM-dd", CultureInfo.InvariantCulture)));

            // Ascension
            Assert.IsFalse(Toolkit.IsWorkingDay(DateTime.ParseExact("2022-05-26", "yyyy-MM-dd", CultureInfo.InvariantCulture)));
        }

        [TestMethod]
        public void TestMethodExtraDayOff()
        {
            Config config = new ();
            const string INPUT_DATA = "20221118,20221119";
            config.FuturUse1 = INPUT_DATA;            

            Assert.AreEqual(config.ExtraDaysOff.Count, 2);

            CultureInfo FrenchCulture = new CultureInfo("fr-FR", true);
            foreach (String date in config.ExtraDaysOff)
                Assert.IsTrue(DateTime.TryParseExact(date, "yyyyMMdd", FrenchCulture, DateTimeStyles.None, out _));

            config.ExtraDaysOff = config.ExtraDaysOff;
            Assert.AreEqual(config.FuturUse1, INPUT_DATA);
        }
    }
}