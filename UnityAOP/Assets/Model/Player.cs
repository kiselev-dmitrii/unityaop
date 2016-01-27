using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.UnityAOP.Observable;
using Assets.UnityAOP.Observable.Core;

namespace Assets.Model {
    [Observable]
    public class Player : User {
        public ObservableList<User> Friends { get; private set; }
        public Int32 Rating { get; private set; }

        public Player(int id, string name) : base(id, name) {
            Friends = new ObservableList<User>();
            Rating = 0;
        }
    }
}
