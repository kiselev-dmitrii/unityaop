using System;
using UnityEngine;

namespace Assets.UnityAOP.Utils {
	[Serializable]
	public sealed class SerializableType : ISerializationCallbackReceiver {
        [SerializeField]
        private String fullTypeName;
        public Type Type;

		public SerializableType(Type type) {
		    Type = type;
		}

	    public void OnBeforeSerialize() {
	        if (Type != null) {
	            fullTypeName = Type.FullName;
	        } else {
	            fullTypeName = "";
	        }
	    }

	    public void OnAfterDeserialize() {
            Type = Type.GetType(fullTypeName);
        }

		public static implicit operator Type(SerializableType serializableType) {
			return serializableType.Type;
		}

		public static implicit operator SerializableType(Type type) {
			return new SerializableType(type);
		}

		public override string ToString() {
            return Type != null ? Type.FullName : "Empty";
		}
	}
}
