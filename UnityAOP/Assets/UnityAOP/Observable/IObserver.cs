﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.UnityAOP.Observable {
public interface IObserver {
    void OnNodeChanged(object parent, int index);
}
}