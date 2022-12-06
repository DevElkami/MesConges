using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using WebApplicationConges;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace TestProject
{
    [TestClass]
    public class UnitTestDatabase
    {
        private const String TEST_PATTERN = "TEST__*-__@@";
        private const String TEST_PATTERN_SERVICE = TEST_PATTERN + "TestService";

        [TestInitialize]
        public void TestMethodeInit()
        {
            Toolkit.InitConfiguration();
            Db.Instance.Init(Toolkit.Configuration[Toolkit.ConfigEnum.DbConnectionString.ToString()], Toolkit.Configuration[Toolkit.ConfigEnum.DbName.ToString()]);

            Service service = new();
            service.Name = TEST_PATTERN + "TestService";
            Db.Instance.DataBase.ServiceRepository.Insert(service);
            Assert.IsTrue(Db.Instance.DataBase.ServiceRepository.GetAll().Count > 0);
        }

        [TestMethod]
        public void TestMethodUser()
        {
#pragma warning disable CS8600 // Conversion de littéral ayant une valeur null ou d'une éventuelle valeur null en type non-nullable.
            Service service = Db.Instance.DataBase.ServiceRepository.GetAll().FirstOrDefault();
#pragma warning restore CS8600 // Conversion de littéral ayant une valeur null ou d'une éventuelle valeur null en type non-nullable.
            Assert.IsNotNull(service);

            User user = new();
            user.Name = TEST_PATTERN + "TestUser";
            user.Email = user.Name + "@mstest.com";
            user.HashPwd = Toolkit.CreateSHAHash(user.Email);
            user.ServiceId = (int)service.Id;

            Db.Instance.DataBase.UserRepository.Delete(user);
            Db.Instance.DataBase.UserRepository.Insert(user);

            user.Surname = "Jambain";
            Db.Instance.DataBase.UserRepository.Update(user);
            Assert.AreEqual(Db.Instance.DataBase.UserRepository.Get(user.Email).Surname, user.Surname);

            user.Login = "LoginJambain";
            Db.Instance.DataBase.UserRepository.Update(user);
            Assert.AreEqual(Db.Instance.DataBase.UserRepository.Get(user.Email).Login, user.Login);

            user.IsDrh = true;
            Db.Instance.DataBase.UserRepository.Update(user);
            Assert.AreEqual(Db.Instance.DataBase.UserRepository.Get(user.Email).IsDrh, true);

            user.IsDrh = false;
            Db.Instance.DataBase.UserRepository.Update(user);
            Assert.AreEqual(Db.Instance.DataBase.UserRepository.Get(user.Email).IsDrh, false);

            user.IsAdmin = true;
            Db.Instance.DataBase.UserRepository.Update(user);
            Assert.AreEqual(Db.Instance.DataBase.UserRepository.Get(user.Email).IsAdmin, true);

            user.IsAdmin = false;
            Db.Instance.DataBase.UserRepository.Update(user);
            Assert.AreEqual(Db.Instance.DataBase.UserRepository.Get(user.Email).IsAdmin, false);

            Db.Instance.DataBase.UserRepository.Delete(user);
            Assert.IsNull(Db.Instance.DataBase.UserRepository.Get(user.Email));
        }

        [TestMethod]
        public void TestMethodService()
        {
            Assert.IsTrue(Db.Instance.DataBase.ServiceRepository.GetAll().Count > 0);
        }

        [TestMethod]
        public void TestMethodConfig()
        {
            Assert.IsNotNull(Db.Instance.DataBase.ConfigRepository.Get());
        }

        [TestMethod]
        public void TestMethodBackup()
        {
            string backupFile = "bla56fds4fd487ty-data.tmp";
            Db.Instance.DataBase.Backup(backupFile);
            File.Delete(Path.ChangeExtension(backupFile, "zip"));

            Assert.IsFalse(File.Exists(backupFile));
            Assert.IsFalse(File.Exists(Path.ChangeExtension(backupFile, "zip")));
        }

        [TestCleanup]
        public void TestMethodeDestroy()
        {
            Service service = new();
            service.Name = TEST_PATTERN + "TestService";
            Db.Instance.DataBase.ServiceRepository.Delete(service);
        }
    }
}