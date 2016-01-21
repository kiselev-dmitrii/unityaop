using System;
using UnityEngine;

namespace Assets.UnityAOP.Utils {
	public enum ClassGrouping {
		None,
		ByNamespace,
		ByNamespaceFlat,
		ByAddComponentMenu,
	}

	public abstract class ClassConstraintAttribute : PropertyAttribute {
		private ClassGrouping grouping = ClassGrouping.ByNamespaceFlat;
		private bool allowAbstract = false;

		public ClassGrouping Grouping {
			get { return grouping; }
			set { grouping = value; }
		}

		public bool AllowAbstract {
			get { return allowAbstract; }
			set { allowAbstract = value; }
		}

		public virtual bool IsConstraintSatisfied(Type type) {
			return AllowAbstract || !type.IsAbstract;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class ClassExtendsAttribute : ClassConstraintAttribute {
        public Type BaseType { get; private set; }

		public ClassExtendsAttribute(Type baseType) {
			BaseType = baseType;
		}

		public override bool IsConstraintSatisfied(Type type) {
			return base.IsConstraintSatisfied(type)
				&& BaseType.IsAssignableFrom(type) && type != BaseType;
		}

	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class ClassImplementsAttribute : ClassConstraintAttribute {
        public Type InterfaceType { get; private set; }

		public ClassImplementsAttribute(Type interfaceType) {
			InterfaceType = interfaceType;
		}

		public override bool IsConstraintSatisfied(Type type) {
			if (base.IsConstraintSatisfied(type)) {
				foreach (var interfaceType in type.GetInterfaces())
					if (interfaceType == InterfaceType)
						return true;
			}
			return false;
		}
	}

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ClassHasAttributeAttribute : ClassConstraintAttribute {
        public Type AttributeType { get; private set; }

        public ClassHasAttributeAttribute(Type attributeType) {
            AttributeType = attributeType;
        }


        public override bool IsConstraintSatisfied(Type type) {
            return base.IsConstraintSatisfied(type) &&
                   type.HasAttribute(AttributeType);
        }
    }

}
