
namespace Assets.UnityAOP.Observable {
    public delegate T GetterDelegate<out T>();
    public delegate void SetterDelegate<in T>(T value);
    
    public interface IObservable {
        void AddObserver(int fieldIndex, IObserver observer);
        void RemoveObserver(int fieldIndex, IObserver observer);
        void NotifyPropertyChanged(int fieldIndex);
        object GetGetterDelegate(int propertyIndex);
        object GetSetterDelegate(int propertyIndex);
    }
    
    public interface IObservableCollection {
        
    }
}
