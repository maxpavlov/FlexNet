using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using System.Xml.Serialization;

namespace SenseNet.ContentRepository.Tests.Security
{
	[TestClass()]
    public class PermissionTest : TestBase
	{
		Node __userFolder;
		User __dincsi;
		User __afekete;
		User __alex;
		User __molnarg;

        Node UserFolder
		{
			get
			{
				if (__userFolder == null)
                {
					//__userFolder = Folder.Load(Path.GetParentPath(User.Administrator.Path));
                    var userFolder = Node.LoadNode(RepositoryPath.GetParentPath(User.Administrator.Path));
                    if (userFolder == null)
                        throw new ApplicationException("UserFolder cannot be found.");
                    __userFolder = userFolder as Node;
                }
				return __userFolder;
			}
		}
		User Dincsi
		{
			get
			{
				if (__dincsi == null)
					__dincsi = LoadOrCreateUser("dincsitest", "Din Chi", UserFolder);
				return __dincsi;
			}
		}
		User AFekete
		{
			get
			{
				if (__afekete == null)
					__afekete = LoadOrCreateUser("afeketetest", "Fekete Andras", UserFolder);
				return __afekete;
			}
		}
		User Alex
		{
			get
			{
				if (__alex == null)
					__alex = LoadOrCreateUser("alextest", "Kiss Sandor", UserFolder);
				return __alex;
			}
		}
		User MolnarG
		{
			get
			{
				if (__molnarg == null)
					__molnarg = LoadOrCreateUser("molnargtest", "Molnar G", UserFolder);
				return __molnarg;
			}
		}


		public static User LoadOrCreateUser(string name, string fullName, Node parentFolder)
		{
            var path = RepositoryPath.Combine(parentFolder.Path, name);
            AddPathToDelete(path);

            var user = Node.LoadNode(path) as User ?? new User(parentFolder) { Name = name };
            user.Email = name + "@email.com";
            user.Enabled = true;
            user.FullName = fullName;
            user.Save();

            return user;
		}

		#region Test infrastructure
		private TestContext testContextInstance;
		public override TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}
		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion
		#endregion

		static List<string> _pathsToDelete = new List<string>();
		static void AddPathToDelete(string path)
		{
			lock (_pathsToDelete)
			{
				if (_pathsToDelete.Contains(path))
					return;
				_pathsToDelete.Add(path);
			}
		}
		[ClassInitialize]
		public static void InitializePlayground(TestContext testContext)
		{
            Repository.Root.Security.SetPermission(Group.Everyone, true, PermissionType.Open, PermissionValue.NonDefined);
		}
		[ClassCleanup]
		public static void DestroyPlayground()
		{
			foreach (string path in _pathsToDelete)
			{
				try
				{
					Node n = Node.LoadNode(path);
					if (n != null)
                        Node.ForceDelete(path);
				}
				catch
				{
					throw;
				}
			}
		}


		[TestMethod()]
		public void Permission_AllowDenyNotDefined()
		{
			string s;
			Folder alma = CreateBrandNewFolder(RepositoryPath.Combine(Repository.RootPath, "Alma"));
			User user = Dincsi;

            alma.Security.SetPermission(user, true, PermissionType.DeleteOldVersion, PermissionValue.Allow);
			Assert.IsTrue(alma.Security.HasPermission((IUser)user, PermissionType.DeleteOldVersion), "#1");
			s = SecurityEntriesToString(alma);
			Assert.IsTrue(s.Contains(String.Concat("<Entry definedOn='", alma.Id, "' principalId='", user.Id, "'>")), "#2");
			Assert.IsTrue(s.Contains("<DeleteOldVersion value='Allow' />"), "#3");

            alma.Security.SetPermission(user, true, PermissionType.DeleteOldVersion, PermissionValue.Deny);
            Assert.IsFalse(alma.Security.HasPermission((IUser)user, PermissionType.DeleteOldVersion), "#4");
			s = SecurityEntriesToString(alma);
			Assert.IsTrue(s.Contains(String.Concat("<Entry definedOn='", alma.Id, "' principalId='", user.Id, "'>")), "#5");
			Assert.IsTrue(s.Contains("<DeleteOldVersion value='Deny' />"), "#6");

            alma.Security.SetPermission(user, true, PermissionType.DeleteOldVersion, PermissionValue.NonDefined);
            Assert.IsFalse(alma.Security.HasPermission((IUser)user, PermissionType.DeleteOldVersion), "#7");
            alma.Security.SetPermission(user, true, PermissionType.See, PermissionValue.NonDefined);
			s = SecurityEntriesToString(alma);
			Assert.IsFalse(s.Contains(String.Concat("<Entry definedOn='", alma.Id, "' principalId='", user.Id, "'>")), "#8");
		}

		[TestMethod()]
		public void Permission_01()
		{
			//	1. 
			//	- Letrehozunk egy "alma" mappat.

			Folder alma = CreateBrandNewFolder(RepositoryPath.Combine(Repository.RootPath, "Alma"));

			//	Elvart eredmeny: A "alma" mappanak azonos jogosultsagai vannak, mint a parent directorynak.

			string almaJogok = SecurityEntriesToString(alma);
			string almaParentJogok = SecurityEntriesToString(alma.Parent);

			Assert.IsTrue(almaJogok == almaParentJogok);
		}
		[TestMethod()]
		public void Permission_02()
		{
			//	2.
			//	- Letrehozunk egy "alma" mappat.

			Folder alma = CreateBrandNewFolder(RepositoryPath.Combine(Repository.RootPath, "Alma"));

			//	- Az "alma" mappan hozzaadtunk sn\dincsi usernek olvasasi jogosultsagot.

			//alma.Jog += dincsi.Read;
            alma.Security.SetPermission(Dincsi, true, PermissionType.Open, PermissionValue.Allow);

			//	- Letrehozunk "alma"-ba egy uj, "korte" mappat.

			Folder korte = CreateBrandNewFolder(RepositoryPath.Combine(alma.Path, "Korte"));

			string korteJogok = SecurityEntriesToString(korte);
			string almaJogok = SecurityEntriesToString(alma);

			//	Elvart eredmeny: Korten ugyan azok a jogosultsagok kell, hogy legyenek, mint alman. 
			//	Alman meg ugyan azok mint az o szulo mappajan. HIBAS. Helyette: Alman es Korten egyezo jogok vannak

			Assert.IsTrue(korteJogok == almaJogok);
		}
		[TestMethod()]
		public void Permission_03()
		{
			//	3.
			//	- Gyoker elemre adunk olvasasi jogosultsagot sn\alex felhasznalonak.
			PermissionValue alexOpen = Repository.Root.Security.GetPermission((IUser)Alex, PermissionType.Open); //-- save original
            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, PermissionValue.Allow);

			//	- Letrehozunk egy "alma" mappat gyoker elem ala.
			Folder alma = CreateBrandNewFolder(RepositoryPath.Combine(Repository.RootPath, "Alma"));

			//	- sn\molnarg usernek adunk read jogosultsagot alma mappara.
            alma.Security.SetPermission(MolnarG, true, PermissionType.Open, PermissionValue.Allow);

			//	- Letrehozunk "alma"-ba egy uj, "korte" mappat.
			Folder korte = CreateBrandNewFolder(RepositoryPath.Combine(alma.Path, "Korte"));

