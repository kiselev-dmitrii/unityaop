using Assets.Model;
using Assets.UnityAOP.Observable.ChainedObservers;
using NUnit.Framework;

namespace Assets.UnityAOP.Editor.UnitTests {
    public class ChainedPropertyObserverTest {
        public Application Application;


        public ChainedPropertyObserverTest() {
            Application = new Application();
        }

        [Test]
        public void ObserveBrokenField() {
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
    }
}
