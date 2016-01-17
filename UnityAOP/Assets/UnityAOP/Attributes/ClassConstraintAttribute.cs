using System;
using UnityEngine;

namespace Assets.UnityAOP.Attributes {
	public enum ClassGrouping {
		None,
		ByNamespace,
		ByNamespaceFlat,
		ByAddComponentMenu,
	}

	public abstract class ClassConstraintAttribute : PropertyAttribute {

		private ClassGrouping _grouping = ClassGrouping.ByNamespaceFlat;
		private bool _allowAbstract = false;

		public ClassGrouping Grouping {
			get { return _grouping; }
			set { _grouping = value; }
		}

		public bool AllowAbstract {
			get { return _allowAbstract; }
			set { _allowAbstract = value; }
		}

		public virtual bool IsConstraintSatisfied(Type type) {
			return AllowAbstract || !type.IsAbstract;
		}

	}


	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class ClassExtendsAttribute : ClassConstraintAttribute {
		public ClassExtendsAttribute() {
		}

		public ClassExtendsAttribute(Type baseType) {
			BaseType = baseType;
		}

		public Type BaseType { get; private set; }

		public override bool IsConstraintSatisfied(Type type) {
			return base.IsConstraintSatisfied(type)
				&& BaseType.IsAssignableFrom(type) && type != BaseType;
		}

	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class ClassImplementsAttribute : ClassConstraintAttribute {
		public ClassImplementsAttribute() {
		}

		public ClassImplementsAttribute(Type interfaceType) {
			InterfaceType = interfaceType;
		}

		public Type InterfaceType { get; private set; }

		public override bool IsConstraintSatisfied(Type type) {
			if (base.IsConstraintSatisfied(type)) {
				foreach (var interfaceType in type.GetInterfaces())
					if (interfaceType == InterfaceType)
						return true;
			}
			return false;
		}

	}

}