			//	- Gyoker elemrol letoroSetPermissionljuk sn\alex jogosultsagait.
            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, PermissionValue.NonDefined);

			//	Elvart eredmeny: Alma mappanak ugyan azok a jogosultsagai kell, hogy legyen, mint gyoker es a ratett sn\molnarg read. 
			//	Korte mappanak azonos jogosultsagokkal kell rendelkeznie, mint almanak.

            bool rootAlex = Repository.Root.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool almaAlex = alma.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool korteAlex = korte.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool almaMolnarG = alma.Security.HasPermission((IUser)MolnarG, PermissionType.Open);
            bool korteMolnarG = korte.Security.HasPermission((IUser)MolnarG, PermissionType.Open);

            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, alexOpen); //-- restore original

			Assert.IsFalse(rootAlex, "#1");
			Assert.IsFalse(almaAlex, "#2");
			Assert.IsFalse(korteAlex, "#3");
			Assert.IsTrue(almaMolnarG, "#4");
			Assert.IsTrue(korteMolnarG, "#5");
		}
		[TestMethod()]
		public void Permission_04()
		{
			//	4.
			//	- Gyoker elemre adunk olvasasi jogosultsagot sn\alex felhasznalonak.
            PermissionValue alexOpen = Repository.Root.Security.GetPermission((IUser)Alex, PermissionType.Open); //-- save original
            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, PermissionValue.Allow);

			//	- Letrehozunk egy "alma" mappat gyoker elem ala.
			Folder alma = CreateBrandNewFolder(RepositoryPath.Combine(Repository.RootPath, "Alma"));

			//	- sn\molnarg usernek adunk read jogosultsagot alma mappara.
            alma.Security.SetPermission(MolnarG, true, PermissionType.Open, PermissionValue.Allow);

			//	- Letrehozunk "alma"-ba egy uj, "korte" mappat.
			Folder korte = CreateBrandNewFolder(RepositoryPath.Combine(alma.Path, "Korte"));

			//	- Gyoker elemre sn\alex-nak adunk modositasi jogot.
            PermissionValue alexSave = Repository.Root.Security.GetPermission((IUser)Alex, PermissionType.Save); //-- save original
            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Save, PermissionValue.Allow);

			//	Elvart eredmeny:  Alma mappanak ugyan azok a jogosultsagai kell, hogy legyen, mint gyoker es a ratett sn\molnarg read. 
			//	Korte mappanak azonos jogosultsagokkal kell rendelkeznie, mint almanak. Azaz:
			//  - root       alexOpen, alexSave
			//  -   alma     alexOpen, alexSave, molnarGOpen
			//  -     korte  alexOpen, alexSave, molnarGOpen
            bool rootAlexOpen = Repository.Root.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool almaAlexOpen = alma.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool korteAlexOpen = korte.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool almaMolnarGOpen = alma.Security.HasPermission((IUser)MolnarG, PermissionType.Open);
            bool korteMolnarGOpen = korte.Security.HasPermission((IUser)MolnarG, PermissionType.Open);
            bool rootAlexSave = Repository.Root.Security.HasPermission((IUser)Alex, PermissionType.Save);
            bool almaAlexSave = alma.Security.HasPermission((IUser)Alex, PermissionType.Save);
            bool korteAlexSave = korte.Security.HasPermission((IUser)Alex, PermissionType.Save);
            bool almaMolnarGSave = alma.Security.HasPermission((IUser)MolnarG, PermissionType.Save);
            bool korteMolnarGSave = korte.Security.HasPermission((IUser)MolnarG, PermissionType.Save);

            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, alexOpen); //-- restore original
            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Save, alexSave); //-- restore original

			Assert.IsTrue(rootAlexOpen, "#1");
			Assert.IsTrue(almaAlexOpen, "#2");
			Assert.IsTrue(korteAlexOpen, "#3");
			Assert.IsTrue(almaMolnarGOpen, "#4");
			Assert.IsTrue(korteMolnarGOpen, "#5");
			Assert.IsTrue(rootAlexSave, "#6");
			Assert.IsTrue(almaAlexSave, "#7");
			Assert.IsTrue(korteAlexSave, "#8");
			Assert.IsFalse(almaMolnarGSave, "#9");
			Assert.IsFalse(korteMolnarGSave, "#10");
		}
		[TestMethod()]
		public void Permission_05_PermissionInheritance()
		{
			//	5.
			//	- Gyoker elemre adunk olvasasi jogosultsagot sn\alex felhasznalonak.
            PermissionValue alexOpen = Repository.Root.Security.GetPermission((IUser)Alex, PermissionType.Open); //-- save original
            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, PermissionValue.Allow);

			//	- Letrehozunk egy "alma" mappat gyoker elem ala.
			Folder alma = CreateBrandNewFolder(RepositoryPath.Combine(Repository.RootPath, "Alma"));

			//	- sn\molnarg usernek adunk read jogosultsagot alma mappara.
            alma.Security.SetPermission(MolnarG, true, PermissionType.Open, PermissionValue.Allow);

			//	- Letrehozunk "alma"-ba egy uj, "korte" mappat.
			Folder korte = CreateBrandNewFolder(RepositoryPath.Combine(alma.Path, "Korte"));

			//	- Gyoker elemre sn\afekete-nek adunk olvasasi jogot.
            PermissionValue afeketeOpen = Repository.Root.Security.GetPermission((IUser)AFekete, PermissionType.Open); //-- save original
            Repository.Root.Security.SetPermission(AFekete, true, PermissionType.Open, PermissionValue.Allow);

			//	Elvart eredmeny: Alma mappanak ugyan azok a jogosultsagai kell, hogy legyen, mint gyoker es a ratett sn\molnarg read. 
			//	Korte mappanak azonos jogosultsagokkal kell rendelkeznie, mint almanak.
			//  - root       alexOpen,              afeketeOpen
			//  -   alma     alexOpen, molnarGOpen, afeketeOpen
			//  -     korte  alexOpen, molnarGOpen, afeketeOpen

            bool rootAlexOpen = Repository.Root.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool rootAFeketeOpen = Repository.Root.Security.HasPermission((IUser)AFekete, PermissionType.Open);
            bool rootMolnarGOpen = Repository.Root.Security.HasPermission((IUser)MolnarG, PermissionType.Open);
            bool almaAlexOpen = alma.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool almaAFeketeOpen = alma.Security.HasPermission((IUser)AFekete, PermissionType.Open);
            bool almaMolnarGOpen = alma.Security.HasPermission((IUser)MolnarG, PermissionType.Open);
            bool korteAlexOpen = korte.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool korteAFeketeOpen = korte.Security.HasPermission((IUser)AFekete, PermissionType.Open);
            bool korteMolnarGOpen = korte.Security.HasPermission((IUser)MolnarG, PermissionType.Open);

            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, alexOpen); //-- restore original
            Repository.Root.Security.SetPermission(AFekete, true, PermissionType.Open, afeketeOpen); //-- restore original

			Assert.IsTrue(rootAlexOpen, "#1");
			Assert.IsTrue(rootAFeketeOpen, "#2");
			Assert.IsFalse(rootMolnarGOpen, "#3");
			Assert.IsTrue(almaAlexOpen, "#4");
			Assert.IsTrue(almaAFeketeOpen, "#5");
			Assert.IsTrue(almaMolnarGOpen, "#6");
			Assert.IsTrue(korteAlexOpen, "#7");
			Assert.IsTrue(korteAFeketeOpen, "#8");
			Assert.IsTrue(korteMolnarGOpen, "#9");
		}
		[TestMethod()]
		public void Permission_06()
		{
			//	6
			//	- Gyoker elemre adunk olvasasi jogosultsagot sn\alex felhasznalonak.
            PermissionValue alexOpen = Repository.Root.Security.GetPermission((IUser)Alex, PermissionType.Open); //-- save original
            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, PermissionValue.Allow);

			//	- Letrehozunk egy "alma" mappat gyoker elem ala.
			Folder alma = CreateBrandNewFolder(RepositoryPath.Combine(Repository.RootPath, "Alma"));

			//	- sn\molnarg usernek adunk read jogosultsagot alma mappara.
            alma.Security.SetPermission(MolnarG, true, PermissionType.Open, PermissionValue.Allow);

			//	- Letrehozunk "alma"-ba egy uj, "korte" mappat.
			Folder korte = CreateBrandNewFolder(RepositoryPath.Combine(alma.Path, "Korte"));

			//	- Kortemappaban letrehozunk egy Feladat.doc fajlt.
			File file = CreateBrandNewFile(RepositoryPath.Combine(korte.Path, "Feladat.doc"));

			//	Elvart eredmeny: Alma mappanak ugyan azok a jogosultsagai kell, hogy legyen, mint gyoker es a ratett sn\molnarg read. 
			//	Korte mappanak azonos jogosultsagokkal kell rendelkeznie, mint almanak. Feladat.doc jogosultsagainak meg kell egyezniuk 
			//	Korte mappa jogosultsagaival.
			//  - root        alexOpen
			//  -   alma      alexOpen, molnarGOpen
			//  -     korte   alexOpen, molnarGOpen
			//  -       file  alexOpen, molnarGOpen

            bool rootAlexOpen = Repository.Root.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool rootMolnarGOpen = Repository.Root.Security.HasPermission((IUser)MolnarG, PermissionType.Open);
            bool almaAlexOpen = alma.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool almaMolnarGOpen = alma.Security.HasPermission((IUser)MolnarG, PermissionType.Open);
            bool korteAlexOpen = korte.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool korteMolnarGOpen = korte.Security.HasPermission((IUser)MolnarG, PermissionType.Open);
            bool fileAlexOpen = file.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool fileMolnarGOpen = file.Security.HasPermission((IUser)MolnarG, PermissionType.Open);

            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, alexOpen); //-- restore original

			Assert.IsTrue(rootAlexOpen, "#1");
			Assert.IsFalse(rootMolnarGOpen, "#2");
			Assert.IsTrue(almaAlexOpen, "#3");
			Assert.IsTrue(almaMolnarGOpen, "#4");
			Assert.IsTrue(korteAlexOpen, "#5");
			Assert.IsTrue(korteMolnarGOpen, "#6");
			Assert.IsTrue(fileAlexOpen, "#7");
			Assert.IsTrue(fileMolnarGOpen, "#8");
		}
		[TestMethod()]
		public void Permission_07()
		{
			//	7
			//	- Gyoker elemre adunk olvasasi jogosultsagot sn\alex felhasznalonak.
            PermissionValue alexOpen = Repository.Root.Security.GetPermission((IUser)Alex, PermissionType.Open); //-- save original
            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, PermissionValue.Allow);

			//	- Letrehozunk egy "alma" mappat gyoker elem ala.
			Folder alma = CreateBrandNewFolder(RepositoryPath.Combine(Repository.RootPath, "Alma"));

			//	- sn\molnarg usernek adunk read jogosultsagot alma mappara.
            alma.Security.SetPermission(MolnarG, true, PermissionType.Open, PermissionValue.Allow);

			//	- Letrehozunk "alma"-ba egy uj, "korte" mappat.
			Folder korte = CreateBrandNewFolder(RepositoryPath.Combine(alma.Path, "Korte"));

			//	- Kortemappaban letrehozunk egy Feladat.doc fajlt.
			File file = CreateBrandNewFile(RepositoryPath.Combine(korte.Path, "Feladat.doc"));

			//	- Feladat.doc fajlra felveszunk sn\alex felhasznalonak olvasasi jogosultsag tiltast.
            file.Security.SetPermission(Alex, true, PermissionType.Open, PermissionValue.Deny);

			//	Elvart eredmeny: Feladat.doc fajlt sn\alex user ne tudja olvasni.
            bool fileAlexOpen = file.Security.HasPermission((IUser)Alex, PermissionType.Open);

            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, alexOpen); //-- restore original

			Assert.IsFalse(fileAlexOpen, "#1");
		}
		[TestMethod()]
		public void Permission_08()
		{
			//	8
			//	- Gyoker elemre adunk olvasasi jogosultsagot sn\alex felhasznalonak.
            PermissionValue alexOpen = Repository.Root.Security.GetPermission((IUser)Alex, PermissionType.Open); //-- save original
            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, PermissionValue.Allow);

			//	- Letrehozunk egy "alma" mappat gyoker elem ala.
			Folder alma = CreateBrandNewFolder(RepositoryPath.Combine(Repository.RootPath, "Alma"));

			//	- sn\molnarg usernek adunk read jogosultsagot alma mappara.
            alma.Security.SetPermission(MolnarG, true, PermissionType.Open, PermissionValue.Allow);

			//	- Letrehozunk "alma"-ba egy uj, "korte" mappat.
			Folder korte = CreateBrandNewFolder(RepositoryPath.Combine(alma.Path, "Korte"));

			//	- Kortemappaban letrehozunk egy Feladat.doc fajlt.
			File file = CreateBrandNewFile(RepositoryPath.Combine(korte.Path, "Feladat.doc"));

			//	- Feladat.doc fajlra felveszunk sn\afekete felhasznalonak olvasasi jogosultsag tiltast.
            file.Security.SetPermission(AFekete, true, PermissionType.Open, PermissionValue.Deny);

			//	- Gyoker elemre beallitunk sn\afekete felhasznalonak olvasas engedelyezeset.
            Repository.Root.Security.SetPermission(AFekete, true, PermissionType.Open, PermissionValue.Allow);

			//	Elvart eredmeny: Alma es Korte mappan sn\afekete usernek kell legyen olvasasi joga. Feladat.doc fajlt sn\afekete user 
			//	ne tudja olvasni.

            bool almaAFeketeOpen = alma.Security.HasPermission((IUser)AFekete, PermissionType.Open);
            bool korteAFeketeOpen = korte.Security.HasPermission((IUser)AFekete, PermissionType.Open);
            bool fileAFeketeOpen = file.Security.HasPermission((IUser)AFekete, PermissionType.Open);

            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, alexOpen); //-- restore original

			Assert.IsTrue(almaAFeketeOpen, "#1");
			Assert.IsTrue(korteAFeketeOpen, "#2");
			Assert.IsFalse(fileAFeketeOpen, "#3");
		}

        //legyen örökre átkozott
        [TestMethod()]
        public void Permission_09()
        {
            //	9
            //	- Gyoker elemre adunk olvasasi jogosultsagot sn\alex felhasznalonak.
            PermissionValue alexOpen = Repository.Root.Security.GetPermission((IUser)Alex, PermissionType.Open); //-- save original
            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, PermissionValue.Allow);

            //	- Letrehozunk egy "alma" mappat gyoker elem ala.
            Folder alma = CreateBrandNewFolder(RepositoryPath.Combine(Repository.RootPath, "Alma"));

            //	- sn\molnarg usernek adunk read jogosultsagot alma mappara.
            alma.Security.SetPermission(MolnarG, true, PermissionType.Open, PermissionValue.Allow);

            //	- Letrehozunk "alma"-ba egy uj, "korte" mappat.
            Folder korte = CreateBrandNewFolder(RepositoryPath.Combine(alma.Path, "Korte"));

            //	- Kortemappaban letrehozunk egy Feladat.doc fajlt.
            File file = CreateBrandNewFile(RepositoryPath.Combine(korte.Path, "Feladat.doc"));

            //	- Feladat.doc fajlra felveszunk sn\afekete felhasznalonak olvasasi jogosultsag tiltast.
            file.Security.SetPermission(AFekete, true, PermissionType.Open, PermissionValue.Deny);

            //	- Gyoker elembe letrehozunk Szilva mappat
            Folder szilva = CreateBrandNewFolder(RepositoryPath.Combine(Repository.RootPath, "Szilva"));

            //	- Szilva mappara megadunk sn\dincsi usernek olvasasi jogosultsagot.
            szilva.Security.SetPermission(Dincsi, true, PermissionType.Open, PermissionValue.Allow);

            //	- Feladat.doc fajlt atmasoljuk Szilva mappaba.
            file.CopyTo(szilva);
            file = File.LoadNode(RepositoryPath.Combine(szilva.Path, file.Name)) as File;

            //	Elvart eredmeny: Feladat.doc falj jogosultsagai: Gyokerelem + sn\dincsi olvasas + sn\afekte olvasasi tiltas
            //  (alexAllow, molnarGAllow, dincsiAllow, afeketeDeny)

            bool alexAllow = file.Security.GetPermission((IUser)Alex, PermissionType.Open) == PermissionValue.Allow;
            bool molnarGAllow = file.Security.GetPermission((IUser)MolnarG, PermissionType.Open) == PermissionValue.NonDefined;
            bool dincsiAllow = file.Security.GetPermission((IUser)Dincsi, PermissionType.Open) == PermissionValue.Allow;
            bool afeketeDeny = file.Security.GetPermission((IUser)AFekete, PermissionType.Open) == PermissionValue.Deny;


            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, alexOpen); //-- restore original

            Assert.IsTrue(alexAllow, "#1");
            Assert.IsTrue(molnarGAllow, "#2");
            Assert.IsTrue(dincsiAllow, "#3");
            Assert.IsTrue(afeketeDeny, "#4");
        }

        [TestMethod()]
        public void Permission_10()
        {
            //	10.
            //	- Gyoker elemre adunk olvasasi jogosultsagot sn\alex felhasznalonak.
            PermissionValue alexOpen = Repository.Root.Security.GetPermission((IUser)Alex, PermissionType.Open); //-- save original
            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, PermissionValue.Allow);

            //	- Letrehozunk egy "alma" mappat gyoker elem ala.
            Folder alma = CreateBrandNewFolder(RepositoryPath.Combine(Repository.RootPath, "Alma"));

            //	- sn\molnarg usernek adunk read jogosultsagot alma mappara.
            alma.Security.SetPermission(MolnarG, true, PermissionType.Open, PermissionValue.Allow);

            //	- Letrehozunk "alma"-ba egy uj, "korte" mappat.
            Folder korte = CreateBrandNewFolder(RepositoryPath.Combine(alma.Path, "Korte"));

            //	- Kortemappaban letrehozunk egy Feladat.doc fajlt.
            File file = CreateBrandNewFile(RepositoryPath.Combine(korte.Path, "Feladat.doc"));

            //	- Feladat.doc fajlra felveszunk sn\afekete felhasznalonak olvasasi jogosultsag tiltast.
            file.Security.SetPermission(AFekete, true, PermissionType.Open, PermissionValue.Deny);

            //	- Gyoker elembe letrehozunk Szilva mappat
            Folder szilva = CreateBrandNewFolder(RepositoryPath.Combine(Repository.RootPath, "Szilva"));

            //	- Szilva mappara megadunk sn\dincsi usernek olvasasi jogosultsagot.
            szilva.Security.SetPermission(Dincsi, true, PermissionType.Open, PermissionValue.Allow);

            //	- Feladat.doc fajlt atmozgatjuk Szilva mappaba.
            file.MoveTo(szilva);
            file = File.LoadNode(RepositoryPath.Combine(szilva.Path, file.Name)) as File;

            //	Elvart eredmeny: Feladat.doc fajl jogosultsagai: Gyokerelem + sn\dincsi olvasas + sn\afekte olvasasi tiltas
            //  (alexAllow, molnarGAllow, dincsiAllow, afeketeDeny)

            bool alexAllow = file.Security.GetPermission((IUser)Alex, PermissionType.Open) == PermissionValue.Allow;
            bool molnarGNonDef = file.Security.GetPermission((IUser)MolnarG, PermissionType.Open) == PermissionValue.NonDefined;
            bool dincsiAllow = file.Security.GetPermission((IUser)Dincsi, PermissionType.Open) == PermissionValue.Allow;
            bool afeketeDeny = file.Security.GetPermission((IUser)AFekete, PermissionType.Open) == PermissionValue.Deny;

            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, alexOpen); //-- restore original

            Assert.IsTrue(alexAllow, "#1");
            Assert.IsTrue(molnarGNonDef, "#2");
            Assert.IsTrue(dincsiAllow, "#3");
            Assert.IsTrue(afeketeDeny, "#4");
        }

		[TestMethod()]
		public void Permission_11()
		{
			//	11. 
            PermissionValue alexOpen = Repository.Root.Security.GetPermission((IUser)Alex, PermissionType.Open); //-- save original
            
            //	- Grant Open right for sn\alex on the Portal1.Root
            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, PermissionValue.Allow);

			//	- Create an "Alma" folder under the Portal1.Root
			Folder alma = CreateBrandNewFolder(RepositoryPath.Combine(Repository.RootPath, "Alma"));

            //	- Create a "Korte" folder under the "Alma"
			Folder korte = CreateBrandNewFolder(RepositoryPath.Combine(alma.Path, "Korte"));

			//	- Create a new file named "Feladat.doc" in the "Korte" folder
			File file = CreateBrandNewFile(RepositoryPath.Combine(korte.Path, "Feladat.doc"));

			//	- Grant explicit Open right for Alex on the Feladat.doc
            file.Security.SetPermission(Alex, true, PermissionType.Open, PermissionValue.Allow);

			//	- Revoke the Open permission from Alex on Portal1.Root
            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, PermissionValue.NonDefined);

            //  Expected result: the explicit Open permission remained on the Feladat.doc for Alex and MolnarG

            bool rootAlex = Repository.Root.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool fileAlex = file.Security.HasPermission((IUser)Alex, PermissionType.Open);

            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, alexOpen); //-- restore original

			Assert.IsFalse(rootAlex, "#1");
			Assert.IsTrue(fileAlex, "#2");
		}

		/*
		[TestMethod()]
		public void Permission_12()
		{
			//	12.
			//	- Portal Explorerben egy adott node-on a usernek van olvasasi joga
			//	- Adott user rakattint a node-ra (betolti a child node-okat)

			//	Elvart eredmeny: A visszakapott listaban bejonnek az adott node azon alnode-jai, amikre a usernek van lathatasi joga

			Assert.Inconclusive();
		}
		[TestMethod()]
		public void Permission_13()
		{
			//	13.
			//	- Portal Explorerben egy adott node-on a usernek van olvasasi joga, a child node-okon van minor verzio olvasasi joga es 
			//	  alap olvasasi joga, es a mindegyikre van lathatasi joga
			//	- Adott user rakattint a node-ra (betolti a child node-okat)

			//	Elvart eredmeny: A visszakapott listaban bejonnek az adott node alnode-jai, a kiirt verzio szamok az adott node utolso minor
			//	verzioi.

			Assert.Inconclusive();
		}
		[TestMethod()]
		public void Permission_14()
		{
			//	14.
			//	- Portal Explorerben egy adott node-on a usernek van olvasasi joga, a child node-okon nincs minor verzio olvasasi joga es van 
			//	  alap olvasasi joga, es a mindegyikre van lathatasi joga
			//	- Adott user rakattint a node-ra (betolti a child node-okat)

			//	Elvart eredmeny: A visszakapott listaban bejonnek az adott node alnode-jai, a kiirt verzio szamok az adott node utolso major 
			//	verzioi.

			Assert.Inconclusive();
		}
		[TestMethod()]
		public void Permission_15()
		{
			//	15.
			//	- Az egyik forum egyik topicjaba uj bejegyzest akar tenni sn\molnarg user, de a topicon nincs uj hozzaszolasa felvetele joga
			//	Elvart eredmeny: A rendszer nem engedi uj hozzaszolas felvetelet-> dob egy not sufficinet rights exception-t.

			Assert.Inconclusive();
		}
		[TestMethod()]
		public void Permission_16()
		{
			//	16.
			//	- Az egyik forum egyik topicjanak egyik bejegyzesere sn\molnarg-nek nincs „moderate” joga, mig sn\dinicsi-nek van. 

			//	Elvart eredmeny: sn\molnarg nem, mig sn\dincsi tud moderalni.


			//	Megjegyzes: a tesztesetek nem terjednek ki az "orokles" megszakitasara. 
			//	Nem terjednek ki tovabba a jogosultsagok alapjan a helyes mukodesre, kizarolag a beallitasok helyessegere.

			Assert.Inconclusive();
		}
		*/

        [TestMethod()]
        public void Permission_17_BreakInheriance()
        {
            //	17. Orokles megszakitasa
            // Root
            //  |
            //  --Alma
            //     |
            //     --Korte
            //         |
            //         ---Feladat.doc

            //	- Gyoker elemre adunk olvasasi jogosultsagot sn\alex felhasznalonak.
            PermissionValue alexOpen = Repository.Root.Security.GetPermission((IUser)Alex, PermissionType.Open); //-- save original
            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, PermissionValue.Allow);

            //	- Letrehozunk egy "alma" mappat gyoker elem ala.
            Folder alma = CreateBrandNewFolder(RepositoryPath.Combine(Repository.RootPath, "Alma"));

            //	- Letrehozunk "alma"-ba egy uj, "korte" mappat.
            Folder korte = CreateBrandNewFolder(RepositoryPath.Combine(alma.Path, "Korte"));

            //	- Kortemappaban letrehozunk egy Feladat.doc fajlt.
            File file = CreateBrandNewFile(RepositoryPath.Combine(korte.Path, "Feladat.doc"));

            //  - Megszakitjuk a Root-rol jovo jogosultsagok orokleset a Korte mappan
            //korte.Security.BreakInheritance(Repository.Root.Id);
            korte.Security.BreakInheritance();
            
            //	Elvart eredmeny: Alexnak továbbra is van Open joga minden nodera
            bool alexRoot = Repository.Root.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool alexAlma = alma.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool alexKorte = korte.Security.HasPermission((IUser)Alex, PermissionType.Open);
            bool alexFile = file.Security.HasPermission((IUser)Alex, PermissionType.Open);

            Assert.IsTrue(alexRoot, "#1");
            Assert.IsTrue(alexAlma, "#2");
            Assert.IsTrue(alexKorte, "#3");
            Assert.IsTrue(alexFile, "#4");


            // Root-rol elvesszuk Alex olvasasi jogosultsagat
            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, PermissionValue.NonDefined);

            //	Elvart eredmeny: Alexnak Root-on és a tole oroklo Alman nincs, de a sajat jogokkal biro
            //  Korten és Feladat.doc-on van van Open joga
            alexRoot = Repository.Root.Security.HasPermission((IUser)Alex, PermissionType.Open);
            alexAlma = alma.Security.HasPermission((IUser)Alex, PermissionType.Open);
            alexKorte = korte.Security.HasPermission((IUser)Alex, PermissionType.Open);
            alexFile = file.Security.HasPermission((IUser)Alex, PermissionType.Open);

            Assert.IsFalse(alexRoot, "#5");
            Assert.IsFalse(alexAlma, "#6");
            Assert.IsTrue(alexKorte, "#7");
            Assert.IsTrue(alexFile, "#8");

            Repository.Root.Security.SetPermission(Alex, true, PermissionType.Open, alexOpen); //-- restore original
        }

        [TestMethod()]
        public void Permission_18_SeePermission()
        {
            //	18. test of 'See' permission

            // create a brand new file
            string feladatPath = RepositoryPath.Combine(Repository.Root.Path, "Feladat.doc");
            File file = CreateBrandNewFile(feladatPath);

            int idOfFile = file.Id;

            // Deny Open, allow only See
            file.Security.SetPermission(MolnarG, true, PermissionType.Open, PermissionValue.Deny);
            file.Security.SetPermission(MolnarG, true, PermissionType.OpenMinor, PermissionValue.Deny);
            file.Security.SetPermission(MolnarG, true, PermissionType.See, PermissionValue.Allow);
            // Load file
            AccessProvider.Current.SetCurrentUser(MolnarG);
            var fileNode = Node.Load<File> (idOfFile);
            AccessProvider.Current.SetCurrentUser(User.Administrator);

            bool expectedExceptionThrown = false;
            try
            {
                // should throw NotSupportedException
                var x = fileNode.Binary;
            }
            catch (InvalidOperationException)
            {
                expectedExceptionThrown = true;
            }

			Assert.IsTrue(expectedExceptionThrown, "#### Storage2 See only node view is not implemented");

            // default: allow all
            file.Security.SetPermission(MolnarG, true, PermissionType.Open, PermissionValue.Allow);
            file.Security.SetPermission(MolnarG, true, PermissionType.OpenMinor, PermissionValue.Allow);
            file.Security.SetPermission(MolnarG, true, PermissionType.See, PermissionValue.Allow);
        }

        [TestMethod()]
        public void Permission_19_PermissionsViaGroupMembership()
        {
            //	19. test of user rights via group membership

            // create a brand new file
            File file = CreateBrandNewFile(RepositoryPath.Combine(Repository.Root.Path, "GroupTest.doc"));
            int idOfFile = file.Id;

            // create a group
            string testGroupAName = "TestGroupA";
            string testGroupAPath = RepositoryPath.Combine(Repository.Root.Path, testGroupAName);
            if (Node.Exists(testGroupAPath))
                Node.ForceDelete(testGroupAPath);

            Group testGroupA = new Group(Repository.Root);
            testGroupA.Name = testGroupAName;
            testGroupA.AddMember(Dincsi);
            testGroupA.Save();

            // Deny Open for the group
            file.Security.SetPermission(testGroupA, true, PermissionType.Open, PermissionValue.Deny);

            // Sould be denied for the user who is a member of the group
            Assert.IsTrue(file.Security.GetPermission((IUser)Dincsi, PermissionType.Open) == PermissionValue.Deny);

            // Allow Open for the group
            file.Security.SetPermission(testGroupA, true, PermissionType.Open, PermissionValue.Allow);

            // Sould be allowed for the user who is a member of the group
            Assert.IsTrue(file.Security.GetPermission((IUser)Dincsi, PermissionType.Open) == PermissionValue.Allow);
        }

        [TestMethod()]
        public void Permission_20_PermissionsViaIndirectGroupMembership()
        {
            //	20. 

            // create a brand new file
            File file = CreateBrandNewFile(RepositoryPath.Combine(Repository.Root.Path, "GroupTest.doc"));
            int idOfFile = file.Id;

            // create a group
            string testGroupAName = "TestGroupA";
            string testGroupAPath = RepositoryPath.Combine(Repository.Root.Path, testGroupAName);
            if (Node.Exists(testGroupAPath))
                Node.ForceDelete(testGroupAPath);

            Group testGroupA = new Group(Repository.Root);
            testGroupA.Name = testGroupAName;
			testGroupA.AddMember(Dincsi);
            testGroupA.Save();


            string testGroupBName = "TestGroupB";
            string testGroupBPath = RepositoryPath.Combine(Repository.Root.Path, testGroupBName);
            if (Node.Exists(testGroupBPath))
                Node.ForceDelete(testGroupBPath);
            
            Group testGroupB = new Group(Repository.Root);
            testGroupB.Name = testGroupBName;
			testGroupB.AddMember(User.Administrator);
			testGroupB.AddMember(testGroupA);
            testGroupB.Save();

            // Deny Open for the group
            file.Security.SetPermission(testGroupA, true, PermissionType.Open, PermissionValue.Deny);

            // Sould be denied for the user who is a member of the group
            Assert.IsTrue(file.Security.GetPermission((IUser)Dincsi, PermissionType.Open) == PermissionValue.Deny);

            // Allow Open for the group
            file.Security.SetPermission(testGroupA, true, PermissionType.Open, PermissionValue.Allow);

            // Sould be allowed for the user who is a member of the group
            Assert.IsTrue(file.Security.GetPermission((IUser)Dincsi, PermissionType.Open) == PermissionValue.Allow);
        }

        [TestMethod()]
        public void Permission_21_EveryoneGroupHack()
        {
            // 21: the Everyone group is a special group, you don't put user into it but every user should
			//     be a member of it by default. There's no Repository representation, the system should "just know" it.

            // create a brand new file
            File file = CreateBrandNewFile(RepositoryPath.Combine(Repository.Root.Path, "GroupTest.doc"));
            int idOfFile = file.Id;

            PermissionValue originalValue = file.Security.GetPermission((IUser)MolnarG, PermissionType.OpenMinor);

            if (originalValue == PermissionValue.Allow)
            {
                Assert.Inconclusive("Cannot run this test: the OpenMinor permission is already granted.");
            }

            file.Security.SetPermission(Group.Everyone, true, PermissionType.OpenMinor, PermissionValue.Allow);

            PermissionValue viaEveryoneValue = file.Security.GetPermission((IUser)MolnarG, PermissionType.OpenMinor);

            file.ForceDelete();

            Assert.IsTrue(viaEveryoneValue == PermissionValue.Allow, "The test user hasn't got OpenMinor permission eventough the permission was granted to Everyone.");

        }


