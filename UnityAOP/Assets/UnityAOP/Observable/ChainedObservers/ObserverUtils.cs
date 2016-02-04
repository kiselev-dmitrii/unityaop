using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Assets.UnityAOP.Observable.CodeObjectModel;
using Assets.UnityAOP.Observable.Core;
using UnityEngine.Assertions;

namespace Assets.UnityAOP.Observable.ChainedObservers {
    public static class ObserverUtils {
        #region Observe Property
        public static ChainedPropertyObserver<TTarget> ObserveProperty<TTarget>(this object root, String path, Action onChanged = null) {
            var props = CalculatePropertyPath(root.GetType(), path);
            return new ChainedPropertyObserver<TTarget>((IObservable)root, props, onChanged);
        }

        public static UntypedPropertyObserver ObserveProperty(this object root, String path, Action onChanged = null) {
            var props = CalculatePropertyPath(root.GetType(), path);
            return new UntypedPropertyObserver((IObservable)root, props, onChanged);
        }

        public static ChainedPropertyObserver<TTarget> ObserveProperty<TTarget>(this object root, String[] path, Action onChanged = null) {
            var props = CalculatePropertyPath(root.GetType(), path);
            return new ChainedPropertyObserver<TTarget>((IObservable)root, props, onChanged);
        }

        public static UntypedPropertyObserver ObserveProperty(this object root, String[] path, Action onChanged = null) {
            var props = CalculatePropertyPath(root.GetType(), path);
            return new UntypedPropertyObserver((IObservable)root, props, onChanged);
        }

        public static ChainedPropertyObserver<TTarget> ObserveProperty<TRoot, TTarget>(this TRoot root,
            Expression<Func<TRoot, TTarget>> expression, Action onChanged) {
    
            String path = expression.ToString().Replace("get_Item(", "").Replace(")", "");
            String[] memberNames = path.Split('.').Skip(1).ToArray();
            var props = CalculatePropertyPath(root.GetType(), memberNames);
            return new ChainedPropertyObserver<TTarget>((IObservable)root, props, onChanged);
        }
        #endregion

        #region Observe List
        public static UntypedListObserver ObserveList(this object root, String path) {
            var props = CalculatePropertyPath(root.GetType(), path);
            return new UntypedListObserver((IObservable)root, props);
        }

        public static UntypedListObserver ObserveList(this object root, String[] path) {
            var props = CalculatePropertyPath(root.GetType(), path);
            return new UntypedListObserver((IObservable)root, props);
        }

        public static SyncListObserver<TSource, TDestination> SyncList<TRoot, TSource, TDestination>(
            this TRoot root,
            Expression<Func<TRoot, IList<TSource>>> expression,
            IList<TDestination> destination,
            SyncListObserver<TSource, TDestination>.ConstructorFunc constructor) where TSource : class 
        {
            var path = expression.ToString().Replace("get_Item(", "").Replace(")", "");
            var fieldsName = path.Split('.').Skip(1).ToArray();
            var fullPath = CalculatePropertyPath(root.GetType(), fieldsName);
            return new SyncListObserver<TSource, TDestination>((IObservable)root, fullPath, destination, constructor);
        }
        #endregion

        #region Utils
        private static PropertyMetadata[] CalculatePropertyPath(Type rootType, String path) {
            String[] fieldsName = path.Split('.');
            return CalculatePropertyPath(rootType, fieldsName);
        }

        private static PropertyMetadata[] CalculatePropertyPath(Type rootType, String[] propertyNames) {
            PropertyMetadata[] result = new PropertyMetadata[propertyNames.Length];
    
            TypeMetadata typeMeta = CodeModel.GetType(rootType);
            for (int i = 0; i < propertyNames.Length; ++i) {
                var propertyName = propertyNames[i];
                var propertyMeta = typeMeta.GetProperty(propertyName);
                Assert.IsTrue(propertyMeta != null, "Property not found");
    
                result[i] = propertyMeta;
    
                if (i != propertyNames.Length - 1) {
                    if (propertyMeta.IsCollection) {
                        ++i;
                        propertyName = propertyNames[i];
                        propertyMeta = new PropertyMetadata(propertyName, propertyMeta.ItemType, int.Parse(propertyName));
                        result[i] = propertyMeta;  
                    }
                    typeMeta = CodeModel.GetType(propertyMeta.Type);
                }
            }
    
            return result;
        }
        #endregion
    }
}
