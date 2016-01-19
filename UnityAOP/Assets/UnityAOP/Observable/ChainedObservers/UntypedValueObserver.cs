using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.UnityAOP.Observable.ChainedObservers {
    public class UntypedValueObserver : BaseChainedObserver {
        public enum PropertyType {
            Bool,
            Int,
            Float,
            Double,
            String,
            Vector2,
            Vector3,
            Vector4
        }

        #region Getter/Setters
        private static readonly Dictionary<Type, PropertyType> Types = new Dictionary<Type, PropertyType>() {
            {typeof(bool), PropertyType.Bool},
            {typeof(int), PropertyType.Int},
            {typeof(float), PropertyType.Float},
            {typeof(double), PropertyType.Double},
            {typeof(string), PropertyType.String},
            {typeof(Vector2), PropertyType.Vector2},
            {typeof(Vector3), PropertyType.Vector3},
            {typeof(Vector4), PropertyType.Vector4},
        };

        private GetterDelegate<bool> boolGetter; 
        private GetterDelegate<int> intGetter;
        private GetterDelegate<float> floatGetter;
        private GetterDelegate<double> doubleGetter;
        private GetterDelegate<String> stringGetter;
        private GetterDelegate<Vector2> vector2Getter;
        private GetterDelegate<Vector3> vector3Getter;
        private GetterDelegate<Vector4> vector4Getter;

        private SetterDelegate<bool> boolSetter;
        private SetterDelegate<int> intSetter;
        private SetterDelegate<float> floatSetter;
        private SetterDelegate<double> doubleSetter;
        private SetterDelegate<String> stringSetter;
        private SetterDelegate<Vector2> vector2Setter;
        private SetterDelegate<Vector3> vector3Setter;
        private SetterDelegate<Vector4> vector4Setter;
        #endregion

        private readonly PropertyMetadata targetProperty;
        private bool isCompleteBound;
        private Action callback;

        public UntypedValueObserver(IObservable root, PropertyMetadata[] propertyPath, Action onValueChanged = null) : base(root, propertyPath) {
            targetProperty = TargetProperty;
            isCompleteBound = false;
            callback = onValueChanged;

            Bind(0);
        }

        public PropertyType GetPropertyType() {
            return Types[targetProperty.Type];
        }

        public bool GetBooleanValue() {
            if (!isCompleteBound) {
                return false;
            }

            PropertyType type = Types[targetProperty.Type];
            switch (type) {
                case PropertyType.Bool:
                    return boolGetter();
                case PropertyType.Int:
                    return intGetter() != 0;
                case PropertyType.Float:
                    return floatGetter() != 0.0f;
                case PropertyType.Double:
                    return doubleGetter() != 0.0f;
                case PropertyType.String:
                    return !String.IsNullOrEmpty(stringGetter());
                case PropertyType.Vector2:
                    return vector2Getter() != Vector2.zero;
                case PropertyType.Vector3:
                    return vector3Getter() != Vector3.zero;
                case PropertyType.Vector4:
                    return vector4Getter() != Vector4.zero;
            }

            return false;
        }

        public double GetDoubleValue() {
            if (!isCompleteBound) {
                return default(double);
            }

            PropertyType type = GetPropertyType();
            switch (type) {
                case PropertyType.Bool:
                    return boolGetter() ? 1.0f : 0.0f;
                case PropertyType.Int:
                    return intGetter();
                case PropertyType.Float:
                    return floatGetter();
                case PropertyType.Double:
                    return doubleGetter();
                case PropertyType.String:
                    double result = 0;
                    double.TryParse(stringGetter(), out result);
                    return result;
            }

            return 0;
        }

        public string GetStringValue() {
            if (!isCompleteBound) {
                return "";
            }

            PropertyType type = GetPropertyType();
            switch (type) {
                case PropertyType.Bool:
                    return boolGetter().ToString();
                case PropertyType.Int:
                    return intGetter().ToString();
                case PropertyType.Float:
                    return floatGetter().ToString();
                case PropertyType.Double:
                    return doubleGetter().ToString();
                case PropertyType.String:
                    return stringGetter();
                case PropertyType.Vector2:
                    return vector2Getter().ToString();
                case PropertyType.Vector3:
                    return vector3Getter().ToString();
                case PropertyType.Vector4:
                    return vector4Getter().ToString();
            }

            return "";
        }

        public Vector4 GetVectorValue() {
            if (!isCompleteBound) {
                return Vector4.zero;
            }

            PropertyType type = GetPropertyType();
            switch (type) {
                case PropertyType.Vector2:
                    return vector2Getter();
                case PropertyType.Vector3:
                    return vector3Getter();
                case PropertyType.Vector4:
                    return vector4Getter();
            }

            return Vector4.zero;
        }

        public void SetBooleanValue(bool value) {
            if (!isCompleteBound) {
                return;
            }

            PropertyType type = GetPropertyType();
            switch (type) {
                case PropertyType.Bool:
                    boolSetter(value);
                    break;
                case PropertyType.Int:
                    intSetter(value ? 1 : 0);
                    break;
                case PropertyType.Float:
                    floatSetter(value ? 1.0f : 0.0f);
                    break;
                case PropertyType.Double:
                    doubleSetter(value ? 1.0 : 0.0);
                    break;
                case PropertyType.String:
                    stringSetter(value.ToString());
                    break;
                case PropertyType.Vector2:
                    vector2Setter(value ? Vector2.one : Vector2.zero);
                    break;
                case PropertyType.Vector3:
                    vector3Setter(value ? Vector3.one : Vector3.zero);
                    break;
                case PropertyType.Vector4:
                    vector4Setter(value ? Vector4.one : Vector4.zero);
                    break;
            }
        }

        public void SetDoubleValue(double value) {
            if (!isCompleteBound) {
                return;
            }

            PropertyType type = GetPropertyType();
            switch (type) {
                case PropertyType.Bool:
                    boolSetter(value != 0.0);
                    break;
                case PropertyType.Int:
                    intSetter((int)value);
                    break;
                case PropertyType.Float:
                    floatSetter((float)value);
                    break;
                case PropertyType.Double:
                    doubleSetter(value);
                    break;
                case PropertyType.String:
                    stringSetter(value.ToString());
                    break;
            }
        }

        public void SetStringValue(String value) {
            if (!isCompleteBound) {
                return;
            }

            PropertyType type = GetPropertyType();
            switch (type) {
                case PropertyType.Bool:
                    boolSetter(!String.IsNullOrEmpty(value));
                    break;
                case PropertyType.Int:
                    int intValue = 0;
                    int.TryParse(value, out intValue);
                    intSetter(intValue);
                    break;
                case PropertyType.Float:
                    float floatValue = 0.0f;
                    float.TryParse(value, out floatValue);
                    floatSetter(floatValue);
                    break;
                case PropertyType.Double:
                    double doubleValue = 0.0;
                    double.TryParse(value, out doubleValue);
                    doubleSetter(doubleValue);
                    break;
                case PropertyType.String:
                    stringSetter(value);
                    break;
            }
        }

        protected override void BindTarget(IObservable parent, PropertyMetadata targetMeta) {
            PropertyType type = GetPropertyType();
            switch (type) {
                case PropertyType.Bool:
                    boolGetter = (GetterDelegate<bool>) parent.GetGetterDelegate(targetProperty.Index);
                    boolSetter = (SetterDelegate<bool>) parent.GetSetterDelegate(targetProperty.Index);
                    break;
                case PropertyType.Int:
                    intGetter = (GetterDelegate<int>) parent.GetGetterDelegate(targetProperty.Index);
                    intSetter = (SetterDelegate<int>) parent.GetSetterDelegate(targetProperty.Index);
                    break;
                case PropertyType.Float:
                    floatGetter = (GetterDelegate<float>) parent.GetGetterDelegate(targetProperty.Index);
                    floatSetter = (SetterDelegate<float>)parent.GetSetterDelegate(targetProperty.Index);
                    break;
                case PropertyType.Double:
                    doubleGetter = (GetterDelegate<double>) parent.GetGetterDelegate(targetProperty.Index);
                    doubleSetter = (SetterDelegate<double>)parent.GetSetterDelegate(targetProperty.Index);
                    break;
                case PropertyType.String:
                    stringGetter = (GetterDelegate<string>) parent.GetGetterDelegate(targetProperty.Index);
                    stringSetter = (SetterDelegate<string>)parent.GetSetterDelegate(targetProperty.Index);
                    break;
                case PropertyType.Vector2:
                    vector2Getter = (GetterDelegate<Vector2>) parent.GetGetterDelegate(targetProperty.Index);
                    vector2Setter = (SetterDelegate<Vector2>)parent.GetSetterDelegate(targetProperty.Index);
                    break;
                case PropertyType.Vector3:
                    vector3Getter = (GetterDelegate<Vector3>) parent.GetGetterDelegate(targetProperty.Index);
                    vector3Setter = (SetterDelegate<Vector3>)parent.GetSetterDelegate(targetProperty.Index);
                    break;
                case PropertyType.Vector4:
                    vector4Getter = (GetterDelegate<Vector4>) parent.GetGetterDelegate(targetProperty.Index);
                    vector4Setter = (SetterDelegate<Vector4>)parent.GetSetterDelegate(targetProperty.Index);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unsupported format");
            }

            isCompleteBound = true;
        }

        protected override void UnbindTarget() {
            isCompleteBound = false;
        }

        protected override void OnParentNodeChanged() {
            if (callback != null) {
                callback();
            }
        }

        public override void Dispose() {
            base.Dispose();
            callback = null;
        }
    }
}
