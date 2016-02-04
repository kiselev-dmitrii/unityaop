using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.UnityAOP.Observable.ChainedObservers;
using Assets.UnityAOP.Observable.Core;

namespace Assets.Samples.BindingSamples.SimpleWindow {
    [Observable]
    public class SimpleWindow {
        public String Username { get; private set; }
        public String Password { get; private set; }
        public bool IsValid { get; private set; }
        public bool IsLoggedIn { get; private set; }

        public SimpleWindow() {
            this.ObserveProperty(x => x.IsValid, OnUsernameChanged);
        }

        public void OnLoginButtonClick() {
            if (IsValid) {
                IsLoggedIn = true;
            }
        }

        public void OnUsernameChanged() {
            IsValid = Username.Length > 0 && char.IsLetter(Username[0]);
        }
    }
}
