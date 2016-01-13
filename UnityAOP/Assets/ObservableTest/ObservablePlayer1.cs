using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Example1;
using Assets.UnityAOP.Observable;

namespace Assets.ObservableTest {
public class ObservablePlayer1 : IObservable {
    private ObservableImpl<ObservablePlayer1> observableImpl = new ObservableImpl<ObservablePlayer1>(new [] {"Id", "Name", "Warrior"});

    private Int32 id;
    public Int32 Id {
        get { return id; }
        private set {
            id = value;
            NotifyPropertyChanged(0);
        }
    }

    private String name;
    public String Name {
        get { return name; }
        private set {
            name = value;
            NotifyPropertyChanged(1);
        }
    }

    private Warrior warrior;
    public Warrior Warrior {
        get { return warrior; }
        private set {
            warrior = value;
            NotifyPropertyChanged(3);
        }
    }

    /*
    public object[] Getters;
    public object[] Setters;
     */

    public ObservablePlayer1() {
        /*
        Getters = new object[] {
            new GetterDelegate<int>(delegate { return Id; }),
            new GetterDelegate<String>(delegate { return Name; }),
            new GetterDelegate<Warrior>(delegate { return Warrior; })
        };
        Setters = new object[] {
            new SetterDelegate<int>(delegate(int value) { Id = value; }),
            new SetterDelegate<String>(delegate(String value) { Name = value; }),
            new SetterDelegate<Warrior>(delegate(Warrior value) { Warrior = value; }),
        };
        */
        Id = 0;
        Name = "Unknown";
    }

    public int GetIndexOfProperty(string property) {
        return observableImpl.GetIndexOfProperty(property);
    }

    public void AddObserver(int index, IObserver observer) {
        observableImpl.AddObserver(index, observer);
    }

    public void RemoveObserver(int index, IObserver observer) {
        observableImpl.RemoveObserver(index, observer);
    }

    /*
    public GetterDelegate<T> GetGetterDelegate<T>(int propertyIndex) {
        return (GetterDelegate<T>)Getters[propertyIndex];
    }

    public SetterDelegate<T> GetSetterDelegate<T>(int propertyIndex) {
        return (SetterDelegate<T>)Setters[propertyIndex];
    }
     */

    public void NotifyPropertyChanged(int index) {
        observableImpl.NotifyPropertyChanged(this, index);
    }
}
}
