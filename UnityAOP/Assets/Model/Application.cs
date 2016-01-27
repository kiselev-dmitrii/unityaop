using Assets.UnityAOP.Observable;
using Assets.UnityAOP.Observable.Core;

namespace Assets.Model {
    [Observable]
    public class Application {
        public User Player { get; set; }

        public Application() {
            Player = new Player(1, "Player");
        }
    }
}
