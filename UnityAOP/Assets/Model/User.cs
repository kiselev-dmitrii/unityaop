using System;
using Assets.UnityAOP.Observable;
using Assets.UnityAOP.Observable.Core;

namespace Assets.Model {
    [Observable]
    public class User {
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public String Avatar { get; set; }
        public Group Group { get; set; }

        public User(int id, String name) {
            Id = id;
            Name = name;
            Avatar = null;
        }

        private static String[] names = new[] {
            "Dick", "John", "Bill", "Steave", "Kate"
        };
        public void RandomName() {
            Name = names[UnityEngine.Random.Range(0, names.Length - 1)];
        }
    }
}