using System;

namespace Assets.UnityAOP.Observable.CodeObjectModel {
    public enum MemberType {
        Method,
        Property
    }

    public class MemberMetadata {
        public String Name { get; private set; }
        public Type Type { get; private set; }
        public MemberType MemberType { get; private set; }
        public int Code { get; private set; }

        public MemberMetadata(String name, Type type, MemberType memberType) {
            Name = name;
            Type = type;
            MemberType = memberType;
            Code = name.GetHashCode();
        }

        public MemberMetadata(String name, Type type, int code, MemberType memberType) {
            Name = name;
            Type = type;
            MemberType = memberType;
            Code = code;
        }

        public override string ToString() {
            return String.Format("{0} {1}", Type.Name, Name);
        }
    }
}