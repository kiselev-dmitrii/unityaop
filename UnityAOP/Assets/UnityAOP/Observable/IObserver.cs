namespace Assets.UnityAOP.Observable {
    public interface IObserver {
        void OnNodeChanged(IObservable parent, int index);
    }
}