//        [TestMethod]
//        public void Security_GetAcl()
//        {
//            // create structure: /Root/_PermissionEditingTest/AA/BB/CC
//            var path0 = RepositoryPath.Combine(Repository.Root.Path, "_PermissionEditingTest");
//            AddPathToDelete(path0);
//            var path1 = RepositoryPath.Combine(path0, "AA");
//            var path2 = RepositoryPath.Combine(path1, "BB");
//            var path3 = RepositoryPath.Combine(path2, "CC");
//            var filePath = RepositoryPath.Combine(path3, "Doc1.doc");
//            var folder0 = CreateBrandNewFolder(path0);
//            var folder1 = CreateBrandNewFolder(path1);
//            var folder2 = CreateBrandNewFolder(path2);
//            var folder3 = CreateBrandNewFolder(path3);
//            var file = CreateBrandNewFile(filePath);

//            folder1.Security.SetPermission(Alex, true, PermissionType.OpenMinor, PermissionValue.Allow);
//            folder1.Security.SetPermission(Alex, true, PermissionType.Save, PermissionValue.Allow);
//            folder1.Security.SetPermission(Alex, true, PermissionType.AddNew, PermissionValue.Allow);
//            folder1.Security.SetPermission(Alex, true, PermissionType.Delete, PermissionValue.Allow);

