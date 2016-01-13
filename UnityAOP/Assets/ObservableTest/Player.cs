using System;
using Assets.UnityAOP.Attributes;

namespace Assets.ObservableTest {
[Observable]
public class Player {
    public Int32 Id { get; private set; }
    public String Name { get; private set; }

    public Player() {
        Id = 0;
        Name = "Unknown";
    }
}
}
