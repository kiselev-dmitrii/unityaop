using System;
using UnityEngine;

namespace Assets.UnityAOP.Utils {
	[Serializable]
	public sealed class SerializableType : ISerializationCallbackReceiver {
        [SerializeField]
        private string _classRef;
        private Type _type;

        public Type Type {
            get { return _type; }
            set {
                if (value != null && !value.IsClass)
                    throw new ArgumentException(string.Format("'{0}' is not a class type.", value.FullName), "value");

                _type = value;
                _classRef = GetClassRef(value);
            }
        }

		public static string GetClassRef(Type type) {
			return type != null
				? type.FullName + ", " + type.Assembly.GetName().Name
				: "";
		}

		public SerializableType() {
		}

		public SerializableType(string assemblyQualifiedClassName) {
			Type = !string.IsNullOrEmpty(assemblyQualifiedClassName)
				? Type.GetType(assemblyQualifiedClassName)
				: null;
		}

		public SerializableType(Type type) {
			Type = type;
		}

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            if (!string.IsNullOrEmpty(_classRef)) {
                _type = System.Type.GetType(_classRef);

                if (_type == null)
                    Debug.LogWarning(string.Format("'{0}' was referenced but class type was not found.", _classRef));
            } else {
                _type = null;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
        }

		public static implicit operator string(SerializableType typeReference) {
			return typeReference._classRef;
		}

		public static implicit operator Type(SerializableType typeReference) {
			return typeReference.Type;
		}

		public static implicit operator SerializableType(Type type) {
			return new SerializableType(type);
		}

		public override string ToString() {
			return Type != null ? Type.FullName : "(None)";
		}

	}

}
