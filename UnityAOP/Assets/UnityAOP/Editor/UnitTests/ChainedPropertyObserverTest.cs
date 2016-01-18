using System;
using Assets.Model;
using Assets.UnityAOP.Observable.ChainedObservers;
using NUnit.Framework;

namespace Assets.UnityAOP.Editor.UnitTests {
    public class ChainedPropertyObserverTest {
        public Application Application;

        [Test]
        public void ObserveBrokenField() {
            Application = new Application();

            int numMembers = 0;

            ChainedPropertyObserver<int> observer = null;
            observer = Application.Observe(x => x.Player.Group.NumMembers, () => {
                numMembers = observer.GetValue();
            });
            Assert.IsTrue(numMembers == 0);

            Application.Player.Group = new Group(10);
            Assert.IsTrue(numMembers == 0);

            Application.Player.Group.NumMembers++;
            Assert.IsTrue(numMembers == Application.Player.Group.NumMembers);

            Application.Player.Group = null;
            Assert.IsTrue(numMembers == 0);

            var group = new Group(11);
            group.NumMembers = 100500;
            Assert.IsTrue(numMembers == 0);

            Application.Player.Group = group;
            Assert.IsTrue(numMembers == group.NumMembers);

            Application.Player = null;
            Assert.IsTrue(numMembers == 0);
        }

        [Test]
        public void SetValueField() {
            Application = new Application();

            ChainedPropertyObserver<int> idObserver = null;
            idObserver = Application.Observe(x => x.Player.Id, () => {});

            Application.Player.Id = 10;
            Assert.IsTrue(idObserver.GetValue() == 10);

            idObserver.SetValue(12);
            Assert.IsTrue(idObserver.GetValue() == 12);
        }

        [Test]
        public void GetObservableField() {
            Application = new Application();

            ChainedPropertyObserver<User> userObserver = null;
            userObserver = Application.Observe(x => x.Player, () => { });

            Assert.IsTrue(userObserver.GetValue() == Application.Player);
        }

        [Test]
        public void ObserveCollectionItem() {
            var group = new Group(1);

            String secondMemberName = null;

            ChainedPropertyObserver<String> observer = null;
            observer = group.Observe(x => x.Members[2].Name, () => secondMemberName = observer.GetValue());
            Assert.IsTrue(secondMemberName == null);

            group.Members.Add(new User(1, "First user", ""));
            Assert.IsTrue(secondMemberName == null);

            group.Members.Add(new User(2, "Second user", ""));
            Assert.IsTrue(secondMemberName == null);
            
            group.Members.Add(new User(3, "Third user", ""));
            Assert.IsTrue(secondMemberName == "Third user");

            group.Members.Insert(0, new User(0, "Zero user", ""));
            Assert.IsTrue(secondMemberName == "Second user");

            group.Members.RemoveAt(0);
            Assert.IsTrue(secondMemberName == "Third user");

            group.Members.RemoveAt(0);
            Assert.IsTrue(secondMemberName == null);

            group.Members.RemoveAt(0);
            Assert.IsTrue(secondMemberName == null);

            group.Members.RemoveAt(0);
            Assert.IsTrue(secondMemberName == null);
        }

        [Test]
        public void ReplaceCollection() {
            Application = new Application();

            var group1 = new Group(1, new User[] {
                new User(1, "John Nash"),
                new User(2, "Jack London"),
                new User(3, "Johnny Cash"), 
            });

            var group2 = new Group(2, new User[] {
                new User(4, "Kate Ostin"),
                new User(5, "Evangeline Lili"),
                new User(6, "Cassie Cage"), 
            });

            var group3 = new Group(2, new User[] {
                new User(7, "Lisa Simpson"),
                new User(8, "Turanga Lila") 
            });

            String secondMemberName = null;
            ChainedPropertyObserver<String> observer = null;
            observer = Application.Observe(x => x.Player.Group.Members[2].Name, () => secondMemberName = observer.GetValue());

            Assert.IsTrue(secondMemberName == null);

            Application.Player.Group = group1;
            Assert.IsTrue(secondMemberName == "Johnny Cash");

            Application.Player.Group = group2;
            Assert.IsTrue(secondMemberName == "Cassie Cage");

            Application.Player.Group = group3;
            Assert.IsTrue(secondMemberName == null);
        }
    }
}
