using System;
using Assets.UnityAOP.Attributes;

namespace Assets.Model {
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
}