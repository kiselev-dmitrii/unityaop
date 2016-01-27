using System;
using System.Collections.Generic;
using Assets.UnityAOP.Observable.CodeObjectModel;
using Assets.UnityAOP.Observable.Core;

namespace Assets.UnityAOP.Documentation {

    public class Application : IObservable {
        public ObservableImplementation ObservableImpl;
        public Dictionary<int, Object> Getters;
        public Dictionary<int, Object> Setters;
        public Dictionary<int, Object> Methods; 
    
        private User player;
        public User Player {
            get { return player; }
            set {
                player = value; 
                NotifyMemberChanged(-1901885695);
            }
        }
    
        public Application() {
            ObservableImpl = new ObservableImplementation();
            Getters = new Dictionary<int, Object>();
            Setters = new Dictionary<int, Object>();
            Methods = new Dictionary<int, Object>();
    
            Getters[-1901885695] = new GetterDelegate<IObservable>(delegate { return Player; });
            Setters[-1901885695] = new SetterDelegate<IObservable>(delegate(IObservable value) { Player = (User)value; });
    
            Player = new Player(1, "Player", "http://vk.com/id0/avatar.jpg");
        }
    
        public virtual void AddMemberObserver(int memberCode, IObserver observer) {
            ObservableImpl.AddObserver(memberCode, observer);
        }
    
        public virtual void RemoveMemberObserver(int memberCode, IObserver observer) {
            ObservableImpl.RemoveObserver(memberCode, observer);
        }
    
        public virtual void NotifyMemberChanged(int memberCode) {
            ObservableImpl.NotifyPropertyChanged(this, memberCode);
        }
    
        public virtual object GetGetterDelegate(int propertyCode) {
            return Getters[propertyCode];
        }
    
        public virtual object GetSetterDelegate(int propertyCode) {
            return Setters[propertyCode];
        }
    
        public object GetMethodDelegate(int methodCode) {
            return Methods[methodCode];
        }
    }

    public class User : IObservable {
        public ObservableImplementation ObservableImpl;
        public Dictionary<int, Object> Getters;
        public Dictionary<int, Object> Setters;
        public Dictionary<int, Object> Methods; 
    
        private int id;
        public Int32 Id {
            get { return id; }
            set {
                id = value;
                NotifyMemberChanged(2363);
            }
        }

        private string name;
        public String Name {
            get { return name; }
            set {
                name = value;
                NotifyMemberChanged(2420395);
            }
        }

        private string avatar;
        public String Avatar {
            get { return avatar; }
            set {
                avatar = value;
                NotifyMemberChanged(1972874617);
            }
        }

        private Group group;
        public Group Group {
            get { return group; }
            set {
                group = value;
                NotifyMemberChanged(69076575);
            }
        }
    
        public User(int id, string name, string avatar) {
            ObservableImpl = new ObservableImplementation();
            Getters = new Dictionary<int, Object>();
            Setters = new Dictionary<int, Object>();
            Methods = new Dictionary<int, Object>();

            Getters[2363] = new GetterDelegate<Int32>(delegate { return Id; });
            Getters[2420395] = new GetterDelegate<String>(delegate { return Name; });
            Getters[1972874617] = new GetterDelegate<String>(delegate { return Avatar; });
            Getters[69076575] = new GetterDelegate<IObservable>(delegate { return Group; });
    
            Setters[2363] = new SetterDelegate<Int32>(delegate(Int32 value) { Id = value; });
            Setters[2420395] = new SetterDelegate<String>(delegate(String value) { Name = value; });
            Setters[1972874617] = new SetterDelegate<String>(delegate(String value) { Avatar = value; });
            Setters[69076575] = new SetterDelegate<IObservable>(delegate(IObservable value) { Group = (Group)value; });
    
            Id = id;
            Name = name;
            Avatar = avatar;
            Group = new Group(id);
        }
    
        public virtual void AddMemberObserver(int memberCode, IObserver observer) {
            ObservableImpl.AddObserver(memberCode, observer);
        }
    
        public virtual void RemoveMemberObserver(int memberCode, IObserver observer) {
            ObservableImpl.RemoveObserver(memberCode, observer);
        }
    
        public virtual void NotifyMemberChanged(int memberCode) {
            ObservableImpl.NotifyPropertyChanged(this, memberCode);
        }
    
        public virtual object GetGetterDelegate(int propertyCode) {
            return Getters[propertyCode];
        }
    
        public virtual object GetSetterDelegate(int propertyCode) {
            return Setters[propertyCode];
        }

        public object GetMethodDelegate(int methodCode) {
            return Methods[methodCode];
        }
    }

    public class Player : User {
        private int rating;
        public int Rating {
            get { return rating; }
            private set {
                rating = value;
                NotifyMemberChanged(-1854235203);
            }
        }

        public Player(int id, string name, string avatar) : base(id, name, avatar) {
            Getters[-1854235203] = new GetterDelegate<Int32>(delegate { return Rating; });
            Setters[-1854235203] = new SetterDelegate<Int32>(delegate(Int32 value) { Rating = value; });
            Methods[-713773409] = new Action(IncreaseRating);

            Rating = 100;
        }

        public void IncreaseRating() {
            Rating++;
        }
    }

    public class Group : IObservable {
        public ObservableImplementation ObservableImpl;
        public Dictionary<int, Object> Getters;
        public Dictionary<int, Object> Setters;
        public Dictionary<int, Object> Methods; 

        private int id;
        public Int32 Id {
            get { return id; }
            set {
                id = value;
                NotifyMemberChanged(2363);
            }
        }

        private int numMembers;
        public Int32 NumMembers {
            get { return numMembers; }
            set {
                numMembers = value;
                NotifyMemberChanged(139374291);
            }
        }

        private int numReadyMembers;
        public Int32 NumReadyMembers {
            get { return numReadyMembers; }
            set {
                numReadyMembers = value;
                NotifyMemberChanged(-1914201988);
            }
        }
    
        public Group(int id) {
            ObservableImpl = new ObservableImplementation();
            Getters = new Dictionary<int, Object>();
            Setters = new Dictionary<int, Object>();
            Methods = new Dictionary<int, Object>();
    
            Getters[2363] = new GetterDelegate<Int32>(delegate { return Id; });
            Getters[139374291] = new GetterDelegate<Int32>(delegate { return NumMembers; });
            Getters[-1914201988] = new GetterDelegate<Int32>(delegate { return NumReadyMembers; });
    
            Setters[2363] = new SetterDelegate<Int32>(delegate(Int32 value) { Id = value; });
            Setters[139374291] = new SetterDelegate<Int32>(delegate(Int32 value) { NumMembers = value; });
            Setters[-1914201988] = new SetterDelegate<Int32>(delegate(Int32 value) { NumReadyMembers = value; });

            Methods[1819038331] = new Action<User>(AddMember);
            Methods[-874885026] = new Action<User>(RemoveMember);

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

        public virtual void AddMemberObserver(int memberCode, IObserver observer) {
            ObservableImpl.AddObserver(memberCode, observer);
        }

        public virtual void RemoveMemberObserver(int memberCode, IObserver observer) {
            ObservableImpl.RemoveObserver(memberCode, observer);
        }

        public virtual void NotifyMemberChanged(int memberCode) {
            ObservableImpl.NotifyPropertyChanged(this, memberCode);
        }

        public virtual object GetGetterDelegate(int propertyCode) {
            return Getters[propertyCode];
        }

        public virtual object GetSetterDelegate(int propertyCode) {
            return Setters[propertyCode];
        }

        public object GetMethodDelegate(int methodCode) {
            return Methods[methodCode];
        }
    }
}
