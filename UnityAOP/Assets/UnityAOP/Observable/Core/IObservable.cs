
namespace Assets.UnityAOP.Observable.Core {
    public delegate T GetterDelegate<out T>();
    public delegate void SetterDelegate<in T>(T value);
    
    public interface IObservable {
        void AddMemberObserver(int memberCode, IObserver observer);
        void RemoveMemberObserver(int memberCode, IObserver observer);
        void NotifyMemberChanged(int memberCode);

        object GetGetterDelegate(int propertyCode);
        object GetSetterDelegate(int propertyCode);
        object GetMethodDelegate(int methodCode);
    }
    
    public interface IObservableCollection<T> {
        void AddCollectionObserver(IListObserver<T> observer);
        void RemoveCollectionObserver(IListObserver<T> observer);

        void NotifyItemInserted(int index, T item);
        void NotifyItemRemoved(int index, T item);
        void NotifyListCleared();
    }
}
