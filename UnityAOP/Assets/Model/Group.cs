using System;
using Assets.UnityAOP.Observable.Core;

namespace Assets.Model {
    [Observable]
    public class Group {
        public Int32 Id { get; set; }
        public Int32 NumMembers { get; set; }
        public Int32 NumReadyMembers { get; set; }
        public ObservableList<User> Members { get; set; }

        public Group(int id) {
            Id = id;
            NumMembers = 0;
            NumReadyMembers = 0;
            Members = new ObservableList<User>();
        }

        public Group(int id, User[] users) {
            Id = id;
            NumMembers = users.Length;
            NumReadyMembers = 0;
            Members = new ObservableList<User>();
            Members.AddRange(users);
        }

        public void AddMember(User member) {
            Members.Add(member);
            NumMembers++;
        }

        public void RemoveMember(User member) {
            Members.Remove(member);
            NumMembers--;
        }

        public void AddRandomMember() {
            Members.Add(new User(NumMembers+1, ""));
            NumMembers++;
        }
    }
}