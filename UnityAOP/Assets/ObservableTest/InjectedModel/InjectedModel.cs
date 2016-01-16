using System;
using System.Collections.Generic;
using Assets.UnityAOP.Attributes;
using Assets.UnityAOP.Observable;

namespace Assets.ObservableTest.InjectedModel {
    public class Application : IObservable {
        public ObservableImpl ObservableImpl;
        public TypeMetadata Metadata;
        public List<Object> Getters;
        public List<Object> Setters;

        public User Player { get; private set; }

        public Application() {
            ObservableImpl = new ObservableImpl();
            Metadata = ObservableMetadata.GetTypeMetadata("Application");
            Getters = new List<object>();
            Setters = new List<object>();

            ObservableImpl.SetNumProperties(1);

            Getters.Add(new GetterDelegate<IObservable>(delegate { return Player; }));

            Setters.Add(new SetterDelegate<IObservable>(delegate(IObservable value) { Player = (User)value; }));

            Player = new User(1, "Player", "http://vk.com/id0/avatar.jpg");
        }

        public virtual PropertyMetadata GetPropertyMetadata(string property) {
            return Metadata.GetPropertyMetadata(property);
        }

        public virtual void AddObserver(int fieldIndex, IObserver observer) {
            ObservableImpl.AddObserver(fieldIndex, observer);
        }

        public virtual void RemoveObserver(int fieldIndex, IObserver observer) {
            ObservableImpl.RemoveObserver(fieldIndex, observer);
        }

        public virtual void NotifyPropertyChanged(int fieldIndex) {
            ObservableImpl.NotifyPropertyChanged(this, fieldIndex);
        }

        public virtual object GetGetterDelegate(int propertyIndex) {
            return Getters[propertyIndex];
        }

        public virtual object GetSetterDelegate(int propertyIndex) {
            return Setters[propertyIndex];
        }
    }

    public class User : IObservable {
        public ObservableImpl ObservableImpl;
        public TypeMetadata Metadata;
        public List<Object> Getters;
        public List<Object> Setters;

        public Int32 Id { get; set; }
        public String Name { get; set; }
        public String Avatar { get; set; }
        public Group Group { get; set; }

        public User(int id, string name, string avatar) {
            ObservableImpl = new ObservableImpl();
            Metadata = ObservableMetadata.GetTypeMetadata("User");
            Getters = new List<object>();
            Setters = new List<object>();

            ObservableImpl.SetNumProperties(4);

            Getters.Add(new GetterDelegate<Int32>(delegate { return Id; }));
            Getters.Add(new GetterDelegate<String>(delegate { return Name; }));
            Getters.Add(new GetterDelegate<String>(delegate { return Avatar; }));
            Getters.Add(new GetterDelegate<IObservable>(delegate { return Group; }));

            Setters.Add(new SetterDelegate<Int32>(delegate(Int32 value) { Id = value; }));
            Setters.Add(new SetterDelegate<String>(delegate(String value) { Name = value; }));
            Setters.Add(new SetterDelegate<String>(delegate(String value) { Avatar = value; }));
            Setters.Add(new SetterDelegate<IObservable>(delegate(IObservable value) { Group = (Group)value; }));

            Id = id;
            Name = name;
            Avatar = avatar;
            Group = new Group(id);
        }

        public virtual PropertyMetadata GetPropertyMetadata(string property) {
            return Metadata.GetPropertyMetadata(property);
        }

        public virtual void AddObserver(int fieldIndex, IObserver observer) {
            ObservableImpl.AddObserver(fieldIndex, observer);
        }

        public virtual void RemoveObserver(int fieldIndex, IObserver observer) {
            ObservableImpl.RemoveObserver(fieldIndex, observer);
        }

        public virtual void NotifyPropertyChanged(int fieldIndex) {
            ObservableImpl.NotifyPropertyChanged(this, fieldIndex);
        }

        public virtual object GetGetterDelegate(int propertyIndex) {
            return Getters[propertyIndex];
        }

        public virtual object GetSetterDelegate(int propertyIndex) {
            return Setters[propertyIndex];
        }
    }

    public class Group : IObservable {
        public ObservableImpl ObservableImpl;
        public TypeMetadata Metadata;
        public List<Object> Getters;
        public List<Object> Setters;

        public Int32 Id { get; set; }
        public Int32 NumMembers { get; set; }
        public Int32 NumReadyMembers { get; set; }

        public Group(int id) {
            ObservableImpl = new ObservableImpl();
            Metadata = ObservableMetadata.GetTypeMetadata("Group");

            ObservableImpl.SetNumProperties(3);

            Getters.Add(new GetterDelegate<Int32>(delegate { return Id; }));
            Getters.Add(new GetterDelegate<Int32>(delegate { return NumMembers; }));
            Getters.Add(new GetterDelegate<Int32>(delegate { return NumReadyMembers; }));

            Setters.Add(new SetterDelegate<Int32>(delegate(Int32 value) { Id = value; }));
            Setters.Add(new SetterDelegate<Int32>(delegate(Int32 value) { NumMembers = value; }));
            Setters.Add(new SetterDelegate<Int32>(delegate(Int32 value) { NumReadyMembers = value; }));

            Id = id;
            NumMembers = 0;
            NumReadyMembers = 0;
        }

        public void AddMember(User member) {
            NumMembers++;
        }

        public void RemoveMember(User member) {
            NumMembers--;
        }

        public virtual PropertyMetadata GetPropertyMetadata(string property) {
            return Metadata.GetPropertyMetadata(property);
        }

        public virtual void AddObserver(int fieldIndex, IObserver observer) {
            ObservableImpl.AddObserver(fieldIndex, observer);
        }

        public virtual void RemoveObserver(int fieldIndex, IObserver observer) {
            ObservableImpl.RemoveObserver(fieldIndex, observer);
        }

        public virtual void NotifyPropertyChanged(int fieldIndex) {
            ObservableImpl.NotifyPropertyChanged(this, fieldIndex);
        }

        public virtual object GetGetterDelegate(int propertyIndex) {
            return Getters[propertyIndex];
        }

        public virtual object GetSetterDelegate(int propertyIndex) {
            return Setters[propertyIndex];
        }
    }
}