//            folder3.Security.SetPermission(MolnarG, true, PermissionType.Open, PermissionValue.Allow);
//            folder3.Security.SetPermission(MolnarG, true, PermissionType.OpenMinor, PermissionValue.Allow);
//            folder3.Security.SetPermission(MolnarG, true, PermissionType.Save, PermissionValue.Allow);

//            //var edited = PermissionEvaluatorTests.GetEntriesToString(file.Security.GetEffectiveEntries());

//            var acl = file.Security.GetAcl();

//            Assert.IsTrue(acl.NodeId == file.Id, "#1");
//            Assert.IsTrue(acl.Path == file.Path.ToLower(), "#2");
//            Assert.IsTrue(acl.Inherits == true, "#3");
//            Assert.IsTrue(acl.Owner.NodeId == User.Administrator.Id, "#4");
//            Assert.IsTrue(acl.Owner.Kind == SnIdentityKind.User, "#5");
//            Assert.IsTrue(acl.Entries.Count() == 6, "#6");

//            var log = AclEntriesToString(acl);
//            var exp = String.Format(@"  <Entries>
//    <Entry identityId='{0}' identityKind='User' propagates='True'/>
//      <Perm name='See' allow='True' allowEnabled='False' allowFrom='/root/_permissioneditingtest/aa/bb/cc' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Open' allow='True' allowEnabled='False' allowFrom='/root/_permissioneditingtest/aa/bb/cc' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='OpenMinor' allow='True' allowEnabled='False' allowFrom='/root/_permissioneditingtest/aa/bb/cc' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Save' allow='True' allowEnabled='False' allowFrom='/root/_permissioneditingtest/aa/bb/cc' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Publish' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='ForceCheckin' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='AddNew' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Approve' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Delete' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='RecallOldVersion' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='DeleteOldVersion' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='SeePermissions' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='SetPermissions' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='RunApplication' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//    </Entry>
//    <Entry identityId='{1}' identityKind='User' propagates='True'/>
//      <Perm name='See' allow='True' allowEnabled='False' allowFrom='/root/_permissioneditingtest/aa' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Open' allow='True' allowEnabled='False' allowFrom='/root/_permissioneditingtest/aa' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='OpenMinor' allow='True' allowEnabled='False' allowFrom='/root/_permissioneditingtest/aa' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Save' allow='True' allowEnabled='False' allowFrom='/root/_permissioneditingtest/aa' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Publish' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='ForceCheckin' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='AddNew' allow='True' allowEnabled='False' allowFrom='/root/_permissioneditingtest/aa' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Approve' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Delete' allow='True' allowEnabled='False' allowFrom='/root/_permissioneditingtest/aa' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='RecallOldVersion' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='DeleteOldVersion' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='SeePermissions' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='SetPermissions' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='RunApplication' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//    </Entry>
//    <Entry identityId='1' identityKind='User' propagates='True'/>
//      <Perm name='See' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Open' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='OpenMinor' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Save' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Publish' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='ForceCheckin' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='AddNew' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Approve' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Delete' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='RecallOldVersion' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='DeleteOldVersion' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='SeePermissions' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='SetPermissions' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='RunApplication' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//    </Entry>
//    <Entry identityId='7' identityKind='Group' propagates='True'/>
//      <Perm name='See' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Open' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='OpenMinor' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Save' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Publish' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='ForceCheckin' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='AddNew' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Approve' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Delete' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='RecallOldVersion' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='DeleteOldVersion' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='SeePermissions' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='SetPermissions' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='RunApplication' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//    </Entry>
//    <Entry identityId='6' identityKind='User' propagates='True'/>
//      <Perm name='See' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Open' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='OpenMinor' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Save' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Publish' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='ForceCheckin' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='AddNew' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Approve' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Delete' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='RecallOldVersion' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='DeleteOldVersion' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='SeePermissions' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='SetPermissions' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='RunApplication' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//    </Entry>
//    <Entry identityId='8' identityKind='Group' propagates='True'/>
//      <Perm name='See' allow='True' allowEnabled='False' allowFrom='/root' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Open' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='OpenMinor' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Save' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Publish' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='ForceCheckin' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='AddNew' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Approve' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='Delete' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='RecallOldVersion' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='DeleteOldVersion' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='SeePermissions' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='SetPermissions' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//      <Perm name='RunApplication' allow='False' allowEnabled='True' allowFrom='' deny='False' denyEnabled='True' denyFrom=''/>
//    </Entry>
//  </Entries>
//", MolnarG.Id, Alex.Id);

