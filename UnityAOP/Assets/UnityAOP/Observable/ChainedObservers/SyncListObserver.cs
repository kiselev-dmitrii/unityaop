using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.UnityAOP.Observable.CodeObjectModel;
using Assets.UnityAOP.Observable.Core;

namespace Assets.UnityAOP.Observable.ChainedObservers {
    public class SyncListObserver<TSource, TDestination> : BaseListObserver<TSource> where TSource : class {
        public delegate TDestination ConstructorFunc(int index, TSource srcItem);

        private readonly IList<TDestination> dstList;
        private readonly ConstructorFunc constructorFunc;

        public SyncListObserver(IObservable root, PropertyMetadata[] propertyPath, IList<TDestination> destination, ConstructorFunc constructor) : base(root, propertyPath) {
            dstList = destination;
            constructorFunc = constructor;
        }

        public override void OnListCleared() {
            dstList.Clear();
        }

        public override void OnItemInserted(int index, TSource item) {
            TDestination dstItem = constructorFunc(index, item);
            dstList.Insert(index, dstItem);
        }

        public override void OnItemRemoved(int index, TSource item) {
            dstList.RemoveAt(index);
        }
    }
}
