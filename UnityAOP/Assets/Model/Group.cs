using System;
using Assets.UnityAOP.Attributes;
using Assets.UnityAOP.Observable;

namespace Assets.Model {
    [Observable]
    public class Group {
        public Int32 Id { get; set; }
        public Int32 NumMembers { get; set; }
        public Int32 NumReadyMembers { get; set; }
        public ObservableList<User> Members { get; private set; }

        public Group(int id) {
            Id = id;
            NumMembers = 0;
            NumReadyMembers = 0;
            Members = new ObservableList<User>();
        }

        public void AddMember(User member) {
            NumMembers++;
        }

        public void RemoveMember(User member) {
            NumMembers--;
        }
    }
}