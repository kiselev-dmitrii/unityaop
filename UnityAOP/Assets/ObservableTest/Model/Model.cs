using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.UnityAOP.Attributes;
using Assets.UnityAOP.Observable;

namespace Assets.ObservableTest.Model {
    [Observable]
    public class Application {
        public User Player { get; set; }

        public Application() {
            Player = new User(1, "Player", "http://vk.com/id0/avatar.jpg");
        }
    }

    [Observable]
    public class User {
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public String Avatar { get; set; }
        public Group Group { get; set; }

        public User(int id, string name, string avatar) {
            Id = id;
            Name = name;
            Avatar = avatar;
            Group = new Group(id);
        }
    }

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