//            Assert.IsTrue(log.Replace(" ", "").Replace("\r", "").Replace("\n", "") == exp.Replace(" ", "").Replace("\r", "").Replace("\n", ""));

//        }
//        [TestMethod]
//        public void Security_AclEditor()
//        {
//            // create structure: /Root/_PermissionEditingTest/AA/BB/CC
//            var path0 = RepositoryPath.Combine(Repository.Root.Path, "_PermissionEditingTest");
//            AddPathToDelete(path0);
//            var path1 = RepositoryPath.Combine(path0, "AA");
//            var path2 = RepositoryPath.Combine(path1, "BB");
//            var path3 = RepositoryPath.Combine(path2, "CC");
//            var filePath = RepositoryPath.Combine(path3, "Doc1.doc");
//            var folder0 = CreateBrandNewFolder(path0);
//            var folder1 = CreateBrandNewFolder(path1);
//            var folder2 = CreateBrandNewFolder(path2);
//            var folder3 = CreateBrandNewFolder(path3);
//            var file = CreateBrandNewFile(filePath);

//            //DefinedOn=2, Principal=1, IsInheritable=true, Values=++++++++++++++
//            //DefinedOn=2, Principal=7, IsInheritable=true, Values=++++++++++++++
//            //DefinedOn=2, Principal=6, IsInheritable=true, Values=____________++
//            //DefinedOn=2, Principal=8, IsInheritable=true, Values=_____________+
//            var log1 = PermissionEvaluatorTests.GetEntriesToString(file.Security.GetAllEntries());
//            var expectedLog1 = String.Format(
//                @"DefinedOn=2, Principal={0}, Propagates=true, Values=++++++++++++++
//                DefinedOn=2, Principal={2}, Propagates=true, Values=++++++++++++++
//                DefinedOn=2, Principal={1}, Propagates=true, Values=____________++
//                DefinedOn=2, Principal={3}, Propagates=true, Values=_____________+",
//                User.Administrator.Id, User.Visitor.Id, Group.Administrators.Id, Group.Everyone.Id);

