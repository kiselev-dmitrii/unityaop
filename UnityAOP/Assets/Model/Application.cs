using Assets.UnityAOP.Attributes;

namespace Assets.Model {
    [Observable]
    public class Application {
        public User Player { get; set; }

        public Application() {
            Player = new Player(1, "Player");
        }
    }
}
