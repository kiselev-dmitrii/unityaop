using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Model;
using Assets.UnityAOP.Observable.ChainedObservers;
using Assets.UnityAOP.Utils;
using NUnit.Framework;

namespace Assets.UnityAOP.Editor.UnitTests {
    public class SyncListObserverTest {
        public class GroupMemberItem {
            public User Member { get; private set; }
            public String Name { get; private set; }
            public Int32 Index { get; private set; }

            public GroupMemberItem(int index, User user) {
                Member = user;
                Name = user.Name;
                Index = index;
            }

            public override string ToString() {
                return Name;
            }
        }

        [Test]
        public void ObserveBrokenField() {
            var player = new User(1, "Player");

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

            var dstList = new List<GroupMemberItem>();
            var observer = player.SyncList(x => x.Group.Members, dstList,
                (index, item) => new GroupMemberItem(index, item));

            player.Group = group1;
            Assert.IsTrue(dstList.Count == player.Group.Members.Count);
            for (int i = 0; i < dstList.Count; ++i) {
                Assert.IsTrue(dstList[i].Name == player.Group.Members[i].Name);
            }

            player.Group = group2;
            Assert.IsTrue(dstList.Count == player.Group.Members.Count);
            for (int i = 0; i < dstList.Count; ++i) {
                Assert.IsTrue(dstList[i].Name == player.Group.Members[i].Name);
            }

            player.Group = group3;
            Assert.IsTrue(dstList.Count == player.Group.Members.Count);
            for (int i = 0; i < dstList.Count; ++i) {
                Assert.IsTrue(dstList[i].Name == player.Group.Members[i].Name);
            }

            player.Group = null;
            Assert.IsTrue(dstList.Count == 0);

            player.Group = new Group(3);
            Assert.IsTrue(dstList.Count == 0);

            player.Group.Members.Add(new User(9, "Elvis Presley"));
            Assert.IsTrue(dstList.Count == 1);
        }
    }
}