//            folder1.Security.GetAclEditor().SetPermission(Alex, true, PermissionType.OpenMinor, PermissionValue.Allow).Apply();

//            folder1.Security.GetAclEditor()
//                .SetPermission(Alex, true, PermissionType.OpenMinor, PermissionValue.Allow)
//                .SetPermission(Alex, true, PermissionType.Save, PermissionValue.Allow)
//                .SetPermission(Alex, true, PermissionType.AddNew, PermissionValue.Allow)
//                .SetPermission(Alex, true, PermissionType.Delete, PermissionValue.Allow)
//                .Merge
//                (
//                    folder3.Security.GetAclEditor()
//                    .SetPermission(MolnarG, true, PermissionType.Open, PermissionValue.Allow)
//                    .SetPermission(MolnarG, true, PermissionType.OpenMinor, PermissionValue.Allow)
//                    .SetPermission(MolnarG, true, PermissionType.Save, PermissionValue.Allow)
//                ).Apply();

//            //============================

//            var log2 = PermissionEvaluatorTests.GetEntriesToString(file.Security.GetAllEntries());
//            var expectedLog2 = String.Format(
//                @"DefinedOn={7}, Principal={5}, Propagates=true, Values=__________+++_
//                DefinedOn={6}, Principal={4}, Propagates=true, Values=_____+_+__++__
//                DefinedOn=2, Principal={0}, Propagates=true, Values=++++++++++++++
//                DefinedOn=2, Principal={2}, Propagates=true, Values=++++++++++++++
//                DefinedOn=2, Principal={1}, Propagates=true, Values=____________++
//                DefinedOn=2, Principal={3}, Propagates=true, Values=_____________+",
//                User.Administrator.Id, User.Visitor.Id, Group.Administrators.Id, Group.Everyone.Id, Alex.Id, MolnarG.Id, folder1.Id, folder3.Id);

