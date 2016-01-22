﻿using Assets.UnityAOP.Observable.Binding;

namespace Assets.Model {
    class ApplicationContext : RootNode {
        private Application application;

        public override void Initialize() {
            application = new Application();

            SetRoot(application);
        }
    }
}