//            Assert.IsTrue(log1.Replace(" ", "") == expectedLog1.Replace(" ", ""), "#1");
//            Assert.IsTrue(log2.Replace(" ", "") == expectedLog2.Replace(" ", ""), "#2");
//        }

        [TestMethod]
        public void Security_SystemAccount()
        {
            while (User.Current.Id == -1)
                AccessProvider.RestoreOriginalUser();

            try
            {
                var origUserId = User.Current.Id; // == 1
                Assert.IsTrue(origUserId != -1, "Initial userId is -1");

                //----

                AccessProvider.ChangeToSystemAccount();
                Assert.IsTrue(User.Current.Id == -1, "#1");
                AccessProvider.RestoreOriginalUser();
                Assert.IsTrue(User.Current.Id == origUserId, "#2");

                //----

                var expectedIds = new int[] { -1, -1, -1, -1, origUserId };
                var ids = new int[expectedIds.Length];
                for (int i = 0; i < expectedIds.Length; i++)
                    AccessProvider.ChangeToSystemAccount();
                for (int i = 0; i < expectedIds.Length; i++)
                {
                    AccessProvider.RestoreOriginalUser();
                    ids[i] = User.Current.Id;
                }
                var s0 = String.Join(", ", (from id in expectedIds select id.ToString()).ToArray());
                var s1 = String.Join(", ", (from id in ids select id.ToString()).ToArray());
                Assert.IsTrue(s0 == s1, "#3");
            }
            finally
            {
                while (User.Current.Id == -1)
                    AccessProvider.RestoreOriginalUser();
            }
        }
        [TestMethod]
        public void Security_SystemAccountAndOriginalUser()
        {
            while (User.Current.Id == -1)
                AccessProvider.RestoreOriginalUser();

            try
            {
                var origUserId = User.Current.Id; // == 1
                Assert.IsTrue(origUserId != -1, "Initial userId is -1");
                {
                    AccessProvider.ChangeToSystemAccount();
                    Assert.IsTrue(User.Current.Id == -1, "#1");
                    Assert.IsTrue(AccessProvider.Current.GetOriginalUser().Id == origUserId, "#2");
                    {
                        AccessProvider.ChangeToSystemAccount();
                        Assert.IsTrue(User.Current.Id == -1, "#3");
                        Assert.IsTrue(AccessProvider.Current.GetOriginalUser().Id == origUserId, "#4");
                        AccessProvider.RestoreOriginalUser();
                        Assert.IsTrue(User.Current.Id == -1, "#5");
                        Assert.IsTrue(AccessProvider.Current.GetOriginalUser().Id == origUserId, "#6");
                    }
                    AccessProvider.RestoreOriginalUser();
                    Assert.IsTrue(User.Current.Id == origUserId, "#7");
                }
            }
            finally
            {
                while (User.Current.Id == -1)
                    AccessProvider.RestoreOriginalUser();
            }
        }

        //---------------------------------------------------------------------------------------------

        [TestMethod]
        public void Security_Bug737_Unbreak()
        {
            var rootFolderPath = "/Root/Security_Bug737_Unbreak";
            var folderPath = rootFolderPath + "/Folder1";
            var filePath = "/Root/Security_Bug737_Unbreak/MyDocument";
            var file = CreateBrandNewFile(filePath);

            var user = AFekete;

            var rootFolder = Node.Load<Folder>(rootFolderPath);
            if (rootFolder.Security.HasPermission((IUser)user, PermissionType.ForceCheckin))
                Assert.Inconclusive();

            rootFolder.Security.SetPermission(user, true, PermissionType.ForceCheckin, PermissionValue.Allow);

            Assert.IsTrue(file.Security.HasPermission((IUser)user, PermissionType.ForceCheckin), "#1");

            file.Security.BreakInheritance();

            Assert.IsTrue(HasExplicitEntry(file), "#2: File does not have any explicit entries.");
        }
        private bool HasExplicitEntry(Node node)
        {
            return node.Security.GetExplicitEntries().Length > 0;
        }
        //private bool ContainsExplicitEntry(Node node, PermissionType perm)
        //{
        //    foreach (var entry in node.Security.GetExplicitEntries())
        //    {
        //    }
        //}

        [TestMethod]
        public void Security_Bug234_BreakInheritanceAndCopy()
        {
            var rootFolderPath = "/Root/Security_Bug234_BreakInheritanceAndCopy";
            var folder1Path = rootFolderPath + "/Folder1";
            var folder2Path = rootFolderPath + "/Folder2";
            var file1Path = folder1Path + "/MyDocument";
            var file2Path = folder2Path + "/MyDocument";

            var folder1 = CreateBrandNewFolder(folder1Path);
            var folder2 = CreateBrandNewFolder(folder2Path);
            var file1 = CreateBrandNewFile(file1Path);

            folder1.Security.SetPermission(AFekete, true, PermissionType.OpenMinor, PermissionValue.Allow);
            folder2.Security.SetPermission(Dincsi, true, PermissionType.OpenMinor, PermissionValue.Allow);
            file1.Security.SetPermission(Alex, true, PermissionType.OpenMinor, PermissionValue.Allow);

            file1.Security.BreakInheritance();

            file1.CopyTo(folder2);

            Node file2 = null;
            using (new SystemAccount())
            {
                file2 = Node.LoadNode(file2Path);
                Assert.IsTrue(file2.Security.HasPermission((IUser)User.Administrator, PermissionType.OpenMinor), "#1");
                Assert.IsFalse(file2.Security.HasPermission((IUser)User.Visitor, PermissionType.OpenMinor), "#2");
                Assert.IsTrue(file2.Security.HasPermission((IUser)User.Visitor, PermissionType.Open), "#3");

                Assert.IsTrue(file2.Security.HasPermission((IUser)AFekete, PermissionType.OpenMinor), "#4");
                Assert.IsTrue(file2.Security.HasPermission((IUser)Dincsi, PermissionType.OpenMinor), "#5");
                Assert.IsTrue(file2.Security.HasPermission((IUser)Alex, PermissionType.OpenMinor), "#6");
            }
        }

        //---------------------------------------------------------------------------------------------

		private Folder CreateBrandNewFolder(string path)
		{
			if (Node.Exists(path))
                Node.ForceDelete(path);
			return LoadOrCreateFolder(path);
		}
		private Folder LoadOrCreateFolder(string path)
		{
			Folder folder = (Folder)Node.LoadNode(path);
			if (folder != null)
				return folder;

			string parentPath = RepositoryPath.GetParentPath(path);
			Folder parentFolder = (Folder)Node.LoadNode(parentPath);
			if (parentFolder == null)
				parentFolder = LoadOrCreateFolder(parentPath);

			folder = new Folder(parentFolder);
			folder.Name = RepositoryPath.GetFileName(path);
			folder.Save();
			AddPathToDelete(path);

			return folder;
		}

		private File CreateBrandNewFile(string path)
		{
            if (Node.Exists(path))
            {
                Node.ForceDelete(path);
            }
			return LoadOrCreateFile(path);
		}
		private File LoadOrCreateFile(string path)
		{
            AccessProvider.ChangeToSystemAccount();
			File file = File.LoadNode(path) as File;
            AccessProvider.RestoreOriginalUser();
			if (file != null)
				return file;

			string parentPath = RepositoryPath.GetParentPath(path);
			Folder parentFolder = (Folder)Node.LoadNode(parentPath);
			if (parentFolder == null)
				parentFolder = LoadOrCreateFolder(parentPath);

			file = new File(parentFolder);
			file.Name = RepositoryPath.GetFileName(path);
			file.Binary = TestTools.CreateTestBinary();
			file.Save();
			AddPathToDelete(path);

			return file;
		}
		
		private string SecurityEntriesToString(Node node)
		{
            return SecurityEntriesToString(node.Security.GetAllEntries());
		}
        private string SecurityEntriesToString(IEnumerable<SecurityEntry> entries)
        {
			StringBuilder sb = new StringBuilder();
			foreach (SecurityEntry entry in entries)
			{
				sb.Append("<Entry definedOn='").Append(entry.DefinedOnNodeId);
				sb.Append("' principalId='").Append(entry.PrincipalId).Append("'>\r\n");
				for (int i = 0; i < ActiveSchema.PermissionTypes.Count; i++)
					sb.Append("\t<").Append(ActiveSchema.PermissionTypes[i].Name).Append(" value='").Append(entry.PermissionValues[i]).Append("' />\r\n");
				sb.Append("</Entry>\r\n");
			}
			return sb.ToString();
        }

        private string AclEntriesToString(SnAccessControlList acl)
        {
            var sb = new StringBuilder();

            sb.AppendLine("  <Entries>");
            foreach (var ace in acl.Entries)
            {
                sb.Append("    <Entry identityId='").Append(ace.Identity.NodeId).Append("'");
                sb.Append(" identityKind='").Append(ace.Identity.Kind).Append("'");
                sb.Append(" propagates='").Append(ace.Propagates).Append("'");
                sb.AppendLine("/>");
                foreach (var perm in ace.Permissions)
                {
                    sb.Append("      <Perm name='").Append(perm.Name).Append("'");
                    sb.Append(" allow='").Append(perm.Allow).Append("'");
                    sb.Append(" allowEnabled='").Append(perm.AllowEnabled).Append("'");
                    sb.Append(" allowFrom='").Append(perm.AllowFrom).Append("'");
                    sb.Append(" deny='").Append(perm.Deny).Append("'");
                    sb.Append(" denyEnabled='").Append(perm.DenyEnabled).Append("'");
                    sb.Append(" denyFrom='").Append(perm.DenyFrom).Append("'");
                    sb.AppendLine("/>");
                }
                sb.AppendLine("    </Entry>");
            }
            sb.AppendLine("  </Entries>");

            return sb.ToString();
        }
	}
}